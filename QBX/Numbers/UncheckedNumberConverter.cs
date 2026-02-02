using QBX.ExecutionEngine;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Xml;

namespace QBX.Numbers;

public static class UncheckedNumberConverter
{
	public static byte ToByte(byte value) => unchecked((byte)value);
	public static byte ToByte(sbyte value) => unchecked((byte)value);
	public static byte ToByte(short value) => unchecked((byte)value);
	public static byte ToByte(ushort value) => unchecked((byte)value);
	public static byte ToByte(int value) => unchecked((byte)value);
	public static byte ToByte(uint value) => unchecked((byte)value);
	public static byte ToByte(long value) => unchecked((byte)value);
	public static byte ToByte(ulong value) => unchecked((byte)value);
	public static byte ToByte(float value) => unchecked((byte)value);
	public static byte ToByte(double value) => unchecked((byte)value);
	public static byte ToByte(decimal value) => unchecked((byte)value);

	public static sbyte ToSByte(byte value) => unchecked((sbyte)value);
	public static sbyte ToSByte(sbyte value) => unchecked((sbyte)value);
	public static sbyte ToSByte(short value) => unchecked((sbyte)value);
	public static sbyte ToSByte(ushort value) => unchecked((sbyte)value);
	public static sbyte ToSByte(int value) => unchecked((sbyte)value);
	public static sbyte ToSByte(uint value) => unchecked((sbyte)value);
	public static sbyte ToSByte(long value) => unchecked((sbyte)value);
	public static sbyte ToSByte(ulong value) => unchecked((sbyte)value);
	public static sbyte ToSByte(float value) => unchecked((sbyte)value);
	public static sbyte ToSByte(double value) => unchecked((sbyte)value);
	public static sbyte ToSByte(decimal value) => unchecked((sbyte)value);

	public static short ToInt16(byte value) => unchecked((short)value);
	public static short ToInt16(sbyte value) => unchecked((short)value);
	public static short ToInt16(short value) => unchecked((short)value);
	public static short ToInt16(ushort value) => unchecked((short)value);
	public static short ToInt16(int value) => unchecked((short)value);
	public static short ToInt16(uint value) => unchecked((short)value);
	public static short ToInt16(long value) => unchecked((short)value);
	public static short ToInt16(ulong value) => unchecked((short)value);
	public static short ToInt16(float value) => unchecked((short)value);
	public static short ToInt16(double value) => unchecked((short)value);
	public static short ToInt16(decimal value) => unchecked((short)value);

	public static ushort ToUInt16(byte value) => unchecked((ushort)value);
	public static ushort ToUInt16(sbyte value) => unchecked((ushort)value);
	public static ushort ToUInt16(short value) => unchecked((ushort)value);
	public static ushort ToUInt16(ushort value) => unchecked((ushort)value);
	public static ushort ToUInt16(int value) => unchecked((ushort)value);
	public static ushort ToUInt16(uint value) => unchecked((ushort)value);
	public static ushort ToUInt16(long value) => unchecked((ushort)value);
	public static ushort ToUInt16(ulong value) => unchecked((ushort)value);
	public static ushort ToUInt16(float value) => unchecked((ushort)value);
	public static ushort ToUInt16(double value) => unchecked((ushort)value);
	public static ushort ToUInt16(decimal value) => unchecked((ushort)value);

	public static int ToInt32(byte value) => unchecked((int)value);
	public static int ToInt32(sbyte value) => unchecked((int)value);
	public static int ToInt32(short value) => unchecked((int)value);
	public static int ToInt32(ushort value) => unchecked((int)value);
	public static int ToInt32(int value) => unchecked((int)value);
	public static int ToInt32(uint value) => unchecked((int)value);
	public static int ToInt32(long value) => unchecked((int)value);
	public static int ToInt32(ulong value) => unchecked((int)value);
	public static int ToInt32(float value) => unchecked((int)value);
	public static int ToInt32(double value) => unchecked((int)value);
	public static int ToInt32(decimal value) => unchecked((int)value);

	public static uint ToUInt32(byte value) => unchecked((uint)value);
	public static uint ToUInt32(sbyte value) => unchecked((uint)value);
	public static uint ToUInt32(short value) => unchecked((uint)value);
	public static uint ToUInt32(ushort value) => unchecked((uint)value);
	public static uint ToUInt32(int value) => unchecked((uint)value);
	public static uint ToUInt32(uint value) => unchecked((uint)value);
	public static uint ToUInt32(long value) => unchecked((uint)value);
	public static uint ToUInt32(ulong value) => unchecked((uint)value);
	public static uint ToUInt32(float value) => unchecked((uint)value);
	public static uint ToUInt32(double value) => unchecked((uint)value);
	public static uint ToUInt32(decimal value) => unchecked((uint)value);

	public static long ToInt64(byte value) => unchecked((long)value);
	public static long ToInt64(sbyte value) => unchecked((long)value);
	public static long ToInt64(short value) => unchecked((long)value);
	public static long ToInt64(ushort value) => unchecked((long)value);
	public static long ToInt64(int value) => unchecked((long)value);
	public static long ToInt64(uint value) => unchecked((long)value);
	public static long ToInt64(long value) => unchecked((long)value);
	public static long ToInt64(ulong value) => unchecked((long)value);
	public static long ToInt64(float value) => unchecked((long)value);
	public static long ToInt64(double value) => unchecked((long)value);
	public static long ToInt64(decimal value) => unchecked((long)value);

	public static ulong ToUInt64(byte value) => unchecked((ulong)value);
	public static ulong ToUInt64(sbyte value) => unchecked((ulong)value);
	public static ulong ToUInt64(short value) => unchecked((ulong)value);
	public static ulong ToUInt64(ushort value) => unchecked((ulong)value);
	public static ulong ToUInt64(int value) => unchecked((ulong)value);
	public static ulong ToUInt64(uint value) => unchecked((ulong)value);
	public static ulong ToUInt64(long value) => unchecked((ulong)value);
	public static ulong ToUInt64(ulong value) => unchecked((ulong)value);
	public static ulong ToUInt64(float value) => unchecked((ulong)value);
	public static ulong ToUInt64(double value) => unchecked((ulong)value);
	public static ulong ToUInt64(decimal value) => unchecked((ulong)value);

	public static float ToSingle(byte value) => unchecked((float)value);
	public static float ToSingle(sbyte value) => unchecked((float)value);
	public static float ToSingle(short value) => unchecked((float)value);
	public static float ToSingle(ushort value) => unchecked((float)value);
	public static float ToSingle(int value) => unchecked((float)value);
	public static float ToSingle(uint value) => unchecked((float)value);
	public static float ToSingle(long value) => unchecked((float)value);
	public static float ToSingle(ulong value) => unchecked((float)value);
	public static float ToSingle(float value) => unchecked((float)value);
	public static float ToSingle(double value) => unchecked((float)value);
	public static float ToSingle(decimal value) => unchecked((float)value);

	public static double ToDouble(byte value) => unchecked((double)value);
	public static double ToDouble(sbyte value) => unchecked((double)value);
	public static double ToDouble(short value) => unchecked((double)value);
	public static double ToDouble(ushort value) => unchecked((double)value);
	public static double ToDouble(int value) => unchecked((double)value);
	public static double ToDouble(uint value) => unchecked((double)value);
	public static double ToDouble(long value) => unchecked((double)value);
	public static double ToDouble(ulong value) => unchecked((double)value);
	public static double ToDouble(float value) => unchecked((double)value);
	public static double ToDouble(double value) => unchecked((double)value);
	public static double ToDouble(decimal value) => unchecked((double)value);

	public static decimal ToDecimal(byte value) => unchecked((decimal)value);
	public static decimal ToDecimal(sbyte value) => unchecked((decimal)value);
	public static decimal ToDecimal(short value) => unchecked((decimal)value);
	public static decimal ToDecimal(ushort value) => unchecked((decimal)value);
	public static decimal ToDecimal(int value) => unchecked((decimal)value);
	public static decimal ToDecimal(uint value) => unchecked((decimal)value);
	public static decimal ToDecimal(long value) => unchecked((decimal)value);
	public static decimal ToDecimal(ulong value) => unchecked((decimal)value);
	public static decimal ToDecimal(float value) => unchecked((decimal)value);
	public static decimal ToDecimal(double value) => unchecked((decimal)value);
	public static decimal ToDecimal(decimal value) => unchecked((decimal)value);

	public static byte ToByte(Enum value)
	{
		try
		{
			return ((IConvertible)value).ToByte(null);
		}
		catch
		{
			return ToByteDynamic(Convert.ChangeType(value, Enum.GetUnderlyingType(value.GetType())));
		}
	}

	public static sbyte ToSByte(Enum value)
	{
		try
		{
			return ((IConvertible)value).ToSByte(null);
		}
		catch
		{
			return ToSByteDynamic(Convert.ChangeType(value, Enum.GetUnderlyingType(value.GetType())));
		}
	}

	public static short ToInt16(Enum value)
	{
		try
		{
			return ((IConvertible)value).ToInt16(null);
		}
		catch
		{
			return ToInt16Dynamic(Convert.ChangeType(value, Enum.GetUnderlyingType(value.GetType())));
		}
	}

	public static ushort ToUInt16(Enum value)
	{
		try
		{
			return ((IConvertible)value).ToUInt16(null);
		}
		catch
		{
			return ToUInt16Dynamic(Convert.ChangeType(value, Enum.GetUnderlyingType(value.GetType())));
		}
	}

	public static int ToInt32(Enum value)
	{
		try
		{
			return ((IConvertible)value).ToInt32(null);
		}
		catch
		{
			return ToInt32Dynamic(Convert.ChangeType(value, Enum.GetUnderlyingType(value.GetType())));
		}
	}

	public static uint ToUInt32(Enum value)
	{
		try
		{
			return ((IConvertible)value).ToUInt32(null);
		}
		catch
		{
			return ToUInt32Dynamic(Convert.ChangeType(value, Enum.GetUnderlyingType(value.GetType())));
		}
	}

	public static long ToInt64(Enum value)
	{
		try
		{
			return ((IConvertible)value).ToInt64(null);
		}
		catch
		{
			return ToInt64Dynamic(Convert.ChangeType(value, Enum.GetUnderlyingType(value.GetType())));
		}
	}

	public static ulong ToUInt64(Enum value)
	{
		try
		{
			return ((IConvertible)value).ToUInt64(null);
		}
		catch
		{
			return ToUInt64Dynamic(Convert.ChangeType(value, Enum.GetUnderlyingType(value.GetType())));
		}
	}

	public static float ToSingle(Enum value)
	{
		try
		{
			return ((IConvertible)value).ToSingle(null);
		}
		catch
		{
			return ToSingleDynamic(Convert.ChangeType(value, Enum.GetUnderlyingType(value.GetType())));
		}
	}

	public static double ToDouble(Enum value)
	{
		try
		{
			return ((IConvertible)value).ToDouble(null);
		}
		catch
		{
			return ToDoubleDynamic(Convert.ChangeType(value, Enum.GetUnderlyingType(value.GetType())));
		}
	}

	public static decimal ToDecimal(Enum value)
	{
		try
		{
			return ((IConvertible)value).ToDecimal(null);
		}
		catch
		{
			return ToDecimalDynamic(Convert.ChangeType(value, Enum.GetUnderlyingType(value.GetType())));
		}
	}

	public static byte ToByteDynamic(dynamic value) => value is Enum enumValue ? ToByte(enumValue) : ToByte(value);
	public static sbyte ToSByteDynamic(dynamic value) => value is Enum enumValue ? ToSByte(enumValue) : ToSByte(value);
	public static short ToInt16Dynamic(dynamic value) => value is Enum enumValue ? ToInt16(enumValue) : ToInt16(value);
	public static ushort ToUInt16Dynamic(dynamic value) => value is Enum enumValue ? ToUInt16(enumValue) : ToUInt16(value);
	public static short ToInt32Dynamic(dynamic value) => value is Enum enumValue ? ToInt32(enumValue) : ToInt32(value);
	public static uint ToUInt32Dynamic(dynamic value) => value is Enum enumValue ? ToUInt32(enumValue) : ToUInt32(value);
	public static long ToInt64Dynamic(dynamic value) => value is Enum enumValue ? ToInt64(enumValue) : ToInt64(value);
	public static ulong ToUInt64Dynamic(dynamic value) => value is Enum enumValue ? ToUInt64(enumValue) : ToUInt64(value);
	public static float ToSingleDynamic(dynamic value) => value is Enum enumValue ? ToSingle(enumValue) : ToSingle(value);
	public static double ToDoubleDynamic(dynamic value) => value is Enum enumValue ? ToDouble(enumValue) : ToDouble(value);
	public static decimal ToDecimalDynamic(dynamic value) => value is Enum enumValue ? ToDecimal(enumValue) : ToDecimal(value);

	public static object ToType(dynamic value, Type targetType)
	{
		try
		{
			if (targetType == typeof(byte))
				return ToByte(value);
			if (targetType == typeof(sbyte))
				return ToByte(value);
			if (targetType == typeof(short))
				return ToInt16(value);
			if (targetType == typeof(ushort))
				return ToUInt16(value);
			if (targetType == typeof(int))
				return ToInt32(value);
			if (targetType == typeof(uint))
				return ToUInt32(value);
			if (targetType == typeof(long))
				return ToInt64(value);
			if (targetType == typeof(ulong))
				return ToUInt64(value);
			if (targetType == typeof(float))
				return ToSingle(value);
			if (targetType == typeof(double))
				return ToDouble(value);
			if (targetType == typeof(decimal))
				return ToDecimal(value);
		}
		catch { }

		throw RuntimeException.IllegalFunctionCall();
	}
}
