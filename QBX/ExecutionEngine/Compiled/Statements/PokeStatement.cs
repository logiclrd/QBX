using System;

using QBX.ExecutionEngine.Execution;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class PokeStatement(CodeModel.Statements.Statement source) : Executable(source)
{
	public Evaluable? AddressExpression;
	public Evaluable? ValueExpression;

	public override void Execute(ExecutionContext context, StackFrame stackFrame)
	{
		if (AddressExpression is null)
			throw new Exception("PokeStatement has no AddressExpression");
		if (ValueExpression is null)
			throw new Exception("PokeStatement has no ValueExpression");

		int address = AddressExpression.EvaluateAndCoerceToInt(context, stackFrame);
		int value = ValueExpression.EvaluateAndCoerceToInt(context, stackFrame);

		context.Machine.MemoryBus[context.RuntimeState.SegmentBase + address] =
			unchecked((byte)value);
	}
}
