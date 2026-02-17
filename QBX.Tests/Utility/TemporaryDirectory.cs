using System.Runtime.InteropServices;

using QBX.Tests.Utility.Interop;

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
			if (IsSubdirectoryOf(test: Environment.CurrentDirectory, possibleParent: _directory))
				Environment.CurrentDirectory = _directory.Parent?.FullName ?? "\\";

			if (_directory.Exists)
				_directory.Delete(recursive: true);
		}
		catch { }
	}

	static bool IsSubdirectoryOf(string test, DirectoryInfo possibleParent)
	{
		if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			return IsSubdirectoryOf(test, possibleParent, new FileIndexProvider());
		else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
			return IsSubdirectoryOf(test, possibleParent, new LinuxINodeProvider());
		else if (RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD))
			return IsSubdirectoryOf(test, possibleParent, new FreeBSDINodeProvider());
		else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
			return IsSubdirectoryOf(test, possibleParent, new OSXINodeProvider());
		else
			return possibleParent.FullName.StartsWith(System.IO.Path.GetFullPath(test));
	}

	static bool IsSubdirectoryOf<TINode>(string? test, DirectoryInfo possibleParent, INodeProvider<TINode> inodeProvider)
		where TINode : INode<TINode>
	{
		if (test == null)
			return false;

		if (!inodeProvider.TryGetINode(possibleParent.FullName, out var possibleParentINode))
			throw new IOException("Couldn't retrieve inode for " + possibleParent.FullName);

		bool haveINode = false;

		test = System.IO.Path.GetFullPath(test);

		while (test != null)
		{
			if (inodeProvider.TryGetINode(test, out var testINode))
			{
				if (testINode.IsSameVolumeAndFileAs(possibleParentINode))
					return true;

				haveINode = true;
			}
			else
			{
				if (haveINode)
					throw new IOException("Couldn't retrieve inode for " + test);
			}

			test = System.IO.Path.GetDirectoryName(test);
		}

		return false;
	}
}
