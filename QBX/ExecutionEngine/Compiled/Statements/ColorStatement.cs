using QBX.ExecutionEngine.Execution;
using QBX.Firmware;

using static QBX.Hardware.GraphicsArray;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class ColorStatement(CodeModel.Statements.Statement? source) : Executable(source)
{
	public Evaluable? Argument1Expression;
	public Evaluable? Argument2Expression;
	public Evaluable? Argument3Expression;

	public override void Execute(ExecutionContext context, StackFrame stackFrame)
	{
		// COLOR modes:
		//
		// - In text modes,
		//   * Argument 1 changes the foreground attribute used for new character writes
		//   * Argument 2 changes the background attribute used for new character writes
		//   * Argument 3 changes the overscan attribute
		//
		// - In CGA 2bpp modes (that use shift interleave, I think?),
		//   * Argument 1 changes the palette mapping for attribute 0
		//   * Argument 2 sets CGA palette
		//
		// - In EGA modes (but not VGA),
		//   * Argument 1 changes which attribute is used for drawing operations
		//   * Argument 2 changes the palette mapping for attribute 0
		//
		// - In VGA modes (0x12 and 0x13),
		//   * Argument 1 changes which attribute is used for drawing operations
		//
		// All arguments are optional.
		if (!context.Machine.GraphicsArray.Graphics.DisableText)
		{
			// Text mode
			if (context.VisualLibrary is TextLibrary library)
			{
				if ((Argument1Expression != null) || (Argument2Expression != null))
				{
					bool blink = ((library.Attributes & 0x80) != 0);

					int foregroundColour = library.Attributes & 0x0F;
					int backgroundColour = (library.Attributes & 0x70) >> 4;

					if (Argument1Expression != null)
					{
						foregroundColour = Argument1Expression.EvaluateAndCoerceToInt(context, stackFrame);

						if (foregroundColour != (foregroundColour & 31))
							throw RuntimeException.IllegalFunctionCall(Argument1Expression.Source);

						blink = (foregroundColour & 16) != 0;

						foregroundColour &= 15;
					}

					if (Argument2Expression != null)
					{
						backgroundColour = Argument2Expression.EvaluateAndCoerceToInt(context, stackFrame);

						if (backgroundColour != (backgroundColour & 15))
							throw RuntimeException.IllegalFunctionCall(Argument2Expression.Source);

						backgroundColour &= 7;
					}

					library.SetAttributes(
						foregroundColour,
						backgroundColour | (blink ? 8 : 0));
				}

				if (Argument3Expression != null)
				{
					int newOverscanAttribute = Argument3Expression.EvaluateAndCoerceToInt(context, stackFrame) & 15;

					context.Machine.InPort(InputStatusRegisters.InputStatus1Port);
					context.Machine.OutPort(AttributeControllerRegisters.IndexAndDataWritePort, AttributeControllerRegisters.OverscanPaletteIndex);
					context.Machine.OutPort(AttributeControllerRegisters.IndexAndDataWritePort, unchecked((byte)newOverscanAttribute));
					context.Machine.OutPort(AttributeControllerRegisters.IndexAndDataWritePort, AttributeControllerRegisters.Index_PaletteAddressSourceBit);
				}
			}
		}
		else if (context.Machine.GraphicsArray.Graphics.ShiftInterleave)
		{
			// CGA modes
			if (Argument1Expression != null)
			{
				int newBackgroundAttribute = Argument1Expression.EvaluateAndCoerceToInt(context, stackFrame);

				if ((newBackgroundAttribute < 0) || (newBackgroundAttribute > 255))
					throw RuntimeException.IllegalFunctionCall(Argument1Expression.Source);

				newBackgroundAttribute &= 15;

				context.Machine.InPort(InputStatusRegisters.InputStatus1Port);
				context.Machine.OutPort(AttributeControllerRegisters.IndexAndDataWritePort, 0);
				context.Machine.OutPort(AttributeControllerRegisters.IndexAndDataWritePort, unchecked((byte)newBackgroundAttribute));
				context.Machine.OutPort(AttributeControllerRegisters.IndexAndDataWritePort, AttributeControllerRegisters.Index_PaletteAddressSourceBit);
			}

			if (Argument2Expression != null)
			{
				int newCGAPalette = Argument2Expression.EvaluateAndCoerceToInt(context, stackFrame);

				if ((newCGAPalette < 0) || (newCGAPalette > 255))
					throw RuntimeException.IllegalFunctionCall(Argument2Expression.Source);

				newCGAPalette &= 1;

				context.Machine.VideoFirmware.LoadCGAPalette(newCGAPalette, intensity: false);
			}

			// argument 3 is ignored
		}
		else
		{
			if (context.VisualLibrary is GraphicsLibrary graphicsLibrary)
			{
				if (Argument3Expression != null)
					throw RuntimeException.IllegalFunctionCall(Argument3Expression.Source);

				if ((Argument2Expression != null) && !context.RuntimeState.EnablePaletteRemapping)
					throw RuntimeException.IllegalFunctionCall(Argument2Expression.Source);

				if (Argument1Expression != null)
				{
					int newAttribute = Argument1Expression.EvaluateAndCoerceToInt(context, stackFrame);

					graphicsLibrary.SetDrawingAttribute(newAttribute);
				}
			}
		}
	}
}
