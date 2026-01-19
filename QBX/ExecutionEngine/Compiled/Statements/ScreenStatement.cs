using QBX.ExecutionEngine.Execution;
using QBX.Firmware;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class ScreenStatement(CodeModel.Statements.Statement? source) : Executable(source)
{
	public Evaluable? ModeExpression;
	public Evaluable? ColourSwitchExpression;
	public Evaluable? ActivePageExpression;
	public Evaluable? VisiblePageExpression;

	public override void Execute(ExecutionContext context, StackFrame stackFrame)
	{
		if (ModeExpression != null)
		{
			int qbMode = ModeExpression.EvaluateAndCoerceToInt(context, stackFrame);

			int hardwareMode = System.Array.FindLastIndex(
				Video.Modes,
				mode => mode?.ScreenNumber == qbMode);

			if (!context.Machine.VideoFirmware.SetMode(hardwareMode))
				throw RuntimeException.IllegalFunctionCall(ModeExpression.Source);

			if (Video.Modes[hardwareMode] is ModeParameters modeParams)
			{
				if (!modeParams.IsGraphicsMode)
				{
					var textLibrary = new TextLibrary(context.Machine);

					context.VisualLibrary = textLibrary;

					textLibrary.HideCursor();
				}
				else if (modeParams.ShiftRegisterInterleave)
					context.VisualLibrary = new GraphicsLibrary_2bppInterleaved(context.Machine);
				else if (modeParams.IsMonochrome)
					context.VisualLibrary = new GraphicsLibrary_1bppPacked(context.Machine);
				else if (modeParams.Use256Colours)
					context.VisualLibrary = new GraphicsLibrary_8bppFlat(context.Machine);
				else
					context.VisualLibrary = new GraphicsLibrary_4bppPlanar(context.Machine);

				context.RuntimeState.EnablePaletteRemapping = (hardwareMode < 0x12);
				context.RuntimeState.PaletteMode = modeParams.Use256Colours ? PaletteMode.DAC : PaletteMode.Attribute;
				context.RuntimeState.MaximumAttribute = modeParams.Use256Colours ? 255 : 15;

				if (modeParams.ShiftRegisterInterleave)
				{
					context.RuntimeState.PaletteMode = PaletteMode.CGA;
					context.RuntimeState.MaximumAttribute = 3;
				}
				else if (modeParams.IsMonochrome)
					context.RuntimeState.MaximumAttribute = 1;
			}
		}

		if (ActivePageExpression != null)
		{
			int activePage = ActivePageExpression.EvaluateAndCoerceToInt(context, stackFrame);

			if (!context.VisualLibrary.SetActivePage(activePage))
				throw RuntimeException.IllegalFunctionCall(ActivePageExpression.Source);
		}

		if (VisiblePageExpression != null)
		{
			int visiblePage = VisiblePageExpression.EvaluateAndCoerceToInt(context, stackFrame);

			if (!context.Machine.VideoFirmware.SetVisiblePage(visiblePage))
				throw RuntimeException.IllegalFunctionCall(VisiblePageExpression.Source);
		}
	}
}
