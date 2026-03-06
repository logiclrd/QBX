namespace QBX.ExecutionEngine.Execution;

public class OpenFile
{
	public int FileHandle;
	public OpenFileIOMode IOMode;
	public int BufferSize = 512;
	public int RecordLength = 128;
	public int LineWidth = int.MaxValue;

	public const int NoLineWidthLimit = 255;
}
