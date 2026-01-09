using QBX.ExecutionEngine.Execution;
using QBX.Firmware;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class PixelSetStatement(CodeModel.Statements.Statement? source) : Statement(source)
{
	public bool StepCoordinates;
	public IEvaluable? XExpression;
	public IEvaluable? YExpression;
	public IEvaluable? ColourExpression;
	public bool UseForegroundColour;

	public override void Execute(ExecutionContext context, StackFrame stackFrame)
	{
		if (context.VisualLibrary is not GraphicsLibrary graphicsLibrary)
			throw RuntimeException.IllegalFunctionCall(XExpression?.SourceStatement);

		var xValue = XExpression!.Evaluate(context, stackFrame);
		var yValue = YExpression!.Evaluate(context, stackFrame);

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
			var colourValue = ColourExpression.Evaluate(context, stackFrame);

			int colour = colourValue.CoerceToInt();

			graphicsLibrary.PixelSet(x, y, colour);
		}
	}
}
