using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

using QBX.ExecutionEngine.Execution.Variables;
using QBX.Firmware;
using QBX.Firmware.Fonts;
using QBX.Hardware;

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
			string arguments = commandString == "" ? "" : (commandSwitch + " " + commandString);

			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
				RunChildProcessWindows(context, shell, arguments);
			else
			{
				var psi = new ProcessStartInfo(shell, arguments);

				psi.UseShellExecute = false;
				psi.RedirectStandardInput = true;
				psi.RedirectStandardOutput = true;
				psi.RedirectStandardError = true;
				psi.CreateNoWindow = true;

				var cancellationTokenSource = new CancellationTokenSource();

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
						var stdinTask = CreateInputTask(
							() => new TextWriterInjector(process.StandardInput),
							new KeyboardKeyEventSource(context.Machine.Keyboard),
							cancellationTokenSource.Token);

						var ioSync = new Lock();

						var stdoutTask = CreateOutputTask(
							() => process.StandardOutput.Read(),
							i => CP437Encoding.GetByteSemantic((char)i),
							(b, emit) => emit(b),
							ioSync,
							context.VisualLibrary);

						var stderrTask = CreateOutputTask(
							() => process.StandardError.Read(),
							i => CP437Encoding.GetByteSemantic((char)i),
							(b, emit) => emit(b),
							ioSync,
							context.VisualLibrary);

						process.WaitForExit();

						cancellationTokenSource.Cancel();

						Task.WaitAll(stdoutTask, stderrTask);
					}
					finally
					{
						context.VisualLibrary.ProcessControlCharacters = savedProcessControlCharacters;
						context.VisualLibrary.CRLFSemantics = savedCRLFSemantics;
					}
				}
			}
		}
		catch
		{
			context.VisualLibrary.WriteText("Bad command or file name");
			context.VisualLibrary.NewLine();
		}
	}

	class TextWriterInjector(TextWriter sink) : InputInjector
	{
		public void InjectByte(byte b)
			=> InjectChar(CP437Encoding.GetCharSemantic(b));

		public void InjectChar(char ch)
			=> sink.Write(ch);

		public override void Inject(KeyEvent keyEvent)
		{
			if (keyEvent.IsNormalText)
				InjectChar(keyEvent.TextCharacter);
			else
			{
				switch (keyEvent.ScanCode)
				{
					case ScanCode.Backspace: InjectByte(8); break;
					case ScanCode.Tab: InjectByte(9); break;
					case ScanCode.Escape: InjectByte(27); break;
					case ScanCode.Return: InjectByte(13); break;

					default:
					{
						if (keyEvent.Modifiers.CtrlKey && !keyEvent.Modifiers.ShiftKey && !keyEvent.Modifiers.AltKey)
						{
							switch (keyEvent.ScanCode)
							{
								case ScanCode.A: InjectByte(1); break;
								case ScanCode.B: InjectByte(2); break;
								case ScanCode.C: InjectByte(3); break;
								case ScanCode.D: InjectByte(4); break;
								case ScanCode.E: InjectByte(5); break;
								case ScanCode.F: InjectByte(6); break;
								case ScanCode.G: InjectByte(7); break;
								case ScanCode.H: InjectByte(8); break;
								case ScanCode.I: InjectByte(9); break;
								case ScanCode.J: InjectByte(10); break;
								case ScanCode.K: InjectByte(11); break;
								case ScanCode.L: InjectByte(12); break;
								case ScanCode.M: InjectByte(13); break;
								case ScanCode.N: InjectByte(14); break;
								case ScanCode.O: InjectByte(15); break;
								case ScanCode.P: InjectByte(16); break;
								case ScanCode.Q: InjectByte(17); break;
								case ScanCode.R: InjectByte(18); break;
								case ScanCode.S: InjectByte(19); break;
								case ScanCode.T: InjectByte(20); break;
								case ScanCode.U: InjectByte(21); break;
								case ScanCode.V: InjectByte(22); break;
								case ScanCode.W: InjectByte(23); break;
								case ScanCode.X: InjectByte(24); break;
								case ScanCode.Y: InjectByte(25); break;
								case ScanCode.Z: InjectByte(26); break;
							}
						}

						break;
					}
				}
			}
		}
	}

	interface IKeyEventSource
	{
		KeyEvent? ReceiveNextEvent(CancellationToken cancellationToken);
	}

	class KeyboardKeyEventSource(Keyboard keyboard) : IKeyEventSource
	{
		public KeyEvent? ReceiveNextEvent(CancellationToken cancellationToken)
		{
			keyboard.WaitForInput(cancellationToken);

			return keyboard.GetNextEvent();
		}
	}

	private static Task CreateInputTask<TInjector>(IKeyEventSource keyboard, CancellationToken cancellationToken)
		where TInjector : InputInjector, new()
	{
		return CreateInputTask(() => new TInjector(), keyboard, cancellationToken);
	}

	private static Task CreateInputTask<TInjector>(Func<TInjector> injectorFactory, IKeyEventSource keyboard, CancellationToken cancellationToken)
		where TInjector : InputInjector
	{
		return Task.Run(
			() =>
			{
				PumpInput(injectorFactory, keyboard, cancellationToken);
			});
	}

	private static void PumpInput<TInjector>(Func<TInjector> injectorFactory, IKeyEventSource keyboard, CancellationToken cancellationToken)
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

	private Task CreateOutputTask(
		Func<int> read,
		Func<int, byte> convertToByte,
		Action<byte, Action<byte>> processControlSequenceBytes,
		Lock ioSync,
		VisualLibrary visualLibrary)
	{
		return Task.Run(
			() =>
			{
				Action<byte> emit =
					b =>
					{
						lock (ioSync)
							visualLibrary.WriteText(b);
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
