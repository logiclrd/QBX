using System;
using System.Text;

using QBX.ExecutionEngine.Compiled;
using QBX.Firmware.Fonts;

namespace QBX.ExecutionEngine.Execution.Variables;

public class StringVariable : Variable
{
	public readonly StringValue RawValue;

	public virtual StringValue Value => RawValue;
	public virtual Span<byte> ValueSpan => Value.AsSpan();
	public string ValueString => s_cp437.GetString(ValueSpan);

	public virtual StringValue CloneValue() => new StringValue(RawValue);

	static Encoding s_cp437 = new CP437Encoding();

	public StringVariable(int fixedStringLength = 0)
		: base(DataType.String)
	{
		if (fixedStringLength == 0)
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
	public override void SetData(object value) => SetValue(value as StringValue ?? throw CompilerException.TypeMismatch(context: null));

	public override int CoerceToInt() => throw CompilerException.TypeMismatch(context: null);
	public override string ToString() => ValueString;

	public override void Serialize(Span<byte> buffer)
	{
		if (!RawValue.IsFixedLength)
			throw new Exception("Serialize called on a variable-length StringVariable");

		var source = RawValue.AsSpan();

		if (source.Length > buffer.Length)
			source = source.Slice(0, buffer.Length);

		source.CopyTo(buffer);
	}

	public override void Deserialize(ReadOnlySpan<byte> buffer)
	{
		if (!RawValue.IsFixedLength)
			throw new Exception("Serialize called on a variable-length StringVariable");

		var valueSpan = RawValue.AsSpan();

		if (buffer.Length >= RawValue.Length)
			buffer.Slice(0, RawValue.Length).CopyTo(valueSpan);
		else
		{
			buffer.CopyTo(valueSpan);
			valueSpan.Slice(buffer.Length).Clear();
		}
	}

	public override bool IsZero => false;
	public override bool IsPositive => false;
	public override bool IsNegative => false;
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

	public override int CoerceToInt()
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

	public override void Serialize(Span<byte> buffer)
	{
		var source = Value.AsSpan().Slice(_start, _length);

		if (source.Length > buffer.Length)
			source = source.Slice(0, buffer.Length);

		source.CopyTo(buffer);
	}

	public override void Deserialize(ReadOnlySpan<byte> buffer)
	{
		var targetSpan = Value.AsSpan().Slice(_start, _length);

		if (buffer.Length >= _length)
			buffer.Slice(0, _length).CopyTo(targetSpan);
		else
		{
			buffer.CopyTo(targetSpan);
			targetSpan.Slice(buffer.Length).Clear();
		}
	}
}
