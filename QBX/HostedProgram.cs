using System;
using System.Threading;

namespace QBX;

public abstract class HostedProgram
{
	public abstract bool EnableMainLoop { get; }
	public abstract void Run(CancellationToken cancellationToken);
	public event Action<Icon>? WindowIconChanged;

	protected void OnWindowIconChanged(Icon icon)
		=> WindowIconChanged?.Invoke(icon);
}
