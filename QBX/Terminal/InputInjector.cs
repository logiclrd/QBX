using System;

using QBX.Hardware;
namespace QBX.Terminal;

public abstract class InputInjector : IDisposable
{
	public abstract void Inject(KeyEvent evt);

	public virtual void Dispose()
	{
	}
}

