using QBX.ExecutionEngine.Compiled;

namespace QBX.ExecutionEngine.Execution.Variables;

public class StringVariable : Variable
{
	public string Value = "";

	public StringVariable()
		: base(DataType.String)
	{
	}

	public StringVariable(string value)
		: this()
	{
		Value = value;
	}

	public override object GetData() => Value;
	public override void SetData(object value) => Value = value as string ?? throw new RuntimeException("Type mismatch");

	public override int CoerceToInt() => throw new RuntimeException("Type mismatch");
	public override string ToString() => Value;

	public override bool IsZero => false;
	public override bool IsPositive => false;
	public override bool IsNegative => false;
}
