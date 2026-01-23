using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

using QBX.Firmware.Fonts;

namespace QBX.ExecutionEngine.Execution;

public class StringValue : IComparable<StringValue>, IEquatable<StringValue>
{
	static Encoding s_cp437 = new CP437Encoding(ControlCharacterInterpretation.Semantic);

	public static bool IsNullOrEmpty([NotNullWhen(false)] StringValue? test)
		=> (test == null) || (test.Length == 0);

	public StringValue()
	{
	}

	public StringValue(StringValue other)
	{
		Append(other.AsSpan());
	}

	public StringValue(Span<byte> data)
	{
		Append(data);
	}

	public StringValue(string str)
	{
		Append(s_cp437.GetBytes(str));
	}

	StringValue(int fixedStringLength)
	{
		_bytes.EnsureCapacity(fixedStringLength);
		_bytes.AddRange(Enumerable.Repeat<byte>(0, fixedStringLength));

		_isFixedLength = true;
	}

	public static StringValue CreateFixedLength(int length)
		=> new StringValue(length);

	List<byte> _bytes = new List<byte>();
	bool _isFixedLength;

	public Span<byte> AsSpan() => CollectionsMarshal.AsSpan(_bytes);

	public byte[] ToByteArray() => _bytes.ToArray();

	public int Length => _bytes.Count;

	public bool IsFixedLength => _isFixedLength;

	public byte this[int index]
	{
		get => _bytes[index];
		set => _bytes[index] = value;
	}

	public StringValue Set(string str)
		=> Set(s_cp437.GetBytes(str));


	public StringValue Set(StringValue data)
		=> Set(data.AsSpan());

	public StringValue Set(Span<byte> data)
	{
		if (_isFixedLength)
		{
			if (data.Length > _bytes.Count)
				data = data.Slice(0, _bytes.Count);

			var byteSpan = CollectionsMarshal.AsSpan(_bytes);

			data.CopyTo(byteSpan);

			if (data.Length < byteSpan.Length)
				byteSpan.Slice(data.Length).Fill((byte)' ');
		}
		else
		{
			_bytes.Clear();
			_bytes.AddRange(data);
		}

		return this;
	}

	public StringValue Append(StringValue data)
		=> Append(data.AsSpan());

	public StringValue Append(Span<byte> data)
	{
		if (!_isFixedLength)
			_bytes.AddRange(data);

		return this;
	}

	public StringValue ReplaceSubstring(int offset, Span<byte> data)
	{
		if (offset + data.Length > Length)
			data = data.Slice(0, Length - offset);

		data.CopyTo(AsSpan().Slice(offset));

		return this;
	}

	public StringValue LeftSubstring(int length)
		=> new StringValue().Append(AsSpan().Slice(0, length));
	public StringValue RightSubstring(int length)
		=> new StringValue().Append(AsSpan().Slice(Length - length));
	public StringValue Substring(int offset, int length)
		=> new StringValue().Append(AsSpan().Slice(offset, length));

	public override string ToString()
		=> s_cp437.GetString(AsSpan());

	public string ToString(int index, int length)
		=> s_cp437.GetString(AsSpan().Slice(index, length));

	public static int Compare(StringValue left, StringValue right)
		=> Compare(left.AsSpan(), right.AsSpan());

	public static int Compare(Span<byte> leftSpan, Span<byte> rightSpan)
	{
		int l = Math.Min(leftSpan.Length, rightSpan.Length);

		for (int i = 0; i < l; i++)
		{
			int comparison = leftSpan[i] - rightSpan[i];

			if (comparison != 0)
				return comparison;
		}

		return leftSpan.Length - rightSpan.Length;
	}

	public int CompareTo(StringValue other)
	{
		var thisSpan = AsSpan();
		var otherSpan = other.AsSpan();

		return Compare(thisSpan, otherSpan);
	}

	public bool Equals(StringValue? other)
	{
		if (other == null)
			return false;

		return CompareTo(other) == 0;
	}

	int IComparable<StringValue>.CompareTo(StringValue? other)
	{
		if (other == null)
			return 1;

		return CompareTo(other);
	}

	public int IndexOf(StringValue searchFor, int start = 0)
	{
		return AsSpan().Slice(start).IndexOf(searchFor.AsSpan()) + start;
	}

	public static StringValue operator +(StringValue left, StringValue right)
	{
		var ret = new StringValue();

		ret.Append(left);
		ret.Append(right);

		return ret;
	}
}
