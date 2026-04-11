using System.IO;

namespace QBX.ExecutionEngine.Execution;

public class ChainArguments(TextReader reader, string filePath)
{
	public TextReader Reader => reader;
	public string FilePath => filePath;
}
