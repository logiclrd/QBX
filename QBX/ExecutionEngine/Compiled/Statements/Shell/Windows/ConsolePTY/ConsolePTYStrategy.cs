using System.IO.Pipes;
using System.Threading;

using QBX.Firmware;
using QBX.Platform.Windows;
using QBX.Terminal;

using ExecutionContext = QBX.ExecutionEngine.Execution.ExecutionContext;

namespace QBX.ExecutionEngine.Compiled.Statements.Shell.Windows.ConsolePTY;

public class ConsolePTYStrategy : ShellStrategy
{
	public override void Execute(ExecutionContext context, SafePseudoConsoleHandle hPC, AnonymousPipeServerStream stdinPipe, AnonymousPipeServerStream stdoutPipe, PROCESS_INFORMATION processInformation)
	{
		const byte ETX = 3;

		void SendCtrlC() => stdinPipe.WriteByte(ETX);

		using (context.Machine.DOS.TakeOverBreakEventForScope(SendCtrlC))
		{
			var cancellationTokenSource = new CancellationTokenSource();

			var cancellationToken = cancellationTokenSource.Token;

			var savedSemantics = context.VisualLibrary.CRLFSemantics;

			context.VisualLibrary.CRLFSemantics = Firmware.CRLFSemantics.Terminal;

			try
			{
				var textLibrary = context.VisualLibrary as TextLibrary;

				using (textLibrary?.ShowCursorForScope())
				{
					var terminalInput = new TerminalInput(stdinPipe, context.Machine);

					var terminal = new TerminalEmulator(context.VisualLibrary);

					var controlSequenceProcessor = new TerminalControlSequenceProcessor(terminal, terminalInput);

					var inputTask = CreateInputTask(
						() => terminalInput,
						new KeyboardKeyEventSource(context.Machine.Keyboard),
						cancellationToken);

					var outputTask = CreateOutputTask(
						stdoutPipe.ReadByte,
						i => (byte)i,
						controlSequenceProcessor.ProcessByte,
						new Lock(),
						terminal);

					new ProcessWaitHandle(processInformation.hProcess).WaitOne();

					hPC.Close();

					outputTask.Wait();

					cancellationTokenSource.Cancel();
				}
			}
			finally
			{
				context.VisualLibrary.CRLFSemantics = savedSemantics;
			}
		}
	}
}

