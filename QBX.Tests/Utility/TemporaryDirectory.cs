namespace QBX.Tests.Utility;

public class TemporaryDirectory : IDisposable
{
	DirectoryInfo _directory;

	public string Path => _directory.FullName;

	public TemporaryDirectory()
	{
		_directory = Directory.CreateTempSubdirectory();
	}

	public void Dispose()
	{
		try
		{
			if (_directory.Exists)
				_directory.Delete(recursive: true);
		}
		catch { }
	}
}
