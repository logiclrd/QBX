using System;

using QBX.ExecutionEngine.Execution;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class IfStatement(CodeModel.Statements.Statement? source) : Executable(source)
{
	public Evaluable? Condition;
	public Sequence? ThenBody;
	public Sequence? ElseBody;

	public override int IndexOfSequence(Sequence sequence)
	{
		if (sequence == ThenBody)
			return 0;
		if (sequence == ElseBody)
			return 1;

		throw new Exception("Internal error: Sequence is not owned by this statement");
	}

	public override int GetSequenceCount() => 2;

	public override Sequence? GetSequenceByIndex(int sequenceIndex)
	{
		if (sequenceIndex == 0)
			return ThenBody;
		if (sequenceIndex == 1)
			return ElseBody;

		throw new IndexOutOfRangeException();
	}

	public override void Execute(ExecutionContext context, StackFrame stackFrame)
	{
		if (Condition == null)
			throw new Exception("IfStatement with no Condition");

		var value = Condition.Evaluate(context, stackFrame);

		if (!value.DataType.IsNumeric)
			throw CompilerException.TypeMismatch(Condition.Source);

		if (!value.IsZero)
			context.Dispatch(ThenBody, stackFrame);
		else
			context.Dispatch(ElseBody, stackFrame);
	}
}
