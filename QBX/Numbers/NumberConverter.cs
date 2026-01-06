using System;

using QBX.ExecutionEngine;
using QBX.LexicalAnalysis;

namespace QBX.Numbers;

public static class NumberConverter
{
	public static short ToInteger(short value, Token? context = null) => value;

	public static short ToInteger(int value, Token? context = null)
	{
		if ((value < short.MinValue) || (value > short.MaxValue))
			throw new RuntimeException(context, "Overflow");

		return (short)value;
	}

	public static short ToInteger(float value, Token? context = null)
	{
		value = float.Round(value);

		if ((value < short.MinValue) || (value > short.MaxValue))
			throw new RuntimeException(context, "Overflow");

		return (short)value;
	}

	public static short ToInteger(double value, Token? context = null)
	{
		value = double.Round(value);

		if ((value < short.MinValue) || (value > short.MaxValue))
			throw new RuntimeException(context, "Overflow");

		return (short)value;
	}

	public static short ToInteger(decimal value, Token? context = null)
	{
		value = decimal.Round(value);

		if ((value < short.MinValue) || (value > short.MaxValue))
			throw new RuntimeException(context, "Overflow");

		return (short)value;
	}

	public static int ToLong(short value, Token? context = null) => value;

	public static int ToLong(int value, Token? context = null) => value;

	public static int ToLong(float value, Token? context = null)
	{
		value = float.Round(value);

		if ((value < int.MinValue) || (value > int.MaxValue))
			throw new RuntimeException(context, "Overflow");

		return (int)value;
	}

	public static int ToLong(double value, Token? context = null)
	{
		value = double.Round(value);

		if ((value < int.MinValue) || (value > int.MaxValue))
			throw new RuntimeException(context, "Overflow");

		return (int)value;
	}

	public static int ToLong(decimal value, Token? context = null)
	{
		value = decimal.Round(value);

		if ((value < int.MinValue) || (value > int.MaxValue))
			throw new RuntimeException(context, "Overflow");

		return (int)value;
	}

	public static float ToSingle(short value, Token? context = null) => value;

	public static float ToSingle(int value, Token? context = null) => value;

	public static float ToSingle(float value, Token? context = null) => value;

	public static float ToSingle(double value, Token? context = null)
	{
		try
		{
			return (float)value;
		}
		catch (OverflowException)
		{
			throw new RuntimeException(context, "Overflow");
		}
	}

	public static float ToSingle(decimal value, Token? context = null)
	{
		try
		{
			return (float)value;
		}
		catch (OverflowException)
		{
			throw new RuntimeException(context, "Overflow");
		}
	}

	public static double ToDouble(short value, Token? context = null) => value;

	public static double ToDouble(int value, Token? context = null) => value;

	public static double ToDouble(float value, Token? context = null) => value;

	public static double ToDouble(double value, Token? context = null) => value;

	public static double ToDouble(decimal value, Token? context = null)
	{
		try
		{
			return (double)value;
		}
		catch (OverflowException)
		{
			throw new RuntimeException(context, "Overflow");
		}
	}

	public static decimal ToCurrency(short value, Token? context = null) => value;

	public static decimal ToCurrency(int value, Token? context = null) => value;

	public static decimal ToCurrency(float value, Token? context = null)
	{
		try
		{
			var decimalValue = (decimal)value;

			if (!decimalValue.IsInCurrencyRange())
				throw new RuntimeException(context, "Overflow");

			return decimalValue;
		}
		catch (OverflowException)
		{
			throw new RuntimeException(context, "Overflow");
		}
	}

	public static decimal ToCurrency(double value, Token? context = null)
	{
		try
		{
			var decimalValue = (decimal)value;

			if (!decimalValue.IsInCurrencyRange())
				throw new RuntimeException(context, "Overflow");

			return decimalValue;
		}
		catch (OverflowException)
		{
			throw new RuntimeException(context, "Overflow");
		}
	}

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
		}

		throw new RuntimeException(context, "Type mismatch");
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
		}

		throw new RuntimeException(context, "Type mismatch");
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
		}

		throw new RuntimeException(context, "Type mismatch");
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
		}

		throw new RuntimeException(context, "Type mismatch");
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
		}

		throw new RuntimeException(context, "Type mismatch");
	}
}
