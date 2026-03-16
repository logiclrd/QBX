using System;
using System.Runtime.InteropServices;

using QBX.ExecutionEngine.Compiled.Expressions;
using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;

namespace QBX.ExecutionEngine.Compiled.Functions;

public class VarPtrStringFunction : Function
{
	public Evaluable? VariableExpression;

	protected override void SetArgument(int index, Evaluable value)
	{
		if (!value.Type.IsString)
			throw CompilerException.TypeMismatch(value.Source);

		if ((value is not IdentifierExpression)
		 && (value is not ArrayElementExpression))
			throw CompilerException.ExpectedVariable(value.Source);

		VariableExpression = value;
	}

	public override void CollapseConstantSubexpressions()
	{
		CollapseConstantExpression(ref VariableExpression);
	}

	public override DataType Type => DataType.String;

	public override Variable Evaluate(ExecutionContext context, StackFrame stackFrame)
	{
		int key;

		if (VariableExpression is IdentifierExpression identifierExpression)
			key = context.SurfaceString(stackFrame, identifierExpression.VariableIndex);
		else if (VariableExpression is ArrayElementExpression arrayElementExpression)
		{
			var array = (ArrayVariable)arrayElementExpression.ArrayExpression.Evaluate(context, stackFrame);

			var subscriptValues = new Variable[arrayElementExpression.SubscriptExpressions.Count];

			for (int i = 0; i < arrayElementExpression.SubscriptExpressions.Count; i++)
				subscriptValues[i] = arrayElementExpression.SubscriptExpressions[i].Evaluate(context, stackFrame);

			int elementIndex = array.Array.Subscripts.GetElementIndex(subscriptValues, arrayElementExpression.SubscriptExpressions);

			key = context.SurfaceString(array, elementIndex);
		}
		else
			throw RuntimeException.IllegalFunctionCall(Source);

		var keySpan = new Span<int>(ref key);

		var keyBytes = MemoryMarshal.AsBytes(keySpan).Slice(0, 3);

		return new StringVariable(new StringValue(keyBytes));
	}
}
