using System;
using System.Collections.Generic;
using System.Linq;

using QBX.ExecutionEngine.Compiled.Expressions;
using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;
using QBX.LexicalAnalysis;
using QBX.Numbers;

namespace QBX.ExecutionEngine.Compiled.Functions;

public abstract class AbsFunction : ConstructibleFunction
{
	Evaluable? _argumentExpression;

	public Evaluable? ArgumentExpression => _argumentExpression;

	public override bool IsConstant => _argumentExpression?.IsConstant ?? false;

	protected AbsFunction(Evaluable? argumentExpression)
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
			case PrimitiveDataType.Integer: return new IntegerAbsFunction(arg);
			case PrimitiveDataType.Long: return new LongAbsFunction(arg);
			case PrimitiveDataType.Single: return new SingleAbsFunction(arg);
			case PrimitiveDataType.Double: return new DoubleAbsFunction(arg);
			case PrimitiveDataType.Currency: return new CurrencyAbsFunction(arg);
		}

		throw new Exception("Internal error");
	}
}

public class IntegerAbsFunction(Evaluable argument) : AbsFunction(argument)
{
	public override DataType Type => DataType.Integer;

	public override Variable Evaluate(ExecutionContext context, StackFrame stackFrame)
	{
		var value = (IntegerVariable)argument.Evaluate(context, stackFrame);

		return new IntegerVariable(short.Abs(value.Value));
	}

	public override LiteralValue EvaluateConstant()
	{
		var value = argument.EvaluateConstant();

		return new IntegerLiteralValue(short.Abs(NumberConverter.ToInteger(value)));
	}
}

public class LongAbsFunction(Evaluable argument) : AbsFunction(argument)
{
	public override DataType Type => DataType.Long;

	public override Variable Evaluate(ExecutionContext context, StackFrame stackFrame)
	{
		var value = (LongVariable)argument.Evaluate(context, stackFrame);

		return new LongVariable(int.Abs(value.Value));
	}

	public override LiteralValue EvaluateConstant()
	{
		var value = argument.EvaluateConstant();

		return new LongLiteralValue(int.Abs(NumberConverter.ToLong(value)));
	}
}

public class SingleAbsFunction(Evaluable argument) : AbsFunction(argument)
{
	public override DataType Type => DataType.Single;

	public override Variable Evaluate(ExecutionContext context, StackFrame stackFrame)
	{
		var value = (SingleVariable)argument.Evaluate(context, stackFrame);

		return new SingleVariable(float.Abs(value.Value));
	}

	public override LiteralValue EvaluateConstant()
	{
		var value = argument.EvaluateConstant();

		return new SingleLiteralValue(float.Abs(NumberConverter.ToSingle(value)));
	}
}

public class DoubleAbsFunction(Evaluable argument) : AbsFunction(argument)
{
	public override DataType Type => DataType.Double;

	public override Variable Evaluate(ExecutionContext context, StackFrame stackFrame)
	{
		var value = (DoubleVariable)argument.Evaluate(context, stackFrame);

		return new DoubleVariable(double.Abs(value.Value));
	}

	public override LiteralValue EvaluateConstant()
	{
		var value = argument.EvaluateConstant();

		return new DoubleLiteralValue(double.Abs(NumberConverter.ToDouble(value)));
	}
}

public class CurrencyAbsFunction(Evaluable argument) : AbsFunction(argument)
{
	public override DataType Type => DataType.Currency;

	public override Variable Evaluate(ExecutionContext context, StackFrame stackFrame)
	{
		var value = (CurrencyVariable)argument.Evaluate(context, stackFrame);

		return new CurrencyVariable(decimal.Abs(value.Value));
	}

	public override LiteralValue EvaluateConstant()
	{
		var value = argument.EvaluateConstant();

		return new CurrencyLiteralValue(decimal.Abs(NumberConverter.ToCurrency(value)));
	}
}
