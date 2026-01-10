using System;
using System.Collections.Generic;
using System.Linq;

namespace QBX.ExecutionEngine.Compiled;

public class Sequence
{
	public Executable? OwnerExecutable;

	public void GetPathToStatement(StatementPath path)
	{
		if (this is Routine)
			return;

		if (OwnerExecutable == null)
			throw new Exception("Internal error: Sequence does not have a reference to its OwnerExecutable");

		int index = OwnerExecutable.IndexOfSequence(this);

		if (index < 0)
			throw new Exception("Internal error: Sequence refers to an OwnerExecutable that can't provide an index for it");

		path.Push(index);

		OwnerExecutable.GetPathToStatement(path);
	}

	List<Executable> _statements = new List<Executable>();
	List<Executable>? _injectedStatements;

	public IReadOnlyList<Executable> Statements => _statements;
	public IEnumerable<Executable> InjectedStatements => _injectedStatements ?? Enumerable.Empty<Executable>();

	public IEnumerable<Executable> AllStatements
	{
		get
		{
			foreach (var statement in Statements)
			{
				yield return statement;

				int subsequenceCount = statement.GetSequenceCount();

				for (int subsequenceIndex = 0; subsequenceIndex < subsequenceCount; subsequenceIndex++)
				{
					var subsequence = statement.GetSequenceByIndex(subsequenceIndex);

					if (subsequence != null)
						foreach (var nestedStatement in subsequence.AllStatements)
							yield return nestedStatement;
				}
			}
		}
	}

	public Executable this[int index] => Statements[index];
	public int Count => Statements.Count;

	public void Append(Executable executable)
	{
		executable.Sequence = this;
		executable.Index = _statements.Count;
		_statements.Add(executable);
	}

	public void AppendIfNotNull(Executable? executable)
	{
		if (executable != null)
			Append(executable);
	}

	public void Inject(Executable executable)
	{
		executable.Sequence = this;

		_injectedStatements ??= new List<Executable>();
		_injectedStatements.Add(executable);

		// Injected statements may not participate in GOTO/GOSUB.
		executable.Index = -1;
	}
}
