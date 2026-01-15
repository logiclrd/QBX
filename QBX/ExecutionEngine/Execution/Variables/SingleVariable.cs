using System;

using QBX.ExecutionEngine.Compiled;
using QBX.Numbers;
using QBX.Utility;

namespace QBX.ExecutionEngine.Execution.Variables;

public class SingleVariable : Variable
{
	public float Value;

	public SingleVariable()
		: base(DataType.Single)
	{
	}

	public SingleVariable(float value)
		: this()
	{
		Value = value;
	}

	public override object GetData() => Value;
	public override void SetData(object value) => Value = NumberConverter.ToSingle(value);

	public override int CoerceToInt() => NumberConverter.ToLong(Value);
	public override string ToString() => NumberFormatter.Format(Value);

	public override void Serialize(Span<byte> buffer)
		=> BitConverterEx.WriteBytesThatFit(buffer, Value);
	public override void Deserialize(ReadOnlySpan<byte> buffer)
		=> Value = BitConverterEx.ReadAvailableBytesSingle(buffer);

	public override bool IsZero => (Value == 0);
	public override bool IsPositive => (Value > 0);
	public override bool IsNegative => (Value < 0);
}
