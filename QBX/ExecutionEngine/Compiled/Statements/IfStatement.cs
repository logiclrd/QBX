using System;

using QBX.ExecutionEngine.Execution;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class IfStatement : IExecutable
{
	public IEvaluable? Condition;
	public IExecutable? ThenBody;
	public IExecutable? ElseBody;

	public void Execute(ExecutionContext context, bool stepInto)
	{
		if (Condition == null)
			throw new Exception("IfStatement with no Condition");

		var value = Condition.Evaluate(context);

		if (value.DataType.IsString || !value.DataType.IsPrimitiveType)
			throw CompilerException.TypeMismatch(Condition.SourceStatement);

		if (!value.IsZero)
			context.Execute(ThenBody, stepInto);
		else
			context.Execute(ElseBody, stepInto);
	}
}
