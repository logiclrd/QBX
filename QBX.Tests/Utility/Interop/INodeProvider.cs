namespace QBX.Tests.Utility.Interop;

public abstract class INodeProvider<TINode>
	where TINode : INode<TINode>
{
	public abstract bool TryGetINode(string path, out TINode inode);
}
