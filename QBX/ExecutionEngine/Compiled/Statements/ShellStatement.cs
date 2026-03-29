using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

using QBX.ExecutionEngine.Compiled.Statements.Shell;
using QBX.ExecutionEngine.Execution.Variables;
using QBX.Firmware;
using QBX.Terminal;

using ExecutionContext = QBX.ExecutionEngine.Execution.ExecutionContext;
using StackFrame = QBX.ExecutionEngine.Execution.StackFrame;

namespace QBX.ExecutionEngine.Compiled.Statements;

public partial class ShellStatement(CodeModel.Statements.Statement source) : Executable(source)
{
	public Evaluable? CommandStringExpression;

	public override void Execute(ExecutionContext context, StackFrame stackFrame)
	{
		string commandString = "";

		if (CommandStringExpression != null)
		{
			var commandStringResult = (StringVariable)CommandStringExpression.Evaluate(context, stackFrame);

			commandString = commandStringResult.ValueString;
		}

		string shellEnvironmentVariable;
		string fallbackShell;
		string commandSwitch;

		if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
		{
			shellEnvironmentVariable = "COMSPEC";
			fallbackShell = "CMD.EXE";
			commandSwitch = "/C";
		}
		else
		{
			shellEnvironmentVariable = "SHELL";
			fallbackShell = "/bin/sh";
			commandSwitch = "-c";
		}

		var shell = Environment.GetEnvironmentVariable(shellEnvironmentVariable) ?? fallbackShell;

		try
		{
			string[] arguments = commandString == "" ? [] : [commandSwitch, commandString];

			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
				RunChildProcessWindows(context, shell, arguments);
			else
			{
				try
				{
					RunChildProcessUnix(context, shell, arguments);
				}
				catch (PlatformNotSupportedException)
				{
					RunChildProcessFallback(context, shell, arguments);
				}
			}
		}
		catch
		{
			context.VisualLibrary.WriteText("Bad command or file name");
			context.VisualLibrary.NewLine();
		}
	}

	class FallbackStrategy : ShellStrategy
	{
		public void Execute(ExecutionContext context, Process process)
		{
			var textLibrary = context.VisualLibrary as TextLibrary;

			var cancellationTokenSource = new CancellationTokenSource();

			using (textLibrary?.ShowCursorForScope())
			{
				var terminalInput = new TerminalInput(process.StandardInput.BaseStream, context.Machine);

				var stdinTask = CreateInputTask(
					() => terminalInput,
					new KeyboardKeyEventSource(context.Machine.Keyboard),
					cancellationTokenSource.Token);

				var terminal = new TerminalEmulator(context.VisualLibrary);

				var controlSequenceProcessor = new TerminalControlSequenceProcessor(terminal, terminalInput);

				var ioSync = new Lock();

				var stdoutTask = CreateOutputTask(
					() => process.StandardOutput.BaseStream.ReadByte(),
					controlSequenceProcessor.ProcessByte,
					ioSync,
					terminal);

				var stderrTask = CreateOutputTask(
					() => process.StandardError.BaseStream.ReadByte(),
					controlSequenceProcessor.ProcessByte,
					ioSync,
					terminal);

				process.WaitForExit();

				cancellationTokenSource.Cancel();

				Task.WaitAll(stdoutTask, stderrTask);
			}
		}
	}

	void RunChildProcessFallback(ExecutionContext context, string shell, string[] arguments)
	{
		var psi = new ProcessStartInfo(shell, arguments);

		psi.UseShellExecute = false;
		psi.RedirectStandardInput = true;
		psi.RedirectStandardOutput = true;
		psi.RedirectStandardError = true;
		psi.CreateNoWindow = true;

		using (var process = Process.Start(psi))
		{
			if (process == null)
				throw new Exception();

			bool savedProcessControlCharacters = context.VisualLibrary.ProcessControlCharacters;
			var savedCRLFSemantics = context.VisualLibrary.CRLFSemantics;

			context.VisualLibrary.ProcessControlCharacters = true;
			context.VisualLibrary.CRLFSemantics = CRLFSemantics.Terminal;

			try
			{
				var strategy = new FallbackStrategy();

				strategy.Execute(context, process);
			}
			finally
			{
				context.VisualLibrary.ProcessControlCharacters = savedProcessControlCharacters;
				context.VisualLibrary.CRLFSemantics = savedCRLFSemantics;
			}
		}
	}
}

