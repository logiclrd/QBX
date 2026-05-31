using System;
using System.Collections.Generic;
using System.Linq;

using QBX.ExecutionEngine.Execution;
using QBX.LexicalAnalysis;

namespace QBX.ExecutionEngine.Compiled.Functions;

public abstract class ConstructibleOneArgumentMathFunction<TSingleVariantFunction, TDoubleVariantFunction> : OneArgumentMathFunction
	where TSingleVariantFunction : OneArgumentMathFunction, new()
	where TDoubleVariantFunction : OneArgumentMathFunction, new()
{
	static TSingleVariantFunction ConstructSingleVariant(Evaluable argument)
	{
		var function = new TSingleVariantFunction();

		function.SetArgument(argument);

		return function;
	}

	static TDoubleVariantFunction ConstructDoubleVariant(Evaluable argument)
	{
		var function = new TDoubleVariantFunction();

		function.SetArgument(argument);

		return function;
	}

	public static Evaluable Construct(Token? token, IEnumerable<Evaluable> arguments)
	{
		var argList = arguments.Take(2).ToList();

		if (argList.Count != 1)
			throw CompilerException.ArgumentCountMismatch(token);

		var arg = argList[0];

		if (!arg.Type.IsNumeric)
			throw CompilerException.TypeMismatch(token);

		switch (arg.Type.PrimitiveType)
		{
			case PrimitiveDataType.Integer: return ConstructSingleVariant(arg);
			case PrimitiveDataType.Long: return ConstructDoubleVariant(arg);
			case PrimitiveDataType.Single: return ConstructSingleVariant(arg);
			case PrimitiveDataType.Double: return ConstructDoubleVariant(arg);
			case PrimitiveDataType.Currency: return ConstructSingleVariant(arg);
		}

		throw new Exception("Internal error");
	}
}
