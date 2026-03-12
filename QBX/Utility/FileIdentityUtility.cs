using System.IO;
using System.Runtime.InteropServices;

using QBX.Utility.Interop;

namespace QBX.Utility;

public static class FileIdentityUtility
{
	public static bool IsSameFile(string path1, string path2)
	{
		if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			return IsSameFile(path1, path2, new FileIndexProvider());
		else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
			return IsSameFile(path1, path2, new LinuxINodeProvider());
		else if (RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD))
			return IsSameFile(path1, path2, new FreeBSDINodeProvider());
		else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
			return IsSameFile(path1, path2, new OSXINodeProvider());
		else
			return Path.GetFullPath(path1) == Path.GetFullPath(path2);
	}

	static bool IsSameFile<TINode>(string path1, string path2, INodeProvider<TINode> inodeProvider)
		where TINode : INode<TINode>
	{
		return
			inodeProvider.TryGetINode(path1, out var inode1) &&
			inodeProvider.TryGetINode(path2, out var inode2) &&
			inode1.IsSameVolumeAndFileAs(inode2);
	}
}
