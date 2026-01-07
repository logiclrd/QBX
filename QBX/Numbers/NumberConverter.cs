using System;

using QBX.ExecutionEngine;
using QBX.ExecutionEngine.Execution.Variables;
using QBX.LexicalAnalysis;

namespace QBX.Numbers;

public static class NumberConverter
{
	public static short ToInteger(IntegerVariable value, Token? context = null) => value.Value;
	public static short ToInteger(short value, Token? context = null) => value;

	public static short ToInteger(LongVariable value, Token? context = null) => ToInteger(value.Value, context);
	public static short ToInteger(int value, Token? context = null)
	{
		if ((value < short.MinValue) || (value > short.MaxValue))
			throw RuntimeException.Overflow(context);

		return (short)value;
	}

	public static short ToInteger(SingleVariable value, Token? context = null) => ToInteger(value.Value, context);
	public static short ToInteger(float value, Token? context = null)
	{
		value = float.Round(value);

		if ((value < short.MinValue) || (value > short.MaxValue))
			throw RuntimeException.Overflow(context);

		return (short)value;
	}

	public static short ToInteger(DoubleVariable value, Token? context = null) => ToInteger(value.Value, context);
	public static short ToInteger(double value, Token? context = null)
	{
		value = double.Round(value);

		if ((value < short.MinValue) || (value > short.MaxValue))
			throw RuntimeException.Overflow(context);

		return (short)value;
	}

	public static short ToInteger(CurrencyVariable value, Token? context = null) => ToInteger(value.Value, context);
	public static short ToInteger(decimal value, Token? context = null)
	{
		value = decimal.Round(value);

		if ((value < short.MinValue) || (value > short.MaxValue))
			throw RuntimeException.Overflow(context);

		return (short)value;
	}

	public static int ToLong(IntegerVariable value, Token? context = null) => ToLong(value.Value, context);
	public static int ToLong(short value, Token? context = null) => value;

	public static int ToLong(LongVariable value, Token? context = null) => value.Value;
	public static int ToLong(int value, Token? context = null) => value;

	public static int ToLong(SingleVariable value, Token? context = null) => ToLong(value.Value, context);
	public static int ToLong(float value, Token? context = null)
	{
		value = float.Round(value);

		if ((value < int.MinValue) || (value > int.MaxValue))
			throw RuntimeException.Overflow(context);

		return (int)value;
	}

	public static int ToLong(DoubleVariable value, Token? context = null) => ToLong(value.Value, context);
	public static int ToLong(double value, Token? context = null)
	{
		value = double.Round(value);

		if ((value < int.MinValue) || (value > int.MaxValue))
			throw RuntimeException.Overflow(context);

		return (int)value;
	}

	public static int ToLong(CurrencyVariable value, Token? context = null) => ToLong(value.Value, context);
	public static int ToLong(decimal value, Token? context = null)
	{
		value = decimal.Round(value);

		if ((value < int.MinValue) || (value > int.MaxValue))
			throw RuntimeException.Overflow(context);

		return (int)value;
	}

	public static float ToSingle(IntegerVariable value, Token? context = null) => ToSingle(value.Value, context);
	public static float ToSingle(short value, Token? context = null) => value;

	public static float ToSingle(LongVariable value, Token? context = null) => ToSingle(value.Value, context);
	public static float ToSingle(int value, Token? context = null) => value;

	public static float ToSingle(SingleVariable value, Token? context = null) => value.Value;
	public static float ToSingle(float value, Token? context = null) => value;

	public static float ToSingle(DoubleVariable value, Token? context = null) => ToSingle(value.Value, context);
	public static float ToSingle(double value, Token? context = null)
	{
		try
		{
			return (float)value;
		}
		catch (OverflowException)
		{
			throw RuntimeException.Overflow(context);
		}
	}

	public static float ToSingle(CurrencyVariable value, Token? context = null) => ToSingle(value.Value, context);
	public static float ToSingle(decimal value, Token? context = null)
	{
		try
		{
			return (float)value;
		}
		catch (OverflowException)
		{
			throw RuntimeException.Overflow(context);
		}
	}

	public static double ToDouble(IntegerVariable value, Token? context = null) => ToDouble(value.Value, context);
	public static double ToDouble(short value, Token? context = null) => value;

	public static double ToDouble(LongVariable value, Token? context = null) => ToDouble(value.Value, context);
	public static double ToDouble(int value, Token? context = null) => value;

	public static double ToDouble(SingleVariable value, Token? context = null) => ToDouble(value.Value, context);
	public static double ToDouble(float value, Token? context = null) => value;

	public static double ToDouble(DoubleVariable value, Token? context = null) => value.Value;
	public static double ToDouble(double value, Token? context = null) => value;

	public static double ToDouble(CurrencyVariable value, Token? context = null) => ToDouble(value.Value, context);
	public static double ToDouble(decimal value, Token? context = null)
	{
		try
		{
			return (double)value;
		}
		catch (OverflowException)
		{
			throw RuntimeException.Overflow(context);
		}
	}

	public static decimal ToCurrency(IntegerVariable value, Token? context = null) => ToCurrency(value.Value, context);
	public static decimal ToCurrency(short value, Token? context = null) => value;

	public static decimal ToCurrency(LongVariable value, Token? context = null) => ToCurrency(value.Value, context);
	public static decimal ToCurrency(int value, Token? context = null) => value;

	public static decimal ToCurrency(SingleVariable value, Token? context = null) => ToCurrency(value.Value, context);
	public static decimal ToCurrency(float value, Token? context = null)
	{
		try
		{
			var decimalValue = (decimal)value;

			if (!decimalValue.IsInCurrencyRange())
				throw RuntimeException.Overflow(context);

			return decimalValue;
		}
		catch (OverflowException)
		{
			throw RuntimeException.Overflow(context);
		}
	}

	public static decimal ToCurrency(DoubleVariable value, Token? context = null) => ToCurrency(value.Value, context);
	public static decimal ToCurrency(double value, Token? context = null)
	{
		try
		{
			var decimalValue = (decimal)value;

			if (!decimalValue.IsInCurrencyRange())
				throw RuntimeException.Overflow(context);

			return decimalValue;
		}
		catch (OverflowException)
		{
			throw RuntimeException.Overflow(context);
		}
	}

	public static decimal ToCurrency(CurrencyVariable value, Token? context = null) => value.Value;
	public static decimal ToCurrency(decimal value, Token? context = null) => value;

	public static short ToInteger(object value, Token? context = null)
	{
		switch (value)
		{
			case short integerValue: return ToInteger(integerValue, context);
			case int longValue: return ToInteger(longValue, context);
			case float singleValue: return ToInteger(singleValue, context);
			case double doubleValue: return ToInteger(doubleValue, context);
			case decimal decimalValue: return ToInteger(decimalValue, context);

			case IntegerVariable integerValue: return ToInteger(integerValue, context);
			case LongVariable longValue: return ToInteger(longValue, context);
			case SingleVariable singleValue: return ToInteger(singleValue, context);
			case DoubleVariable doubleValue: return ToInteger(doubleValue, context);
			case CurrencyVariable decimalValue: return ToInteger(decimalValue, context);
		}

		throw CompilerException.TypeMismatch(context);
	}

	public static int ToLong(object value, Token? context = null)
	{
		switch (value)
		{
			case short integerValue: return ToLong(integerValue, context);
			case int longValue: return ToLong(longValue, context);
			case float singleValue: return ToLong(singleValue, context);
			case double doubleValue: return ToLong(doubleValue, context);
			case decimal decimalValue: return ToLong(decimalValue, context);

			case IntegerVariable integerValue: return ToLong(integerValue, context);
			case LongVariable longValue: return ToLong(longValue, context);
			case SingleVariable singleValue: return ToLong(singleValue, context);
			case DoubleVariable doubleValue: return ToLong(doubleValue, context);
			case CurrencyVariable decimalValue: return ToLong(decimalValue, context);
		}

		throw CompilerException.TypeMismatch(context);
	}

	public static float ToSingle(object value, Token? context = null)
	{
		switch (value)
		{
			case short integerValue: return ToSingle(integerValue, context);
			case int longValue: return ToSingle(longValue, context);
			case float singleValue: return ToSingle(singleValue, context);
			case double doubleValue: return ToSingle(doubleValue, context);
			case decimal decimalValue: return ToSingle(decimalValue, context);

			case IntegerVariable integerValue: return ToSingle(integerValue, context);
			case LongVariable longValue: return ToSingle(longValue, context);
			case SingleVariable singleValue: return ToSingle(singleValue, context);
			case DoubleVariable doubleValue: return ToSingle(doubleValue, context);
			case CurrencyVariable decimalValue: return ToSingle(decimalValue, context);
		}

		throw CompilerException.TypeMismatch(context);
	}

	public static double ToDouble(object value, Token? context = null)
	{
		switch (value)
		{
			case short integerValue: return ToDouble(integerValue, context);
			case int longValue: return ToDouble(longValue, context);
			case float singleValue: return ToDouble(singleValue, context);
			case double doubleValue: return ToDouble(doubleValue, context);
			case decimal decimalValue: return ToDouble(decimalValue, context);

			case IntegerVariable integerValue: return ToDouble(integerValue, context);
			case LongVariable longValue: return ToDouble(longValue, context);
			case SingleVariable singleValue: return ToDouble(singleValue, context);
			case DoubleVariable doubleValue: return ToDouble(doubleValue, context);
			case CurrencyVariable decimalValue: return ToDouble(decimalValue, context);
		}

		throw CompilerException.TypeMismatch(context);
	}

	public static decimal ToCurrency(object value, Token? context = null)
	{
		switch (value)
		{
			case short integerValue: return ToCurrency(integerValue, context);
			case int longValue: return ToCurrency(longValue, context);
			case float singleValue: return ToCurrency(singleValue, context);
			case double doubleValue: return ToCurrency(doubleValue, context);
			case decimal decimalValue: return ToCurrency(decimalValue, context);

			case IntegerVariable integerValue: return ToCurrency(integerValue, context);
			case LongVariable longValue: return ToCurrency(longValue, context);
			case SingleVariable singleValue: return ToCurrency(singleValue, context);
			case DoubleVariable doubleValue: return ToCurrency(doubleValue, context);
			case CurrencyVariable decimalValue: return ToCurrency(decimalValue, context);
		}

		throw CompilerException.TypeMismatch(context);
	}
}
