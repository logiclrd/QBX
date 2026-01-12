using System;
using System.Collections.Generic;

using QBX.ExecutionEngine.Execution.Variables;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class CallExpression : Evaluable
{
	public Routine? Target;
	public readonly List<Evaluable> Arguments = new List<Evaluable>();

	public string? UnresolvedTargetName;

	public override DataType Type => throw new NotImplementedException();

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
