using QBX.ExecutionEngine.Compiled;
using QBX.Numbers;

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

	public override int CoerceToInt() => NumberConverter.ToLong(Value);
	public override string ToString() => NumberFormatter.Format(Value);

	public override bool IsZero => (Value == 0);
	public override bool IsPositive => (Value > 0);
	public override bool IsNegative => (Value < 0);
}
