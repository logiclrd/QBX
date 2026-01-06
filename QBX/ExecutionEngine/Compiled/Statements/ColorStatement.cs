using QBX.Firmware;
using QBX.Hardware;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class ColorStatement : IExecutable
{
	public IEvaluable? Argument1Expression;
	public IEvaluable? Argument2Expression;
	public IEvaluable? Argument3Expression;

	public void Execute(Execution.ExecutionContext context, bool stepInto)
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
				if (Argument1Expression != null)
					library.SetForegroundAttribute(Argument1Expression.Evaluate(context).CoerceToInt());
				if (Argument2Expression != null)
					library.SetBackgroundAttribute(Argument2Expression.Evaluate(context).CoerceToInt());

				if (Argument3Expression != null)
				{
					int newOverscanAttribute = Argument3Expression.Evaluate(context).CoerceToInt() & 15;

					context.Machine.GraphicsArray.AttributeController.Registers[GraphicsArray.AttributeControllerRegisters.OverscanPaletteIndex]
						= unchecked((byte)newOverscanAttribute);
				}
			}
		}
		else if (context.Machine.GraphicsArray.Graphics.ShiftInterleave)
		{
			// CGA modes
			if (Argument1Expression != null)
			{
				int newBackgroundAttribute = Argument1Expression.Evaluate(context).CoerceToInt();

				if ((newBackgroundAttribute < 0) || (newBackgroundAttribute > 255))
					throw RuntimeException.IllegalFunctionCall(Argument1Expression.SourceStatement);

				newBackgroundAttribute &= 15;

				context.Machine.GraphicsArray.AttributeController.Registers[0] =
					unchecked((byte)newBackgroundAttribute);
			}

			if (Argument2Expression != null)
			{
				int newCGAPalette = Argument2Expression.Evaluate(context).CoerceToInt();

				if ((newCGAPalette < 0) || (newCGAPalette > 255))
					throw RuntimeException.IllegalFunctionCall(Argument2Expression.SourceStatement);

				newCGAPalette &= 1;

				context.Machine.GraphicsArray.LoadCGAPalette(newCGAPalette, intensity: false);
			}

			// argument 3 is ignored
		}
		else
		{
			if (context.VisualLibrary is GraphicsLibrary graphicsLibrary)
			{
				if (Argument3Expression != null)
					throw RuntimeException.IllegalFunctionCall(Argument3Expression.SourceStatement);

				if ((Argument2Expression != null) && !context.EnablePaletteRemapping)
					throw RuntimeException.IllegalFunctionCall(Argument2Expression.SourceStatement);

				if (Argument1Expression != null)
				{
					int newAttribute = Argument1Expression.Evaluate(context).CoerceToInt();

					graphicsLibrary.SetDrawingAttribute(newAttribute);
				}
			}
		}
	}
}
