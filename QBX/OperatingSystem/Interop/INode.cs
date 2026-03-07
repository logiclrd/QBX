namespace QBX.OperatingSystem.Interop;

public abstract class INode<TINode>
	where TINode : INode<TINode>
{
	public abstract bool IsSameVolumeAndFileAs(TINode other);
}
