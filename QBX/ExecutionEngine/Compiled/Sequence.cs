using System.Collections.Generic;

namespace QBX.ExecutionEngine.Compiled;

public class Sequence : ISequence
{
	public List<IExecutable> Statements = new List<IExecutable>();

	public IExecutable this[int index] => Statements[index];
	public int Count => Statements.Count;
	public void Append(IExecutable executable) => Statements.Add(executable);
	public void Prepend(IExecutable executable) => Statements.Insert(0, executable);
}
