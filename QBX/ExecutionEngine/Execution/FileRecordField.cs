using QBX.ExecutionEngine.Execution.Variables;

namespace QBX.ExecutionEngine.Execution;

public class FileRecordField(int width, StringVariable variable)
{
	public int Width = width;
	public StringVariable Variable = variable;
}
