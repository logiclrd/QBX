namespace QBX;

public abstract class HostedProgram
{
	public abstract bool EnableMainLoop { get; }
	public abstract void Run(CancellationToken cancellationToken);
}
