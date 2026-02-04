using System;

using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;

namespace QBX.ExecutionEngine.Compiled.Functions;

public class PenFunction : Function
{
	public Evaluable? FunctionExpression;

	protected override void SetArgument(int index, Evaluable value)
	{
		FunctionExpression = value;
	}

	public override void CollapseConstantSubexpressions()
	{
		CollapseConstantExpression(ref FunctionExpression);
	}

	public override DataType Type => DataType.Integer;

	public override Variable Evaluate(ExecutionContext context, StackFrame stackFrame)
	{
		if (FunctionExpression == null)
			throw new Exception("InpFunction with no PortExpression");

		int function = FunctionExpression.EvaluateAndCoerceToInt(context, stackFrame);

		switch (function)
		{
			case 0: // Pen down since last call
			{
				var penHasBeenDown = context.Machine.MouseDriver.LightPenHasBeenDown;

				context.Machine.MouseDriver.ClearLightPenHasBeenDown();

				return new IntegerVariable(penHasBeenDown);
			}
			case 1: // Pen first down X
				return new IntegerVariable(unchecked((short)context.Machine.MouseDriver.LightPenStartPosition.X));
			case 2: // Pen first down Y
				return new IntegerVariable(unchecked((short)context.Machine.MouseDriver.LightPenStartPosition.Y));
			case 3: // Current pen status
				return new IntegerVariable(unchecked(context.Machine.MouseDriver.LightPenIsDown));
			case 4: // Pen last down X (current if still down)
				return new IntegerVariable(unchecked((short)context.Machine.MouseDriver.LightPenEndPosition.X));
			case 5: // Pen last down Y (current if still down)
				return new IntegerVariable(unchecked((short)context.Machine.MouseDriver.LightPenEndPosition.Y));
			case 6: // Pen first down X, but in characters
				return new IntegerVariable(unchecked((short)(1 + context.Machine.MouseDriver.LightPenStartPosition.X / 8)));
			case 7: // Pen first down Y, but in characters
				return new IntegerVariable(unchecked((short)(1 + context.Machine.MouseDriver.LightPenStartPosition.Y / 8)));
			case 8: // Pen last down X, but in characters
				return new IntegerVariable(unchecked((short)(1 + context.Machine.MouseDriver.LightPenEndPosition.X / 8)));
			case 9: // Pen last down Y, but in characters
				return new IntegerVariable(unchecked((short)(1 + context.Machine.MouseDriver.LightPenEndPosition.Y / 8)));

			default:
				throw RuntimeException.IllegalFunctionCall(Source);
		}
	}
}
