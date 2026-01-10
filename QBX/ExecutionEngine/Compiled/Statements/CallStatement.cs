using QBX.ExecutionEngine.Execution.Variables;
using System;
using System.Collections.Generic;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class CallStatement(CodeModel.Statements.Statement? source) : Executable(source)
{
	public Routine? Target;
	public readonly List<Evaluable> Arguments = new List<Evaluable>();

	public string? UnresolvedTargetName;

	public override void Execute(Execution.ExecutionContext context, Execution.StackFrame stackFrame)
	{
		if (UnresolvedTargetName != null)
			throw CompilerException.SubprogramNotDefined(Source);

		if (Target == null)
			throw new Exception("CallStatement has no Target");

		var arguments = new Variable[Arguments.Count];

		for (int i = 0; i < arguments.Length; i++)
			arguments[i] = Arguments[i].Evaluate(context, stackFrame);

		context.Call(Target, arguments);
	}
}
