using QBX.ExecutionEngine.Execution;

namespace QBX.ExecutionEngine.Compiled.Functions;

public class LTrimFunction : TrimFunction
{
	protected override StringValue PerformTrim(StringValue stringValue)
	{
		int index = 0;

		while ((index < stringValue.Length) && IsSpace(stringValue[index]))
			index++;

		return stringValue.Substring(index, stringValue.Length - index);
	}
}
