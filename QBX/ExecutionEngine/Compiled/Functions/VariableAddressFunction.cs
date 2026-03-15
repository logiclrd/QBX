using System;

using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;
using QBX.OperatingSystem.Memory;

namespace QBX.ExecutionEngine.Compiled.Functions;

public abstract class VariableAddressFunction : Function
{
	public Evaluable? VariableExpression;

	protected override void SetArgument(int index, Evaluable value)
	{
		VariableExpression = value;
	}

	public override void CollapseConstantSubexpressions()
	{
		CollapseConstantExpression(ref VariableExpression);
	}

	public override DataType Type => DataType.Integer;

	protected abstract ushort GetAddressPart(SegmentedAddress address);

	public override Variable Evaluate(ExecutionContext context, StackFrame stackFrame)
	{
		if (VariableExpression == null)
			throw new Exception("VarSegFunction with no VariableExpression");

		var variable = VariableExpression.Evaluate(context, stackFrame);

		var memoryOwner = variable;

		while (memoryOwner.PinnedMemoryOwner != null)
			memoryOwner = memoryOwner.PinnedMemoryOwner;

		if (!memoryOwner.IsPinned)
		{
			context.ReleasePinnedMemory();
			memoryOwner.AllocateAndPin(context);
		}

		var memoryAddress = new SegmentedAddress(variable.PinnedMemoryAddress);

		return new IntegerVariable(GetAddressPart(memoryAddress));
	}
}
