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

	[ThreadStatic]
	static byte[]? s_conversionBuffer;

	[MemberNotNull(nameof(s_conversionBuffer))]
	static void EnsureConversionBuffer(int length)
	{
		if ((s_conversionBuffer == null) || (s_conversionBuffer.Length < length))
			s_conversionBuffer = new byte[length * 2];
	}

	public static bool IsNullOrEmpty([NotNullWhen(false)] StringValue? test)
		=> (test == null) || (test.Length == 0);

	public StringValue()
	{
	}

	public StringValue(StringValue other)
	{
		Append(other.AsSpan());
	}

	public StringValue(byte[] data)
	{
		Append(data);
	}

	public StringValue(ReadOnlySpan<byte> data)
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

	public int Capacity
	{
		get => _bytes.Capacity;
		set => _bytes.Capacity = value;
	}

	public void Reset()
	{
		if (_isFixedLength)
			CollectionsMarshal.AsSpan(_bytes).Clear();
		else
			_bytes.Clear();
	}

	public StringValue Set(string str)
		=> Set(str.AsSpan());

	public StringValue Set(StringValue data)
		=> Set(data.AsSpan());

	public StringValue Set(ReadOnlySpan<char> str)
	{
		EnsureConversionBuffer(str.Length);

		int convertedCharacters = s_cp437.GetBytes(
			str,
			s_conversionBuffer.AsSpan());

		return Set(s_conversionBuffer.AsSpan().Slice(0, convertedCharacters));
	}

	public StringValue Set(ReadOnlySpan<byte> data)
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

	public StringValue SetCharacterAt(int index, char ch)
	{
		byte @byte = CP437Encoding.GetByteSemantic(ch);

		return SetCharacterAt(index, @byte);
	}

	public StringValue SetCharacterAt(int index, byte ch)
	{
		if ((index < 0) || (index > Length))
			throw new ArgumentOutOfRangeException(nameof(index));

		if (index < _bytes.Count)
			_bytes[index] = ch;
		else if (!_isFixedLength)
			_bytes.Add(ch);

		return this;
	}

	public StringValue Append(char ch)
	{
		byte @byte = CP437Encoding.GetByteSemantic(ch);

		return Append(@byte);
	}

	public StringValue Append(byte ch)
	{
		if (!_isFixedLength)
			_bytes.Add(ch);

		return this;
	}

	public StringValue Append(string str)
	{
		EnsureConversionBuffer(str.Length);

		int convertedCharacters = s_cp437.GetBytes(
			str,
			0,
			str.Length,
			s_conversionBuffer,
			0);

		return Append(s_conversionBuffer.AsSpan().Slice(0, convertedCharacters));
	}

	public StringValue Append(StringValue data)
		=> Append(data.AsSpan());

	public StringValue Append(ReadOnlySpan<byte> data)
	{
		if (!_isFixedLength)
			_bytes.AddRange(data);

		return this;
	}

	public StringValue Insert(int index, char ch)
	{
		if (!_isFixedLength)
		{
			byte @byte = CP437Encoding.GetByteSemantic(ch);

			_bytes.Insert(index, @byte);
		}

		return this;
	}

	public StringValue Insert(int index, byte ch)
	{
		if (!_isFixedLength)
			_bytes.Insert(index, ch);

		return this;
	}

	public StringValue Insert(int index, StringValue data)
		=> Insert(index, data.AsSpan());

	public StringValue Insert(int index, ReadOnlySpan<byte> data)
	{
		if (!_isFixedLength)
			_bytes.InsertRange(index, data);

		return this;
	}

	public StringValue ReplaceSubstring(int offset, ReadOnlySpan<byte> data)
	{
		if (offset + data.Length > Length)
			data = data.Slice(0, Length - offset);

		data.CopyTo(AsSpan().Slice(offset));

		return this;
	}

	public StringValue Remove(int index, int length)
	{
		if (!_isFixedLength)
			_bytes.RemoveRange(index, length);
		else
		{
			var byteSpan = AsSpan();

			byteSpan.Slice(index + length).CopyTo(byteSpan.Slice(index));
			byteSpan.Slice(byteSpan.Length - length).Clear();
		}

		return this;
	}

	public StringValue Clear()
	{
		if (_isFixedLength)
			AsSpan().Clear();
		else
			_bytes.Clear();

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

	public string ToString(int index)
		=> s_cp437.GetString(AsSpan().Slice(index));

	public string ToString(int index, int length)
		=> s_cp437.GetString(AsSpan().Slice(index, length));

	public static int Compare(StringValue left, StringValue right)
		=> Compare(left.AsSpan(), right.AsSpan());

	public static int Compare(ReadOnlySpan<byte> leftSpan, ReadOnlySpan<byte> rightSpan)
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

	public bool StartsWith(byte searchFor) => AsSpan().StartsWith(searchFor);
	public bool StartsWith(ReadOnlySpan<byte> searchFor) => AsSpan().StartsWith(searchFor);

	public bool EndsWith(byte searchFor) => AsSpan().EndsWith(searchFor);
	public bool EndsWith(ReadOnlySpan<byte> searchFor) => AsSpan().EndsWith(searchFor);

	public int IndexOf(byte searchFor, int start = 0)
	{
		return AsSpan().Slice(start).IndexOf(searchFor) + start;
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
