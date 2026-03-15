using System;

using QBX.ExecutionEngine.Compiled.Expressions;
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

	protected virtual bool Validate(Variable variable)
	{
		// Maybe in the future we generate simulated string descriptors?
		if (variable.DataType.IsString)
			throw RuntimeException.IllegalFunctionCall(Source);

		return true;
	}

	protected abstract Variable CreateResult(SegmentedAddress address);

	public override Variable Evaluate(ExecutionContext context, StackFrame stackFrame)
	{
		if (VariableExpression == null)
			throw new Exception("VarSegFunction with no VariableExpression");

		var variable = VariableExpression.Evaluate(context, stackFrame);

		if (!Validate(variable))
			return CreateResult(SegmentedAddress.Zero);

		var memoryOwner = variable;

		while (memoryOwner.PinnedMemoryOwner != null)
			memoryOwner = memoryOwner.PinnedMemoryOwner;

		if (!memoryOwner.IsPinned)
		{
			context.ReleasePinnedMemory();

			if (memoryOwner.SelfAllocateAndPin)
			{
				memoryOwner.AllocateAndPin(context);

				variable = VariableExpression.Evaluate(context, stackFrame);
			}
			else
			{
				if (memoryOwner != variable)
					throw new Exception("Internal error: Somehow landed on a parent PinnedMemoryOwner that doesn't self-allocate and pin");
				if (VariableExpression is not IdentifierExpression identifierExpression)
					throw new Exception("Internal error: Don't know how to allocate and pin a variable from a " + VariableExpression.GetType().Name);

				var unpinnedVariable = variable;

				int byteSize = unpinnedVariable is StringVariable stringVariable
					? stringVariable.ValueSpan.Length
					: variable.DataType.ByteSize;

				variable = Variable.AllocateAndConstructPinned(variable.DataType, byteSize, context);
				variable.SetData(unpinnedVariable.GetData());

				stackFrame.Variables[identifierExpression.VariableIndex] = variable;
			}
		}

		var memoryAddress = new SegmentedAddress(variable.PinnedMemoryAddress);

		return CreateResult(memoryAddress);
	}
}
