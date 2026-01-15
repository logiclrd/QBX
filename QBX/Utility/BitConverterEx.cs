using QBX.Parser;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Text;

namespace QBX.Utility;

class BitConverterEx
{
	[ThreadStatic]
	static byte[]? s_buffer;

	[MemberNotNull(nameof(s_buffer))]
	static void EnsureBuffer(int length)
	{
		if (length < 8)
			length = 8;

		if ((s_buffer == null) || (s_buffer.Length < length))
			s_buffer = new byte[length * 2];
	}

	public static void WriteBytesThatFit(Span<byte> destination, short value)
	{
		if (destination.Length >= 2)
			BitConverter.TryWriteBytes(destination, value);
		else
		{
			EnsureBuffer(2);
			BitConverter.TryWriteBytes(s_buffer, value);
			s_buffer.AsSpan().Slice(0, destination.Length).CopyTo(destination);
		}
	}

	public static void WriteBytesThatFit(Span<byte> destination, int value)
	{
		if (destination.Length >= 4)
			BitConverter.TryWriteBytes(destination, value);
		else
		{
			EnsureBuffer(4);
			BitConverter.TryWriteBytes(s_buffer, value);
			s_buffer.AsSpan().Slice(0, destination.Length).CopyTo(destination);
		}
	}

	public static void WriteBytesThatFit(Span<byte> destination, float value)
	{
		if (destination.Length >= 4)
			BitConverter.TryWriteBytes(destination, value);
		else
		{
			EnsureBuffer(4);
			BitConverter.TryWriteBytes(s_buffer, value);
			s_buffer.AsSpan().Slice(0, destination.Length).CopyTo(destination);
		}
	}

	public static void WriteBytesThatFit(Span<byte> destination, double value)
	{
		if (destination.Length >= 8)
			BitConverter.TryWriteBytes(destination, value);
		else
		{
			EnsureBuffer(8);
			BitConverter.TryWriteBytes(s_buffer, value);
			s_buffer.AsSpan().Slice(0, destination.Length).CopyTo(destination);
		}
	}

	public static void WriteBytesThatFit(Span<byte> destination, long value)
	{
		if (destination.Length >= 8)
			BitConverter.TryWriteBytes(destination, value);
		else
		{
			EnsureBuffer(2);
			BitConverter.TryWriteBytes(s_buffer, value);
			s_buffer.AsSpan().Slice(0, destination.Length).CopyTo(destination);
		}
	}

	public static void WriteBytesThatFit(Span<byte> destination, ReadOnlySpan<byte> source)
	{
		if (source.Length > destination.Length)
			source = source.Slice(0, destination.Length);

		source.CopyTo(destination);
	}

	public static short ReadAvailableBytesInteger(ReadOnlySpan<byte> source)
	{
		if (source.Length >= 2)
			return BitConverter.ToInt16(source);

		EnsureBuffer(2);

		var buffer = s_buffer.AsSpan().Slice(0, 2);

		source.CopyTo(buffer);

		short value = BitConverter.ToInt16(buffer);

		buffer.Clear();

		return value;
	}

	public static int ReadAvailableBytesLong(ReadOnlySpan<byte> source)
	{
		if (source.Length >= 4)
			return BitConverter.ToInt32(source);

		EnsureBuffer(4);

		var buffer = s_buffer.AsSpan().Slice(0, 4);

		source.CopyTo(buffer);

		int value = BitConverter.ToInt32(buffer);

		buffer.Clear();

		return value;
	}

	public static float ReadAvailableBytesSingle(ReadOnlySpan<byte> source)
	{
		if (source.Length >= 4)
			return BitConverter.ToSingle(source);

		EnsureBuffer(4);

		var buffer = s_buffer.AsSpan().Slice(0, 4);

		source.CopyTo(buffer);

		float value = BitConverter.ToSingle(buffer);

		buffer.Clear();

		return value;
	}

	public static double ReadAvailableBytesDouble(ReadOnlySpan<byte> source)
	{
		if (source.Length >= 8)
			return BitConverter.ToDouble(source);

		EnsureBuffer(8);

		var buffer = s_buffer.AsSpan().Slice(0, 8);

		source.CopyTo(buffer);

		double value = BitConverter.ToDouble(buffer);

		buffer.Clear();

		return value;
	}

	public static long ReadAvailableBytesInt64(ReadOnlySpan<byte> source)
	{
		if (source.Length >= 8)
			return BitConverter.ToInt64(source);

		EnsureBuffer(8);

		var buffer = s_buffer.AsSpan().Slice(0, 8);

		source.CopyTo(buffer);

		long value = BitConverter.ToInt64(buffer);

		buffer.Clear();

		return value;
	}
}
