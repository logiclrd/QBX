namespace QBX.OperatingSystem.FileStructures;

public static class FileModeExtensions
{
	public static FileMode ToDOSFileMode(this System.IO.FileMode fileMode)
		=> (FileMode)fileMode;

	public static System.IO.FileMode ToSystemFileMode(this FileMode fileMode)
		=> (System.IO.FileMode)fileMode;
}
