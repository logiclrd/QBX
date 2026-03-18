using System;

namespace QBX.Utility;

public static class StringExtensions
{
	public static ReadOnlySpan<char> SkipSpaces(this string? str)
	{
		if (str == null)
			return null;
		else
			return str.AsSpan().SkipSpaces();
	}

	public static ReadOnlySpan<char> SkipSpaces(this ReadOnlySpan<char> span)
	{
		while ((span.Length > 0) && char.IsWhiteSpace(span[0]))
			span = span.Slice(1);

		return span;
	}
}
