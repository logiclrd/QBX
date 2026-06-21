using QBX.ExecutionEngine.Execution;
using QBX.Firmware;
using QBX.Numbers;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class PixelSetStatement(CodeModel.Statements.PixelSetStatement source) : Executable(source)
{
	public bool StepCoordinates;
	public Evaluable? XExpression;
	public Evaluable? YExpression;
	public Evaluable? ColourExpression;
	public bool UseForegroundColour;

	public override void Execute(ExecutionContext context, StackFrame stackFrame)
	{
		if (context.VisualLibrary is not GraphicsLibrary graphicsLibrary)
			throw RuntimeException.IllegalFunctionCall(XExpression?.Source);

		var xValue = XExpression!.Evaluate(context, stackFrame);
		var yValue = YExpression!.Evaluate(context, stackFrame);

		float x = NumberConverter.ToSingle(xValue);
		float y = NumberConverter.ToSingle(yValue);

		if (StepCoordinates)
		{
			x += graphicsLibrary.LastPoint.X;
			y += graphicsLibrary.LastPoint.Y;
		}

		if (ColourExpression == null)
		{
			if (UseForegroundColour)
			{
				graphicsLibrary.PixelSet(x, y);
				context.DrawProcessor.SetColour(graphicsLibrary.DrawingAttribute);
			}
			else
			{
				graphicsLibrary.PixelSet(x, y, 0);
				context.DrawProcessor.SetColour(0);
			}
		}
		else
		{
			int colour = ColourExpression.EvaluateAndCoerceToInt(context, stackFrame);

			colour = colour & 255;

			if (colour > graphicsLibrary.MaximumAttribute)
				colour = graphicsLibrary.MaximumAttribute;

			graphicsLibrary.PixelSet(x, y, colour);

			context.DrawProcessor.SetColour(colour);
		}
	}
}
