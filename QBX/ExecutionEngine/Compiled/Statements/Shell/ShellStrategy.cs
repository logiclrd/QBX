using System;
using System.Threading;
using System.Threading.Tasks;

using QBX.Terminal;

namespace QBX.ExecutionEngine.Compiled.Statements.Shell;

public abstract class ShellStrategy
{
	protected static Task CreateInputTask<TInjector>(IKeyEventSource keyboard, CancellationToken cancellationToken)
		where TInjector : InputInjector, new()
	{
		return CreateInputTask(() => new TInjector(), keyboard, cancellationToken);
	}

	protected static Task CreateInputTask<TInjector>(Func<TInjector> injectorFactory, IKeyEventSource keyboard, CancellationToken cancellationToken)
		where TInjector : InputInjector
	{
		return Task.Run(
			() =>
			{
				PumpInput(injectorFactory, keyboard, cancellationToken);
			});
	}

	protected static void PumpInput<TInjector>(Func<TInjector> injectorFactory, IKeyEventSource keyboard, CancellationToken cancellationToken)
		where TInjector : InputInjector
	{
		try
		{
			using (var injector = injectorFactory())
			{
				while (!cancellationToken.IsCancellationRequested)
				{
					var keyEvent = keyboard.ReceiveNextEvent(cancellationToken);

					if ((keyEvent != null) && !keyEvent.IsRelease)
						injector.Inject(keyEvent);
				}
			}
		}
		catch { }
	}

	static IDisposable? MaybeLock(Lock? maybeLock)
	{
		if (maybeLock == null)
			return null;

		maybeLock.Enter();

		return new LockScope(maybeLock);
	}

	class LockScope(Lock @lock) : IDisposable
	{
		bool _isDisposed;

		public void Dispose()
		{
			if (!_isDisposed)
			{
				_isDisposed = true;
				@lock.Exit();
			}
		}
	}

	protected Task CreateOutputTask(
		Func<int> read,
		Action<byte, Action<byte>> processControlSequenceBytes,
		Lock? ioSync,
		TerminalEmulator target)
	{
		return Task.Run(
			() =>
			{
				Action<byte> emit =
					b =>
					{
						using (MaybeLock(ioSync))
							target.Write(b);
					};

				while (true)
				{
					int b = read();

					if (b < 0)
						break;

					processControlSequenceBytes(unchecked((byte)b), emit);
				}
			});
	}
}
