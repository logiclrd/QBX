using System;
using System.Collections.Generic;
using System.Linq;

using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;
using QBX.Firmware;

namespace QBX.ExecutionEngine.Compiled.Functions;

public class CoordinatePointFunction : Function
{
	public Evaluable? WhichCoordinateExpression;

	protected override int MinArgumentCount => 1;
	protected override int MaxArgumentCount => 1;

	protected override void SetArgument(int index, Evaluable value)
	{
		switch (index)
		{
			case 0:
				if (!value.Type.IsNumeric)
					throw CompilerException.TypeMismatch(value.Source);

				WhichCoordinateExpression = value;
				break;
		}
	}

	public override void CollapseConstantSubexpressions()
	{
		CollapseConstantExpression(ref WhichCoordinateExpression);
	}

	public override DataType Type => DataType.Single;

	public override Variable Evaluate(ExecutionContext context, StackFrame stackFrame)
	{
		if (WhichCoordinateExpression == null)
			throw new Exception("CoordinatePointFunction with no WhichCoordinateExpression");

		if (context.VisualLibrary is not GraphicsLibrary visual)
			throw RuntimeException.IllegalFunctionCall(Source);

		var whichCoordinate = WhichCoordinateExpression.EvaluateAndCoerceToInt(context, stackFrame);

		float coordinateValue;

		switch (whichCoordinate)
		{
			case 0:
			case 1:
				var (x, y) = visual.CoordinateSystem.TranslateWindowToView(visual.LastPoint.X, visual.LastPoint.Y);

				switch (whichCoordinate)
				{
					case 0: coordinateValue = x; break;
					case 1: coordinateValue = y; break;

					default: throw new Exception("Sanity failure");
				}

				break;
			case 2: coordinateValue = visual.LastPoint.X; break;
			case 3: coordinateValue = visual.LastPoint.Y; break;
			default: throw RuntimeException.IllegalFunctionCall();
		}

		return new SingleVariable(coordinateValue);
	}
}
