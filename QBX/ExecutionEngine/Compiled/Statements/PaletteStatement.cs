using System;

using QBX.ExecutionEngine.Execution;
using QBX.Hardware;

using static QBX.Hardware.GraphicsArray;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class PaletteStatement(CodeModel.Statements.Statement source) : Executable(source)
{
	public Evaluable? AttributeExpression;
	public Evaluable? ColourExpression;

	public override void Execute(ExecutionContext context, StackFrame stackFrame)
	{
		if (!context.RuntimeState.EnablePaletteRemapping)
			throw RuntimeException.IllegalFunctionCall(Source);

		if (AttributeExpression == null)
			throw new Exception("PaletteStatement with no AttributeExpression");
		if (ColourExpression == null)
			throw new Exception("PaletteStatement with no ColourExpression");

		int attribute = AttributeExpression.EvaluateAndCoerceToInt(context, stackFrame);
		int colour = ColourExpression.EvaluateAndCoerceToInt(context, stackFrame);

		if ((attribute < 0) || (attribute > context.RuntimeState.MaximumAttribute))
			throw RuntimeException.IllegalFunctionCall(AttributeExpression?.Source);

		try
		{
			AlterPalette(attribute, colour, context.RuntimeState.PaletteMode, context.Machine);
		}
		catch (RuntimeException ex)
		{
			throw ex.AddContext(ColourExpression?.Source);
		}
	}

	public static void AlterPalette(int attribute, int colour, PaletteMode paletteMode, Machine machine)
	{
		switch (paletteMode)
		{
			case PaletteMode.CGA:
				if ((colour < 0) || (colour > 15))
					throw RuntimeException.IllegalFunctionCall();

				// I'm not sure what exactly this translation is, VGA->EGA? Anyway,
				// it's what QuickBASIC itself does in SCREEN 1 / mode 5h.
				if (colour >= 8)
					colour |= 16;

				goto case PaletteMode.Attribute;

			case PaletteMode.Attribute:
				// Remap the DAC colour to which the specified attribute maps.
				if ((colour < 0) || (colour > 63))
					throw RuntimeException.IllegalFunctionCall();


				machine.InPort(InputStatusRegisters.InputStatus1Port);
				machine.OutPort(AttributeControllerRegisters.IndexAndDataWritePort, unchecked((byte)attribute));
				machine.OutPort(AttributeControllerRegisters.IndexAndDataWritePort, unchecked((byte)colour));

				break;
			case PaletteMode.DAC:
				// Reprogram the specified DAC colour to a new RGB.
				machine.OutPort(DACRegisters.WriteIndexPort, unchecked((byte)attribute));

				// The components are each 6 bits wide. The bits are lined up nicely with
				// byte boundaries. Any valid number only has bits set in the component
				// fields.
				if ((colour & ~0x3F3F3F) != 0)
					throw RuntimeException.IllegalFunctionCall();

				for (int i = 0; i < 3; i++)
				{
					machine.OutPort(DACRegisters.DataPort, unchecked((byte)colour));
					colour >>= 8;
				}

				break;
		}
	}
}
