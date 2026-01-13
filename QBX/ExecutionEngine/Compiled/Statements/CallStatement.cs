using QBX.ExecutionEngine.Execution.Variables;
using System;
using System.Collections.Generic;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class CallStatement(CodeModel.Statements.Statement? source) : Executable(source), IUnresolvedCall
{
	public Routine? Target;
	public readonly List<Evaluable> Arguments = new List<Evaluable>();

	public string? UnresolvedTargetName;

	public void Resolve(Routine routine)
	{
		if (UnresolvedTargetName == null)
			throw new Exception("Internal error: Resolving a reference in a CallStatement that is not unresolved");
		if (routine.Name != UnresolvedTargetName)
			throw new Exception("Internal error: Resolving a reference to '" + UnresolvedTargetName + "' with a routine named '" + routine.Name + "'");

		Target = routine;
		UnresolvedTargetName = null;
	}

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
