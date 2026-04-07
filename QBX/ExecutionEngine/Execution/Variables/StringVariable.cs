using System;
using System.Runtime.CompilerServices;
using System.Text;

using QBX.ExecutionEngine.Compiled;
using QBX.Firmware.Fonts;
using QBX.Hardware;

namespace QBX.ExecutionEngine.Execution.Variables;

public class StringVariable : Variable
{
	public readonly StringValue RawValue;

	public virtual StringValue Value => RawValue;
	public virtual Span<byte> ValueSpan => Value.AsSpan();
	public string ValueString => s_cp437.GetString(ValueSpan);

	public virtual StringValue CloneValue() => new StringValue(ValueSpan);

	public int IsMappedFieldCount = 0;

	public bool IsMappedField => (IsMappedFieldCount > 0);

	static Encoding s_cp437 = new CP437Encoding(ControlCharacterInterpretation.Semantic);

	public StringVariable()
		: this(fixedStringLength: 0)
	{
	}

	public StringVariable(int fixedStringLength = 0)
		: base(fixedStringLength == 0 ? DataType.String : DataType.MakeFixedStringType(fixedStringLength))
	{
		if (fixedStringLength < 0)
			RawValue = null!; // naughty naughty
		else if (fixedStringLength == 0)
			RawValue = new StringValue();
		else
			RawValue = StringValue.CreateFixedLength(fixedStringLength);
	}

	public StringVariable(StringValue value)
		: base(DataType.String)
	{
		RawValue = new StringValue(value);
	}

	public StringVariable(StringValue orphan, bool adopt)
		: base(DataType.String)
	{
		if (adopt)
			RawValue = orphan;
		else
			RawValue = new StringValue(orphan);
	}

	public static StringVariable Adopt(StringValue orphan) => new StringVariable(orphan, adopt: true);

	public virtual void SetValue(StringValue value)
	{
		Value.Set(value);
	}

	public override object GetData() => Value;
	public override void SetData(object value) => SetValue(value as StringValue ?? throw RuntimeException.TypeMismatch());

	public override int CoerceToInt(Evaluable? context) => throw RuntimeException.TypeMismatch(context?.Source);
	public override string ToString() => ValueString;

	public override int Serialize(Span<byte> buffer)
	{
		if ((RawValue != null) && !RawValue.IsFixedLength)
			throw new Exception("Serialize called on a variable-length StringVariable");

		var source = ValueSpan;

		if (source.Length > buffer.Length)
			source = source.Slice(0, buffer.Length);

		source.CopyTo(buffer);

		return source.Length;
	}

	public override int Deserialize(ReadOnlySpan<byte> buffer)
	{
		if ((RawValue != null) && !RawValue.IsFixedLength)
			throw new Exception("Serialize called on a variable-length StringVariable");

		var valueSpan = ValueSpan;

		if (buffer.Length >= valueSpan.Length)
			buffer.Slice(0, valueSpan.Length).CopyTo(valueSpan);
		else
		{
			buffer.CopyTo(valueSpan);
			valueSpan.Slice(buffer.Length).Clear();
		}

		return Math.Min(valueSpan.Length, buffer.Length);
	}

	public override void Reset()
	{
		RawValue.Reset();
	}

	public override bool IsZero => false;
	public override bool IsPositive => false;
	public override bool IsNegative => false;

	public virtual StringVariable Substring(int start, int length)
		=> new Substring(this, start, length);
}

public class Substring : StringVariable
{
	int _start;
	int _length;

	public int Start => _start;
	public int Length => _length;

	public Substring(StringVariable variable, int start, int length)
		: base(variable.RawValue, adopt: true)
	{
		if (variable is Substring otherSubstring) // a substring of a substring
			start += otherSubstring.Start;

		_start = start;
		_length = length;
	}

	public override int CoerceToInt(Evaluable? context)
	{
		string substring = ValueString;

		if (int.TryParse(substring, out var intValue))
			return intValue;
		else
			return 0;
	}

	public override StringValue Value => RawValue.Substring(_start, _length);
	public override Span<byte> ValueSpan => RawValue.AsSpan().Slice(_start, _length);

	public override StringValue CloneValue() => Value;

	public override void SetValue(StringValue newValue)
	{
		var newValueSpan = newValue.AsSpan();
		var targetSpan = ValueSpan.Slice(_start, _length);

		if (newValueSpan.Length > targetSpan.Length)
			newValueSpan = newValueSpan.Slice(0, targetSpan.Length);

		newValueSpan.CopyTo(targetSpan);
	}

	public override int Serialize(Span<byte> buffer)
	{
		var source = Value.AsSpan().Slice(_start, _length);

		if (source.Length > buffer.Length)
			source = source.Slice(0, buffer.Length);

		source.CopyTo(buffer);

		return source.Length;
	}

	public override int Deserialize(ReadOnlySpan<byte> buffer)
	{
		var targetSpan = Value.AsSpan().Slice(_start, _length);

		if (buffer.Length >= _length)
			buffer.Slice(0, _length).CopyTo(targetSpan);
		else
		{
			buffer.CopyTo(targetSpan);
			targetSpan.Slice(buffer.Length).Clear();
		}

		return Math.Min(_length, buffer.Length);
	}
}

public class PinnedStringVariable : StringVariable
{
	Machine _machine;
	int _length;

	public Machine Machine => _machine;

	public override StringValue Value => new StringValue(ValueSpan);
	public override Span<byte> ValueSpan => _machine.SystemMemory.AsSpan().Slice(PinnedMemoryAddress, _length);

	public PinnedStringVariable(Machine machine, int memoryAddress, int length)
		: base(fixedStringLength: -1)
	{
		_machine = machine;
		_length = length;

		PinnedMemoryAddress = memoryAddress;
	}

	public override void SetValue(StringValue value)
	{
		var newValueSpan = value.AsSpan();

		if (newValueSpan.Length > _length)
			newValueSpan.Slice(0, _length).CopyTo(ValueSpan);
		else
		{
			newValueSpan.CopyTo(ValueSpan);
			ValueSpan.Slice(newValueSpan.Length).Clear();
		}
	}

	public override StringVariable Substring(int start, int length)
	{
		var pinnedSubstring = new PinnedStringVariable(Machine, PinnedMemoryAddress + start, length);

		pinnedSubstring.PinnedMemoryOwner = PinnedMemoryOwner;

		return pinnedSubstring;
	}

	public override void Reset()
	{
		ValueSpan.Clear();
	}
}
