using System;
using System.Collections.Generic;
using System.Linq;

using QBX.ExecutionEngine.Compiled.Expressions;
using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;
using QBX.LexicalAnalysis;

namespace QBX.ExecutionEngine.Compiled.Functions;

public abstract class FixFunction : ConstructibleFunction
{
	Evaluable? _argumentExpression;

	public Evaluable? ArgumentExpression => _argumentExpression;

	public override bool IsConstant => _argumentExpression?.IsConstant ?? false;

	protected FixFunction(Evaluable? argumentExpression)
	{
		_argumentExpression = argumentExpression;
	}

	public override void CollapseConstantSubexpressions()
	{
		CollapseConstantExpression(ref _argumentExpression);
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
			case PrimitiveDataType.Integer: return arg;
			case PrimitiveDataType.Long: return arg;
			case PrimitiveDataType.Single: return new SingleFixFunction(arg);
			case PrimitiveDataType.Double: return new DoubleFixFunction(arg);
			case PrimitiveDataType.Currency: return new CurrencyFixFunction(arg);
		}

		throw new Exception("Internal error");
	}
}

public class SingleFixFunction(Evaluable argument) : FixFunction(argument)
{
	public override DataType Type => DataType.Single;

	public override Variable Evaluate(ExecutionContext context, StackFrame stackFrame)
	{
		var value = (SingleVariable)argument.Evaluate(context, stackFrame);

		return new SingleVariable(float.Truncate(value.Value));
	}

	public override LiteralValue EvaluateConstant()
	{
		var value = (SingleLiteralValue)argument.EvaluateConstant();

		return new SingleLiteralValue(float.Truncate(value.Value));
	}
}

public class DoubleFixFunction(Evaluable argument) : FixFunction(argument)
{
	public override DataType Type => DataType.Double;

	public override Variable Evaluate(ExecutionContext context, StackFrame stackFrame)
	{
		var value = (DoubleVariable)argument.Evaluate(context, stackFrame);

		return new DoubleVariable(double.Truncate(value.Value));
	}

	public override LiteralValue EvaluateConstant()
	{
		var value = (DoubleLiteralValue)argument.EvaluateConstant();

		return new DoubleLiteralValue(double.Truncate(value.Value));
	}
}

public class CurrencyFixFunction(Evaluable argument) : FixFunction(argument)
{
	public override DataType Type => DataType.Currency;

	public override Variable Evaluate(ExecutionContext context, StackFrame stackFrame)
	{
		var value = (CurrencyVariable)argument.Evaluate(context, stackFrame);

		return new CurrencyVariable(decimal.Truncate(value.Value));
	}

	public override LiteralValue EvaluateConstant()
	{
		var value = (CurrencyLiteralValue)argument.EvaluateConstant();

		return new CurrencyLiteralValue(decimal.Truncate(value.Value));
	}
}
