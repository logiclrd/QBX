using System;

using QBX.ExecutionEngine.Execution;
using QBX.Firmware;
using QBX.Numbers;
using QBX.Utility;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class SetGraphicsViewportStatement(CodeModel.Statements.GraphicsViewportStatement source) : GraphicsViewportStatement(source)
{
	public bool UseScreenCoordinates;

	public Evaluable? X1Expression;
	public Evaluable? Y1Expression;
	public Evaluable? X2Expression;
	public Evaluable? Y2Expression;
	public Evaluable? FillColourExpression;
	public Evaluable? BorderColourExpression;

	public override void Execute(ExecutionContext context, StackFrame stackFrame)
	{
		if (context.VisualLibrary is not GraphicsLibrary visual)
			throw RuntimeException.IllegalFunctionCall(Source);

		if (X1Expression == null)
			throw new Exception("SetGraphicsViewportStatement with no X1Expression");
		if (Y1Expression == null)
			throw new Exception("SetGraphicsViewportStatement with no Y1Expression");
		if (X2Expression == null)
			throw new Exception("SetGraphicsViewportStatement with no X2Expression");
		if (Y2Expression == null)
			throw new Exception("SetGraphicsViewportStatement with no Y2Expression");

		var x1Value = X1Expression.Evaluate(context, stackFrame);
		var y1Value = Y1Expression.Evaluate(context, stackFrame);
		var x2Value = X2Expression.Evaluate(context, stackFrame);
		var y2Value = Y2Expression.Evaluate(context, stackFrame);

		int x1 = (int)NumberConverter.ToSingle(x1Value);
		int y1 = (int)NumberConverter.ToSingle(y1Value);
		int x2 = (int)NumberConverter.ToSingle(x2Value);
		int y2 = (int)NumberConverter.ToSingle(y2Value);

		if ((x1 == x2) || (y1 == y2))
			throw RuntimeException.IllegalFunctionCall(Source);

		if (x1 > x2)
			(x1, x2) = (x2, x1);
		if (y1 > y2)
			(y1, y2) = (y2, y1);

		visual.Clip = new IntegerRect(x1, y1, x2, y2);

		visual.CoordinateSystem.SetViewport(
			visual.Clip,
			UseScreenCoordinates
			? CoordinateType.Screen
			: CoordinateType.Viewport);

		if ((FillColourExpression != null)
		 || (BorderColourExpression != null))
		{
			using (visual.RawCoordinateScope())
			{
				if (FillColourExpression != null)
				{
					int fillColour = FillColourExpression.EvaluateAndCoerceToInt(context, stackFrame);

					visual.FillBox(x1, y1, x2, y2, fillColour & visual.MaximumAttribute);
				}

				if (BorderColourExpression != null)
				{
					int borderColour = BorderColourExpression.EvaluateAndCoerceToInt(context, stackFrame);

					int dy = Math.Sign(y2 - y1);

					visual.Box(x1 - 1, y1 - dy, x2 + 1, y2 + dy, borderColour & visual.MaximumAttribute);
				}
			}
		}

		visual.LastPoint = visual.CoordinateSystem.TranslateBack(visual.CoordinateSystem.ViewportCentre);
	}
}
