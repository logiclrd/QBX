using System;

using QBX.ExecutionEngine.Execution;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class IfStatement(CodeModel.Statements.Statement? source) : Executable(source)
{
	public Evaluable? Condition;
	public Sequence? ThenBody;
	public Sequence? ElseBody;

	public override void Execute(ExecutionContext context, StackFrame stackFrame)
	{
		if (Condition == null)
			throw new Exception("IfStatement with no Condition");

		var value = Condition.Evaluate(context, stackFrame);

		if (value.DataType.IsString || !value.DataType.IsPrimitiveType)
			throw CompilerException.TypeMismatch(Condition.SourceStatement);

		if (!value.IsZero)
			context.Dispatch(ThenBody, stackFrame);
		else
			context.Dispatch(ElseBody, stackFrame);
	}
}
