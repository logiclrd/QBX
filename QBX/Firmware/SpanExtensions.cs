using System;
using System.Runtime.CompilerServices;

namespace QBX.Firmware;

static class SpanExtensions
{
	public static void CopyEvenTo<T>(this Span<T> source, Span<T> dest)
	{
		if (!source.ContainsAddress(ref dest[0]))
		{
			for (int i = 0; i < source.Length; i += 2)
				dest[i] = source[i];
		}
		else
		{
			// Copy backwards, because the destination starts somewhere within
			// the source. If we copy forwards, we'll end up overwriting future
			// bytes before we've copied them.
			int lastByte = source.Length & ~1;

			for (int i = lastByte; i >= 0; i -= 2)
				dest[i] = source[i];
		}
	}

	public static void FillEven<T>(this Span<T> dest, T value)
	{
		for (int i = 0; i < dest.Length; i += 2)
			dest[i] = value;
	}

	public static bool ContainsAddress<T>(this Span<T> span, ref T item)
	{
		return
			Unsafe.IsAddressGreaterThanOrEqualTo(ref item, ref span[0]) &&
			Unsafe.IsAddressLessThanOrEqualTo(ref item, ref span[span.Length - 1]);
	}
}
