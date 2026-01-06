using QBX.ExecutionEngine.Execution;
using QBX.Firmware;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class ScreenStatement : IExecutable
{
	public IEvaluable? ModeExpression;
	public IEvaluable? ColourSwitchExpression;
	public IEvaluable? ActivePageExpression;
	public IEvaluable? VisiblePageExpression;

	public void Execute(ExecutionContext context, bool stepInto)
	{
		if (ModeExpression != null)
		{
			int qbMode = ModeExpression.Evaluate(context).CoerceToInt();

			int hardwareMode = System.Array.FindLastIndex(
				Video.Modes,
				mode => mode?.ScreenNumber == qbMode);

			if (!new Video(context.Machine).SetMode(hardwareMode))
				throw RuntimeException.IllegalFunctionCall(ModeExpression.SourceStatement);

			if (Video.Modes[hardwareMode] is ModeParameters modeParams)
			{
				if (!modeParams.IsGraphicsMode)
					context.VisualLibrary = new TextLibrary(context.Machine.GraphicsArray);
				else if (modeParams.ShiftRegisterInterleave)
					context.VisualLibrary = new GraphicsLibrary_2bppInterleaved(context.Machine.GraphicsArray);
				else if (modeParams.IsMonochrome)
					context.VisualLibrary = new GraphicsLibrary_1bppPacked(context.Machine.GraphicsArray);
				else if (modeParams.Use256Colours)
					context.VisualLibrary = new GraphicsLibrary_8bppFlat(context.Machine.GraphicsArray);
				else
					context.VisualLibrary = new GraphicsLibrary_4bppPlanar(context.Machine.GraphicsArray);

				context.EnablePaletteRemapping = (hardwareMode < 0x12);
			}
		}

		if (ActivePageExpression != null)
		{
			int activePage = ActivePageExpression.Evaluate(context).CoerceToInt();

			if (!context.VisualLibrary.SetActivePage(activePage))
				throw RuntimeException.IllegalFunctionCall(ActivePageExpression.SourceStatement);
		}

		if (VisiblePageExpression != null)
		{
			int visiblePage = VisiblePageExpression.Evaluate(context).CoerceToInt();

			if (!new Video(context.Machine).SetVisiblePage(visiblePage))
				throw RuntimeException.IllegalFunctionCall(VisiblePageExpression.SourceStatement);
		}
	}
}
