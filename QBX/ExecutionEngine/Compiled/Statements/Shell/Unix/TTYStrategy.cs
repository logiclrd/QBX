using System.Diagnostics;
using System.IO;
using System.Threading;

using QBX.Firmware;
using QBX.Terminal;

using ExecutionContext = QBX.ExecutionEngine.Execution.ExecutionContext;

namespace QBX.ExecutionEngine.Compiled.Statements.Shell.Unix;

public class TTYStrategy : ShellStrategy
{
	public void Execute(ExecutionContext context, Stream ptyPipe, int processID)
	{
		const byte ETX = 3;

		void SendCtrlC() => ptyPipe.WriteByte(ETX);

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
					var terminalInput = new TerminalInput(ptyPipe, context.Machine);

					var terminal = new TerminalEmulator(context.VisualLibrary);

					var controlSequenceProcessor = new TerminalControlSequenceProcessor(terminal, terminalInput);

					var inputTask = CreateInputTask(
						() => terminalInput,
						new KeyboardKeyEventSource(context.Machine.Keyboard),
						cancellationToken);

					var outputTask = CreateOutputTask(
						() => ptyPipe.ReadByte(),
						controlSequenceProcessor.ProcessByte,
						ioSync: null,
						terminal);

					Process.GetProcessById(processID).WaitForExit();

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
