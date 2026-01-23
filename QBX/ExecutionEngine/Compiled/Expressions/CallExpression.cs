using System;
using System.Collections.Generic;

using QBX.ExecutionEngine.Compiled.Operations;
using QBX.ExecutionEngine.Execution.Variables;
using QBX.LexicalAnalysis;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class CallExpression : Evaluable, IUnresolvedCall
{
	public Routine? Target;
	public readonly List<Evaluable> Arguments = new List<Evaluable>();

	public string? UnresolvedTargetName;
	public DataType? UnresolvedTargetType;
	public Token? UnresolvedTargetToken;

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

		EnsureParameterTypes();
	}

	public override void CollapseConstantSubexpressions()
	{
		for (int i = 0; i < Arguments.Count; i++)
			CollapseConstantExpression(Arguments, i);
	}

	public void EnsureParameterTypes()
	{
		if (Target == null)
			throw new Exception("Internal error: EnsureParameterTypes called with no Target");

		if (Arguments.Count != Target.ParameterTypes.Count)
			throw new Exception("Internal error: CallExpression configured with wrong number of arguments for the target routine");

		for (int i = 0; i < Arguments.Count; i++)
		{
			var argument = Arguments[i];
			var parameterType = Target.ParameterTypes[i];

			if (argument.Type.IsString != parameterType.IsString)
				throw CompilerException.TypeMismatch(argument?.Source);

			if (parameterType.IsUserType
			 && (!argument.Type.IsUserType || (argument.Type.UserType != parameterType.UserType)))
				throw CompilerException.TypeMismatch(argument?.Source);

			if (parameterType.IsNumeric)
			{
				var converted = Conversion.Construct(argument, parameterType.PrimitiveType);

				if (argument != converted)
					Arguments[i] = converted;

				continue;
			}
		}
	}

	public override Variable Evaluate(Execution.ExecutionContext context, Execution.StackFrame stackFrame)
	{
		if (UnresolvedTargetName != null)
			throw CompilerException.SubprogramNotDefined(UnresolvedTargetToken ?? Source?.Token);

		if (Target == null)
			throw new Exception("CallStatement has no Target");

		var arguments = new Variable[Arguments.Count];

		for (int i = 0; i < arguments.Length; i++)
			arguments[i] = Arguments[i].Evaluate(context, stackFrame);

		return context.Call(Target, arguments);
	}
}
