using System;
using System.Threading;

using QBX.Hardware;

namespace QBX;

public abstract class HostedProgram(Machine machine)
{
	public abstract bool EnableMainLoop { get; }
	public abstract void Run(CancellationToken cancellationToken);
	public virtual void RequestClose() { machine.KeepRunning = false; }

	public event Action<Icon>? WindowIconChanged;

	protected void OnWindowIconChanged(Icon icon)
		=> WindowIconChanged?.Invoke(icon);
}
