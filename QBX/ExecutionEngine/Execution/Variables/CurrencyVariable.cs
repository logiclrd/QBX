using System;

using QBX.ExecutionEngine.Compiled;
using QBX.Numbers;
using QBX.Utility;

namespace QBX.ExecutionEngine.Execution.Variables;

public class CurrencyVariable : Variable
{
	public decimal Value;

	public CurrencyVariable()
		: base(DataType.Currency)
	{
	}

	public CurrencyVariable(decimal value)
		: this()
	{
		Value = value;
	}

	public override object GetData() => Value;
	public override void SetData(object value) => Value = NumberConverter.ToCurrency(value);

	public override int CoerceToInt(Evaluable? context) => NumberConverter.ToLong(Value, context?.Source?.Token);
	public override string ToString() => NumberFormatter.Format(Value);

	public override int Serialize(Span<byte> buffer)
	{
		BitConverterEx.WriteBytesThatFit(buffer, decimal.ToOACurrency(Value));
		return 8;
	}

	public override int Deserialize(ReadOnlySpan<byte> buffer)
	{
		Value = decimal.FromOACurrency(BitConverterEx.ReadAvailableBytesInt64(buffer));
		return 8;
	}

	public override void Reset()
	{
		Value = 0;
	}

	public override bool IsZero => (Value == 0);
	public override bool IsPositive => (Value > 0);
	public override bool IsNegative => (Value < 0);
}
