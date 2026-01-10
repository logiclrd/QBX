using System.Collections.Generic;

namespace QBX.ExecutionEngine.Compiled;

public class Sequence
{
	public List<Executable> Statements = new List<Executable>();

	public Executable this[int index] => Statements[index];
	public int Count => Statements.Count;
	public void Append(Executable executable) => Statements.Add(executable);
	public void Prepend(Executable executable) => Statements.Insert(0, executable);
}
