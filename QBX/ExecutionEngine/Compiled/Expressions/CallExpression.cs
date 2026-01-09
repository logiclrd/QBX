using System;
using System.Collections.Generic;

using QBX.ExecutionEngine.Compiled.Expressions;
using QBX.ExecutionEngine.Execution.Variables;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class CallExpression : Expression
{
	public Routine? Target;
	public readonly List<IEvaluable> Arguments = new List<IEvaluable>();

	public string? UnresolvedTargetName;

	public override DataType Type => throw new NotImplementedException();

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
