using System;
using System.Collections.Generic;

using QBX.LexicalAnalysis;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class CallStatement : IExecutable
{
	public Routine? Target;
	public readonly List<IEvaluable> Arguments = new List<IEvaluable>();

	public string? UnresolvedTargetName;

	public CodeModel.Statements.CallStatement? Source { get; set; }

	public void Execute(Execution.ExecutionContext context, bool stepInto)
	{
		if (UnresolvedTargetName != null)
			throw CompilerException.SubprogramNotDefined(Source?.TargetNameToken);

		if (Target == null)
			throw new Exception("CallStatement has no Target");

		// TODO
	}
}
