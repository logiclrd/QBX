namespace QBX.Tests.Utility;

public static class FileUtility
{
	public static byte[] ReadAllBytes(string path)
	{
		// Just like File.ReadAllBytes, except it doesn't require that nobody have
		// the file open for writing.
		using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
		{
			var buffer = new byte[stream.Length];

			stream.ReadExactly(buffer);

			return buffer;
		}
	}
}
