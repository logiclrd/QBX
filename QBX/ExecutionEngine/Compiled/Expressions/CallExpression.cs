using System;
using System.Collections.Generic;

using QBX.ExecutionEngine.Execution.Variables;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class CallExpression : Evaluable, IUnresolvedCall
{
	public Routine? Target;
	public readonly List<Evaluable> Arguments = new List<Evaluable>();

	public string? UnresolvedTargetName;
	public DataType? UnresolvedTargetType;

	public override DataType Type =>
		Target?.ReturnType ??
		UnresolvedTargetType ??
		throw new Exception("Internal error: CallExpression has no Type");

	public void Resolve(Routine routine)
	{
		if (UnresolvedTargetName == null)
			throw new Exception("Internal error: Resolving a reference in a CallExpression that is not unresolved");
		if (routine.Name != UnresolvedTargetName)
			throw new Exception("Internal error: Resolving a reference to '" + UnresolvedTargetName + "' with a routine named '" + routine.Name + "'");

		Target = routine;
		UnresolvedTargetName = null;
	}

	public override void CollapseConstantSubexpressions()
	{
		for (int i = 0; i < Arguments.Count; i++)
			CollapseConstantExpression(Arguments, i);
	}

	public override Variable Evaluate(Execution.ExecutionContext context, Execution.StackFrame stackFrame)
	{
		if (UnresolvedTargetName != null)
			throw CompilerException.SubprogramNotDefined(SourceStatement);

		if (Target == null)
			throw new Exception("CallStatement has no Target");

		var arguments = new Variable[Arguments.Count];

		for (int i = 0; i < arguments.Length; i++)
			arguments[i] = Arguments[i].Evaluate(context, stackFrame);

		return context.Call(Target, arguments);
	}
}
