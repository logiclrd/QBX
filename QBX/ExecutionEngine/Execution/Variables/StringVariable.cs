using System;

using QBX.ExecutionEngine.Compiled;

namespace QBX.ExecutionEngine.Execution.Variables;

public class StringVariable : Variable
{
	public readonly StringValue Value;

	public StringVariable(int fixedStringLength = 0)
		: base(DataType.String)
	{
		if (fixedStringLength == 0)
			Value = new StringValue();
		else
			Value = StringValue.CreateFixedLength(fixedStringLength);
	}

	public StringVariable(StringValue value)
		: base(DataType.String)
	{
		Value = new StringValue(value);
	}

	protected StringVariable(StringValue orphan, bool adopt)
		: base(DataType.String)
	{
		if (adopt)
			Value = orphan;
		else
			Value = new StringValue(orphan);
	}

	public static StringVariable Adopt(StringValue orphan) => new StringVariable(orphan, adopt: true);

	public void SetValue(StringValue value)
	{
		Value.Set(value);
	}

	public override object GetData() => Value;
	public override void SetData(object value) => SetValue(value as StringValue ?? throw CompilerException.TypeMismatch(context: null));

	public override int CoerceToInt() => throw CompilerException.TypeMismatch(context: null);
	public override string ToString() => Value.ToString();

	public override void Serialize(Span<byte> buffer)
	{
		if (!Value.IsFixedLength)
			throw new Exception("Serialize called on a variable-length StringVariable");

		var source = Value.AsSpan();

		if (source.Length > buffer.Length)
			source = source.Slice(0, buffer.Length);

		source.CopyTo(buffer);
	}

	public override void Deserialize(ReadOnlySpan<byte> buffer)
	{
		if (!Value.IsFixedLength)
			throw new Exception("Serialize called on a variable-length StringVariable");

		var valueSpan = Value.AsSpan();

		if (buffer.Length >= Value.Length)
			buffer.Slice(0, Value.Length).CopyTo(valueSpan);
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

public class Substring(StringVariable variable, int start, int length) : StringVariable(variable.Value, adopt: true)
{
	public override int CoerceToInt()
	{
		string substring = Value.ToString(start, length);

		if (int.TryParse(substring, out var intValue))
			return intValue;
		else
			return 0;
	}

	public override object GetData()
	{
		return Value.Substring(start, length);
	}

	public override void SetData(object value)
	{
		var newValue = (StringValue)value;

		var newValueSpan = newValue.AsSpan();
		var targetSpan = Value.AsSpan().Slice(start, length);

		if (newValueSpan.Length > targetSpan.Length)
			newValueSpan = newValueSpan.Slice(0, targetSpan.Length);

		newValueSpan.CopyTo(targetSpan);
	}

	public override void Serialize(Span<byte> buffer)
	{
		var source = Value.AsSpan().Slice(start, length);

		if (source.Length > buffer.Length)
			source = source.Slice(0, buffer.Length);

		source.CopyTo(buffer);
	}

	public override void Deserialize(ReadOnlySpan<byte> buffer)
	{
		var targetSpan = Value.AsSpan().Slice(start, length);

		if (buffer.Length >= length)
			buffer.Slice(0, length).CopyTo(targetSpan);
		else
		{
			buffer.CopyTo(targetSpan);
			targetSpan.Slice(buffer.Length).Clear();
		}
	}

	public override bool IsZero => false;
	public override bool IsPositive => false;
	public override bool IsNegative => false;
}
