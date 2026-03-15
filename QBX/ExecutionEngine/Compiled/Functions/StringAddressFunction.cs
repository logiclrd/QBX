using QBX.ExecutionEngine.Execution.Variables;

namespace QBX.ExecutionEngine.Compiled.Functions;

public abstract class StringAddressFunction : VariableAddressFunction
{
	protected override void SetArgument(int index, Evaluable value)
	{
		if (!value.Type.IsString)
			throw CompilerException.TypeMismatch(Source);

		base.SetArgument(index, value);
	}

	protected override bool Validate(Variable variable)
		=> ((StringVariable)variable).ValueSpan.Length > 0;
}
