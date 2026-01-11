using System;
using System.Collections.Generic;

using QBX.ExecutionEngine.Execution;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class SelectCaseStatement(CodeModel.Statements.Statement? source) : Executable(source)
{
	public Evaluable? TestExpression;
	public List<CaseBlock> Cases = new List<CaseBlock>();

	public override int IndexOfSequence(Sequence sequence)
	{
		if (sequence is CaseBlock caseBlock)
		{
			int index = Cases.IndexOf(caseBlock);

			if (index >= 0)
				return index;
		}

		throw new Exception("Internal error: Sequence is not owned by this statement");
	}

	public override int GetSequenceCount() => Cases.Count;

	public override Sequence? GetSequenceByIndex(int sequenceIndex) => Cases[sequenceIndex];

	public override void Execute(ExecutionContext context, StackFrame stackFrame)
	{
		if (TestExpression == null)
			throw new Exception("SelectCaseStatement with no TestExpression");

		var testValue = TestExpression.Evaluate(context, stackFrame);

		foreach (var @case in Cases)
		{
			if (@case.IsMatch(testValue, context, stackFrame))
			{
				context.Dispatch(@case, stackFrame);
				break;
			}
		}
	}
}
