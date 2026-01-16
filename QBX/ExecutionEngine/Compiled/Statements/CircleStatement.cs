using QBX.ExecutionEngine.Execution;
using QBX.Firmware;
using QBX.Numbers;
using System;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class CircleStatement(CodeModel.Statements.Statement source) : Executable(source)
{
	public bool Step;
	public Evaluable? XExpression;
	public Evaluable? YExpression;
	public Evaluable? RadiusExpression;
	public Evaluable? ColourExpression;
	public Evaluable? StartExpression;
	public Evaluable? EndExpression;
	public Evaluable? AspectExpression;

	public override void Execute(ExecutionContext context, StackFrame stackFrame)
	{
		if (XExpression == null)
			throw new Exception("CircleStatement with no XExpression");
		if (YExpression == null)
			throw new Exception("CircleStatement with no YExpression");
		if (RadiusExpression == null)
			throw new Exception("CircleStatement with no RadiusExpression");

		if (context.VisualLibrary is not GraphicsLibrary visual)
			throw RuntimeException.IllegalFunctionCall(Source);

		var xValue = XExpression.Evaluate(context, stackFrame);
		var yValue = YExpression.Evaluate(context, stackFrame);
		var radiusValue = RadiusExpression.Evaluate(context, stackFrame);

		float x = NumberConverter.ToSingle(xValue);
		float y = NumberConverter.ToSingle(yValue);

		if (Step)
		{
			x += visual.LastPoint.X;
			y += visual.LastPoint.Y;
		}

		float radiusX = NumberConverter.ToSingle(radiusValue);
		float radiusY = radiusX;

		if (AspectExpression != null)
		{
			var aspectValue = AspectExpression.Evaluate(context, stackFrame);

			float aspect = NumberConverter.ToSingle(aspectValue);

			if (aspect > 1)
				radiusX /= aspect;
			else if (aspect > -1)
				radiusY *= Math.Abs(aspect);
		}

		float startAngle = 0f;
		float endAngle = 0f;

		if (StartExpression != null)
		{
			var startValue = StartExpression.Evaluate(context, stackFrame);

			startAngle = NumberConverter.ToSingle(startValue);
		}

		if (EndExpression != null)
		{
			var endValue = EndExpression.Evaluate(context, stackFrame);

			endAngle = NumberConverter.ToSingle(endValue);
		}

		if (ColourExpression == null)
		{
			visual.Ellipse(
				x, y,
				radiusX, radiusY,
				Math.Abs(startAngle),
				Math.Abs(endAngle),
				drawStartRadius: startAngle < 0,
				drawEndRadius: endAngle < 0);
		}
		else
		{
			var colourValue = ColourExpression.Evaluate(context, stackFrame);

			int colour = colourValue.CoerceToInt();

			visual.Ellipse(
				x, y,
				radiusX, radiusY,
				Math.Abs(startAngle),
				Math.Abs(endAngle),
				drawStartRadius: startAngle < 0,
				drawEndRadius: endAngle < 0,
				colour);
		}
	}
}
