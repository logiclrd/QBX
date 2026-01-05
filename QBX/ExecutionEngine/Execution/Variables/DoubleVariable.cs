using QBX.ExecutionEngine.Compiled;
using QBX.Numbers;

namespace QBX.ExecutionEngine.Execution.Variables;

public class DoubleVariable : Variable
{
	public double Value;

	public DoubleVariable()
		: base(DataType.Double)
	{
	}

	public DoubleVariable(double value)
		: this()
	{
		Value = value;
	}

	public override object GetData() => Value;
	public override void SetData(object value) => Value = NumberConverter.ToDouble(value);

	public override int CoerceToInt() => NumberConverter.ToLong(Value);
	public override string ToString() => NumberFormatter.Format(Value);

	public override bool IsZero => (Value == 0);
	public override bool IsPositive => (Value > 0);
	public override bool IsNegative => (Value < 0);
}
