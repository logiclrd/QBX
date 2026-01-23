using System;

using QBX.ExecutionEngine.Execution;
using QBX.Firmware;
using QBX.Numbers;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class LineStatement(CodeModel.Statements.Statement source) : Executable(source)
{
	public bool FromStep;
	public Evaluable? FromXExpression;
	public Evaluable? FromYExpression;

	public bool ToStep;
	public Evaluable? ToXExpression;
	public Evaluable? ToYExpression;

	public Evaluable? ColourExpression;
	public LineDrawStyle DrawStyle = LineDrawStyle.Line;
	public Evaluable? StyleExpression;

	public override void Execute(ExecutionContext context, StackFrame stackFrame)
	{
		if (context.VisualLibrary is not GraphicsLibrary visual)
			throw RuntimeException.IllegalFunctionCall(Source);

		if (ToXExpression == null)
			throw new Exception("LineExpression with no ToXExpression");
		if (ToYExpression == null)
			throw new Exception("LineExpression with no ToYExpression");

		float fromX, fromY;

		if ((FromXExpression != null) || (FromYExpression != null))
		{
			if (FromXExpression == null)
				throw new Exception("LineExpression with no FromXExpression");
			if (FromYExpression == null)
				throw new Exception("LineExpression with no FromYExpression");

			var fromXValue = FromXExpression.Evaluate(context, stackFrame);
			var fromYValue = FromYExpression.Evaluate(context, stackFrame);

			fromX = NumberConverter.ToSingle(fromXValue);
			fromY = NumberConverter.ToSingle(fromYValue);
		}
		else
		{
			if (FromStep)
				throw new Exception("LineExpression specifies from STEP but with no from coordinate information");

			fromX = visual.LastPoint.X;
			fromY = visual.LastPoint.Y;
		}

		if (FromStep)
		{
			fromX += visual.LastPoint.X;
			fromY += visual.LastPoint.Y;
		}

		var toXValue = ToXExpression.Evaluate(context, stackFrame);
		var toYValue = ToYExpression.Evaluate(context, stackFrame);

		float toX = NumberConverter.ToSingle(toXValue);
		float toY = NumberConverter.ToSingle(toYValue);

		if (ToStep)
		{
			toX += fromX;
			toY += fromY;
		}

		int attribute;

		if (ColourExpression != null)
			attribute = ColourExpression.EvaluateAndCoerceToInt(context, stackFrame);
		else
			attribute = visual.DrawingAttribute;

		bool haveStyle = false;
		int styleBits = 0;

		if (StyleExpression != null)
		{
			styleBits = StyleExpression.EvaluateAndCoerceToInt(context, stackFrame);
			haveStyle = true;
		}

		switch (DrawStyle)
		{
			case LineDrawStyle.Line:
				if (haveStyle)
					visual.LineStyle(fromX, fromY, toX, toY, attribute, styleBits);
				else
					visual.Line(fromX, fromY, toX, toY, attribute);
				break;
			case LineDrawStyle.Box:
				if (haveStyle)
					visual.BoxStyle(fromX, fromY, toX, toY, attribute, styleBits);
				else
					visual.Box(fromX, fromY, toX, toY, attribute);
				break;
			case LineDrawStyle.FilledBox:
				visual.FillBox(fromX, fromY, toX, toY, attribute);
				break;
		}
	}
}
