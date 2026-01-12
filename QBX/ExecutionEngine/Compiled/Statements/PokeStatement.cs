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

		var address = AddressExpression.Evaluate(context, stackFrame);
		var value = ValueExpression.Evaluate(context, stackFrame);

		context.Machine.SystemMemory[address.CoerceToInt()] =
			unchecked((byte)value.CoerceToInt());
	}
}
