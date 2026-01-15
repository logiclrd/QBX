using QBX.CodeModel.Statements;
using System;
using System.Globalization;
using System.Text;

namespace QBX.Utility;

public static class StringBuilderExtensions
{
	static ComparisonInfo s_comparisonInfoInvariant = new CultureComparisonInfo(CultureInfo.InvariantCulture, ignoreCase: false);
	static ComparisonInfo s_comparisonInfoInvariantIgnoreCase = new CultureComparisonInfo(CultureInfo.InvariantCulture, ignoreCase: true);

	static ComparisonInfo s_comparisonInfoOrdinal = new OrdinalComparisonInfo();
	static ComparisonInfo s_comparisonInfoOrdinalIgnoreCase = new OrdinalIgnoreCaseComparisonInfo();

	public static bool Equals(this StringBuilder self, ReadOnlySpan<char> other, StringComparison comparison)
	{
		var comparisonInfo =
			comparison switch
			{
				StringComparison.CurrentCulture => new CultureComparisonInfo(CultureInfo.CurrentCulture, ignoreCase: false),
				StringComparison.CurrentCultureIgnoreCase => new CultureComparisonInfo(CultureInfo.CurrentCulture, ignoreCase: true),
				StringComparison.InvariantCulture => s_comparisonInfoInvariant,
				StringComparison.InvariantCultureIgnoreCase => s_comparisonInfoInvariantIgnoreCase,
				StringComparison.Ordinal => s_comparisonInfoOrdinal,
				StringComparison.OrdinalIgnoreCase => s_comparisonInfoOrdinalIgnoreCase,

				_ => throw new ArgumentException(nameof(comparison))
			};

		foreach (var chunk in self.GetChunks())
		{
			int length = Math.Min(chunk.Length, other.Length);

			bool equals = comparisonInfo.Equals(
				chunk.Span.Slice(0, length),
				other.Slice(0, length));

			if (!equals)
				return false;

			other = other.Slice(length);

			if (other.IsEmpty)
				return true;
		}

		return false;
	}

	abstract class ComparisonInfo
	{
		public abstract bool Equals(ReadOnlySpan<char> a, ReadOnlySpan<char> b);
	}

	class CultureComparisonInfo(CultureInfo culture, bool ignoreCase) : ComparisonInfo
	{
		CompareInfo _compareInfo = culture.CompareInfo;
		CompareOptions _compareOptions = ignoreCase ? CompareOptions.IgnoreCase : default;

		public override bool Equals(ReadOnlySpan<char> a, ReadOnlySpan<char> b)
			=> _compareInfo.Compare(a, b, _compareOptions) == 0;
	}

	class OrdinalComparisonInfo : ComparisonInfo
	{
		public override bool Equals(ReadOnlySpan<char> a, ReadOnlySpan<char> b)
			=> a.SequenceEqual(b);
	}

	class OrdinalIgnoreCaseComparisonInfo : ComparisonInfo
	{
		public override bool Equals(ReadOnlySpan<char> a, ReadOnlySpan<char> b)
		{
			if (a.Length != b.Length)
				return false;

			for (int i = 0; i < a.Length; i++)
			{
				char aCh = a[i];
				char bCh = b[i];

				if ((aCh != bCh)
				 && (((aCh | 0x20) != (bCh | 0x20)) || !char.IsAsciiLetter(aCh)))
					return false;
			}

			return true;
		}
	}
}
