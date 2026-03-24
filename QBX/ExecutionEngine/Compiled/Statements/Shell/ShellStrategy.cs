using System;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;

using QBX.Platform.Windows;
using QBX.Terminal;

using ExecutionContext = QBX.ExecutionEngine.Execution.ExecutionContext;

namespace QBX.ExecutionEngine.Compiled.Statements.Shell;

public abstract class ShellStrategy
{
	public virtual ProcessCreationFlags AdditionalProcessCreationFlags => 0;

	public abstract void Execute(ExecutionContext context, SafePseudoConsoleHandle hPC, AnonymousPipeServerStream stdinPipe, AnonymousPipeServerStream stdoutPipe, PROCESS_INFORMATION processInformation);

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

	protected Task CreateOutputTask(
		Func<int> read,
		Func<int, byte> convertToByte,
		Action<byte, Action<byte>> processControlSequenceBytes,
		Lock ioSync,
		TerminalEmulator target)
	{
		return Task.Run(
			() =>
			{
				Action<byte> emit =
					b =>
					{
						lock (ioSync)
							target.Write(b);
					};

				while (true)
				{
					int b = read();

					if (b < 0)
						break;

					processControlSequenceBytes(convertToByte(b), emit);
				}
			});
	}
}
