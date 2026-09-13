using Microsoft.Win32.SafeHandles;

namespace QBX.Utility.Interop;

public abstract class INodeProvider<TINode>
	where TINode : INode<TINode>
{
	public abstract bool TryGetINode(string path, out TINode inode);
	public abstract bool TryGetINode(SafeFileHandle fileHandle, out TINode inode);
}
