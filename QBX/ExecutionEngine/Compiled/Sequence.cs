using System;
using System.Collections.Generic;
using QBX.ExecutionEngine.Execution;

namespace QBX.ExecutionEngine.Compiled;

public class Sequence : IExecutable, ISequence
{
	public List<IExecutable> Statements = new List<IExecutable>();

	public IExecutable this[int index] => Statements[index];
	public int Count => Statements.Count;
	public void Append(IExecutable executable) => Statements.Add(executable);

	public void Execute(ExecutionContext context, bool stepInto)
	{
		context.PushScope();
		context.CurrentFrame.CurrentSequence = this;
		context.CurrentFrame.NextStatement = null;
		context.CurrentFrame.NextStatementIndex = 0;
	}
}
