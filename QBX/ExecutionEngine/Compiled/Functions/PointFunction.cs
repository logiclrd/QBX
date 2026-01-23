using System;

using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;
using QBX.Firmware;
using QBX.Numbers;

namespace QBX.ExecutionEngine.Compiled.Functions;

public class PointFunction : Function
{
	public Evaluable? XExpression;
	public Evaluable? YExpression;

	protected override int MinArgumentCount => 2;
	protected override int MaxArgumentCount => 2;

	protected override void SetArgument(int index, Evaluable value)
	{
		switch (index)
		{
			case 0:
				if (!value.Type.IsNumeric)
					throw CompilerException.TypeMismatch(value.Source);

				XExpression = value;
				break;
			case 1:
				if (!value.Type.IsNumeric)
					throw CompilerException.TypeMismatch(value.Source);

				YExpression = value;
				break;
		}
	}

	public override void CollapseConstantSubexpressions()
	{
		CollapseConstantExpression(ref XExpression);
		CollapseConstantExpression(ref YExpression);
	}

	public override DataType Type => DataType.Integer;

	public override Variable Evaluate(ExecutionContext context, StackFrame stackFrame)
	{
		if (XExpression == null)
			throw new Exception("PointFunction with no XExpression");
		if (YExpression == null)
			throw new Exception("PointFunction with no YExpression");

		if (context.VisualLibrary is not GraphicsLibrary visual)
			throw RuntimeException.IllegalFunctionCall(Source);

		var xValue = XExpression.Evaluate(context, stackFrame);
		var yValue = YExpression.Evaluate(context, stackFrame);

		float x = NumberConverter.ToSingle(xValue);
		float y = NumberConverter.ToSingle(yValue);

		int attribute = visual.PixelGet(x, y);

		return new IntegerVariable((short)attribute);
	}
}
