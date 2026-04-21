using System;

using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;
using QBX.Firmware;
using QBX.Numbers;

namespace QBX.ExecutionEngine.Compiled.Functions;

public class PMapFunction : Function
{
	public Evaluable? CoordinateExpression;
	public Evaluable? MappingExpression;

	protected override int MinArgumentCount => 2;
	protected override int MaxArgumentCount => 2;

	protected override void SetArgument(int index, Evaluable value)
	{
		switch (index)
		{
			case 0:
				if (!value.Type.IsNumeric)
					throw CompilerException.TypeMismatch(value.Source);

				CoordinateExpression = value;
				break;
			case 1:
				if (!value.Type.IsNumeric)
					throw CompilerException.TypeMismatch(value.Source);

				MappingExpression = value;
				break;
		}
	}

	public override void CollapseConstantSubexpressions()
	{
		CollapseConstantExpression(ref CoordinateExpression);
		CollapseConstantExpression(ref MappingExpression);
	}

	public override DataType Type => DataType.Integer;

	public override Variable Evaluate(ExecutionContext context, StackFrame stackFrame)
	{
		if (CoordinateExpression == null)
			throw new Exception("PMapFunction with no CoordinateExpression");
		if (MappingExpression == null)
			throw new Exception("PMapFunction with no MappingExpression");

		if (context.VisualLibrary is not GraphicsLibrary visual)
			throw RuntimeException.IllegalFunctionCall(Source);

		var coordinateValue = CoordinateExpression.Evaluate(context, stackFrame);

		float coordinate = NumberConverter.ToSingle(coordinateValue);
		int mapping = MappingExpression.EvaluateAndCoerceToInt(context, stackFrame);

		float mappedCoordinate;

		switch (mapping)
		{
			case 0: mappedCoordinate = visual.CoordinateSystem.TranslateWindowToView(coordinate, 0).X; break;
			case 1: mappedCoordinate = visual.CoordinateSystem.TranslateWindowToView(0, coordinate).Y; break;
			case 2: mappedCoordinate = visual.CoordinateSystem.TranslateViewToWindow(coordinate, 0).X; break;
			case 3: mappedCoordinate = visual.CoordinateSystem.TranslateViewToWindow(0, coordinate).Y; break;

			default:
				throw RuntimeException.IllegalFunctionCall();
		}

		return new SingleVariable(mappedCoordinate);
	}
}
