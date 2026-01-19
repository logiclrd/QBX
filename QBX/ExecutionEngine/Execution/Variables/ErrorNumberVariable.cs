using QBX.LexicalAnalysis;
using QBX.Numbers;

namespace QBX.ExecutionEngine.Execution.Variables;

public class ErrorNumberVariable : IntegerVariable
{
	public override object GetData() => Value;

	public override void SetData(object value)
	{
		short newValue = NumberConverter.ToInteger(value);

		if ((newValue < 0) || (newValue > 255))
			throw RuntimeException.IllegalFunctionCall(default(Token));

		Value = newValue;
	}
}
