using System;
using System.Collections.Generic;

namespace QBX.ExecutionEngine.Compiled.Functions;

public abstract class Function : Evaluable
{
	protected virtual int MinArgumentCount => 1;
	protected virtual int MaxArgumentCount => 1;

	protected virtual void SetArgument(int index, Evaluable value)
		=> throw new Exception("Function.SetArgument called on a function type that doesn't define it");

	public virtual void SetArguments(IEnumerable<Evaluable?> arguments)
	{
		int count = 0;
		int minCount = MinArgumentCount;
		int maxCount = MaxArgumentCount;

		foreach (var argument in arguments)
		{
			if (argument == null)
				throw new Exception("Argument expression translated to null");

			if (count >= maxCount)
				throw CompilerException.ArgumentCountMismatch(SourceExpression?.Token);

			SetArgument(count++, argument);
		}

		if (count < minCount)
			throw CompilerException.ArgumentCountMismatch(SourceExpression?.Token);
	}
}
