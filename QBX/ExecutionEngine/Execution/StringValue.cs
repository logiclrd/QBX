using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

using QBX.Firmware.Fonts;

namespace QBX.ExecutionEngine.Execution;

public class StringValue : IComparable<StringValue>, IEquatable<StringValue>
{
	static Encoding s_cp437 = new CP437Encoding();

	public StringValue()
	{
	}

	public StringValue(StringValue other)
	{
		Append(other.AsSpan());
	}

	public StringValue(string str)
	{
		Append(s_cp437.GetBytes(str));
	}

	List<byte> _bytes = new List<byte>();

	public Span<byte> AsSpan() => CollectionsMarshal.AsSpan(_bytes);

	public int Length => _bytes.Count;

	public byte this[int index]
	{
		get => _bytes[index];
		set => _bytes[index] = value;
	}

	public StringValue Set(StringValue data)
		=> Set(data.AsSpan());

	public StringValue Set(Span<byte> data)
	{
		_bytes.Clear();
		_bytes.AddRange(data);

		return this;
	}

	public StringValue Append(StringValue data)
		=> Append(data.AsSpan());

	public StringValue Append(Span<byte> data)
	{
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

	internal int CompareTo(StringValue other)
	{
		var thisSpan = AsSpan();
		var otherSpan = other.AsSpan();

		int l = Math.Min(thisSpan.Length, otherSpan.Length);

		for (int i = 0; i < l; i++)
		{
			int comparison = thisSpan[i] - otherSpan[i];

			if (comparison != 0)
				return comparison;
		}

		return thisSpan.Length - otherSpan.Length;
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

	public static StringValue operator +(StringValue left, StringValue right)
	{
		var ret = new StringValue();

		ret.Append(left);
		ret.Append(right);

		return ret;
	}
}
