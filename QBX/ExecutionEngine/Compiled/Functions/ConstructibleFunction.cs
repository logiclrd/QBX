using System;

namespace QBX.ExecutionEngine.Compiled.Functions;

public abstract class ConstructibleFunction : Function
{
	protected sealed override void SetArgument(int index, Evaluable value)
	{
		throw new Exception("Constructing function type " + GetType().Name + " does not support setting argument(s) after construction");
	}
}
