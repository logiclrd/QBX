using QBX.ExecutionEngine.Execution;
using QBX.Firmware;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class PixelSetStatement : IExecutable
{
	public bool StepCoordinates;
	public IEvaluable? XExpression;
	public IEvaluable? YExpression;
	public IEvaluable? ColourExpression;
	public bool UseForegroundColour;

	public void Execute(ExecutionContext context, bool stepInto)
	{
		if (context.VisualLibrary is not GraphicsLibrary graphicsLibrary)
			throw RuntimeException.IllegalFunctionCall(XExpression?.SourceStatement);

		var xValue = XExpression!.Evaluate(context);
		var yValue = YExpression!.Evaluate(context);

		int x = xValue.CoerceToInt();
		int y = yValue.CoerceToInt();

		if (StepCoordinates)
		{
			x += graphicsLibrary.LastPoint.X;
			y += graphicsLibrary.LastPoint.Y;
		}

		if (ColourExpression == null)
		{
			if (UseForegroundColour)
				graphicsLibrary.PixelSet(x, y);
			else
				graphicsLibrary.PixelSet(x, y, 0);
		}
		else
		{
			var colourValue = ColourExpression.Evaluate(context);

			int colour = colourValue.CoerceToInt();

			graphicsLibrary.PixelSet(x, y, colour);
		}
	}
}
