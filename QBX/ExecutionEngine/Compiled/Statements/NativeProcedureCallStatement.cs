using System;
using System.Collections.Generic;

using QBX.ExecutionEngine.Compiled.Operations;
using QBX.ExecutionEngine.Execution.Variables;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class NativeProcedureCallStatement(CodeModel.Statements.Statement? source) : Executable(source)
{
	public NativeProcedure? Target;
	public Func<Variable[], Variable>? LocalThunk;
	public readonly List<Evaluable> Arguments = new List<Evaluable>();

	public void EnsureParameterTypes()
	{
		if (Target == null)
			throw new Exception("Internal error: EnsureParameterTypes called with no Target");

		if (Arguments.Count != (Target.ParameterTypes?.Length ?? 0))
			throw new Exception("Internal error: CallStatement configured with wrong number of arguments for the target routine");

		if (Target.ParameterTypes == null)
			return;

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

	public override void Execute(Execution.ExecutionContext context, Execution.StackFrame stackFrame)
	{
		if (Target == null)
			throw new Exception("NativeProcedureCallStatement has no Target");
		if (Target.Invoke == null)
			throw new Exception("Internal error: NativeProcedure has not had its thunk built");

		var arguments = new Variable[Arguments.Count];

		for (int i = 0; i < arguments.Length; i++)
			arguments[i] = Arguments[i].Evaluate(context, stackFrame);

		try
		{
			if (LocalThunk != null)
				LocalThunk.Invoke(arguments);
			else
				Target.Invoke(arguments);
		}
		catch (RuntimeException error)
		{
			throw error.AddContext(Source);
		}
	}
}
