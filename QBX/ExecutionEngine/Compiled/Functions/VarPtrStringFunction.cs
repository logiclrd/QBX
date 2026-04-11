using System;
using System.Runtime.InteropServices;

using QBX.ExecutionEngine.Compiled.Expressions;
using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;

namespace QBX.ExecutionEngine.Compiled.Functions;

public class VarPtrStringFunction : Function
{
	public Evaluable? VariableExpression;
	public SurfacedVariableType SurfacedVariableType;

	protected override void SetArgument(int index, Evaluable value)
	{
		if ((value is not IdentifierExpression)
		 && (value is not ArrayElementExpression))
			throw CompilerException.ExpectedVariable(value.Source);

		VariableExpression = value;

		SurfacedVariableType =
			value.Type.PrimitiveType switch
			{
				PrimitiveDataType.Integer => SurfacedVariableType.Integer,
				PrimitiveDataType.Long => SurfacedVariableType.Long,
				PrimitiveDataType.Single => SurfacedVariableType.Single,
				PrimitiveDataType.Double => SurfacedVariableType.Double,
				PrimitiveDataType.Currency => SurfacedVariableType.Currency,
				PrimitiveDataType.String => SurfacedVariableType.String,

				_ => SurfacedVariableType.Unknown,
			};
	}

	public override void CollapseConstantSubexpressions()
	{
		CollapseConstantExpression(ref VariableExpression);
	}

	public override DataType Type => DataType.String;

	public override Variable Evaluate(ExecutionContext context, StackFrame stackFrame)
	{
		if (VariableExpression == null)
			throw new Exception("VarPtrStringFunction with no VariableExpression");

		var variable = VariableExpression.Evaluate(context, stackFrame);

		if (variable.SurfacedVariable != null)
		{
			if (variable.SurfacedVariable.IsValid)
			{
				var existingDescriptorSpan = new Span<SurfacedVariableDescriptor>(ref variable.SurfacedVariableDescriptor);

				var existingDescriptorBytes = MemoryMarshal.AsBytes(existingDescriptorSpan);

				return new StringVariable(new StringValue(existingDescriptorBytes));
			}
		}

		ushort key;

		if (VariableExpression is IdentifierExpression identifierExpression)
			key = context.SurfaceVariable(stackFrame, identifierExpression.VariableIndex);
		else if (VariableExpression is ArrayElementExpression arrayElementExpression)
		{
			var array = (ArrayVariable)arrayElementExpression.ArrayExpression.Evaluate(context, stackFrame);

			var subscriptValues = new Variable[arrayElementExpression.SubscriptExpressions.Count];

			for (int i = 0; i < arrayElementExpression.SubscriptExpressions.Count; i++)
				subscriptValues[i] = arrayElementExpression.SubscriptExpressions[i].Evaluate(context, stackFrame);

			int elementIndex = array.Array.Subscripts.GetElementIndex(subscriptValues, arrayElementExpression.SubscriptExpressions);

			key = context.SurfaceVariable(array, elementIndex);
		}
		else
			throw RuntimeException.IllegalFunctionCall(Source);

		var descriptor = new SurfacedVariableDescriptor();

		descriptor.Key = key;
		descriptor.Type = SurfacedVariableType;

		variable.SurfacedVariable = context.SurfacedVariables[key];
		variable.SurfacedVariableDescriptor = descriptor;

		var descriptorSpan = new Span<SurfacedVariableDescriptor>(ref descriptor);

		var descriptorBytes = MemoryMarshal.AsBytes(descriptorSpan);

		return new StringVariable(new StringValue(descriptorBytes));
	}
}
