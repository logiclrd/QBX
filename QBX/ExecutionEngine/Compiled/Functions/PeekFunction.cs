using System;

using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;

namespace QBX.ExecutionEngine.Compiled.Functions;

public class PeekFunction : Function
{
	public Evaluable? AddressExpression;

	protected override void SetArgument(int index, Evaluable value)
	{
		AddressExpression = value;
	}

	public override void CollapseConstantSubexpressions()
	{
		CollapseConstantExpression(ref AddressExpression);
	}

	public override DataType Type => DataType.Integer;

	public override Variable Evaluate(ExecutionContext context, StackFrame stackFrame)
	{
		if (AddressExpression == null)
			throw new Exception("PeekFunction with no AddressExpression");

		var addressValue = AddressExpression.Evaluate(context, stackFrame);

		int address = 0xFFFF & addressValue.CoerceToInt(context: AddressExpression);

		address += context.RuntimeState.SegmentBase;

		context.Machine.SystemMemory.UpdateDynamicData(address, address + 1);

		return new IntegerVariable(context.Machine.MemoryBus[address]);
	}
}
