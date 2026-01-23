using QBX.ExecutionEngine.Execution;

namespace QBX.ExecutionEngine.Compiled.Functions;

public class RTrimFunction : TrimFunction
{
	protected override StringValue PerformTrim(StringValue stringValue)
	{
		int index = stringValue.Length - 1;

		while ((index >= 0) && IsSpace(stringValue[index]))
			index--;

		return stringValue.Substring(0, index + 1);
	}
}
