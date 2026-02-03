using System;

using QBX.ExecutionEngine.Compiled;
using QBX.Numbers;
using QBX.Utility;

namespace QBX.ExecutionEngine.Execution.Variables;

public class LongVariable : Variable
{
	public int Value;

	public LongVariable()
		: base(DataType.Long)
	{
	}

	public LongVariable(int value)
		: this()
	{
		Value = value;
	}

	public override object GetData() => Value;
	public override void SetData(object value) => Value = NumberConverter.ToLong(value);

	public override int CoerceToInt(Evaluable? context) => NumberConverter.ToLong(Value, context?.Source?.Token);
	public override string ToString() => NumberFormatter.Format(Value);

	public override int Serialize(Span<byte> buffer)
		=> BitConverterEx.WriteBytesThatFit(buffer, Value);
	public override int Deserialize(ReadOnlySpan<byte> buffer)
	{
		Value = BitConverterEx.ReadAvailableBytesLong(buffer);
		return Math.Min(4, buffer.Length);
	}

	public override void Reset()
	{
		Value = 0;
	}

	public override bool IsZero => (Value == 0);
	public override bool IsPositive => (Value > 0);
	public override bool IsNegative => (Value < 0);
}
