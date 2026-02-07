using System;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using System.Linq;

using QBX.CodeModel;
using QBX.Firmware;
using QBX.Hardware;
using QBX.Parser;
using QBX.ExecutionEngine;
using QBX.DevelopmentEnvironment.Dialogs;
using QBX.QuickLibraries;

namespace QBX.DevelopmentEnvironment;

public partial class Program : HostedProgram, IOvertypeFlag
{
	public Machine Machine;
	public TextLibrary TextLibrary;

	public Configuration Configuration = new Configuration();

	public List<CompilationUnit> LoadedFiles = new List<CompilationUnit>();
	public int MainModuleIndex;

	public Viewport? HelpViewport = null; // new Viewport() { HelpPage = new HelpPage(), IsEditable = false };
	public Viewport PrimaryViewport;
	public Viewport? SplitViewport;
	public Viewport ImmediateViewport;
	public ReferenceBarAction[]? ReferenceBarActions;
	public int SelectedReferenceBarAction = -1;
	public string? ReferenceBarText;

	// TODO: disable Utility menu, Options menu and Help menu (and help subsystem)
	// => error message when accessing a removed feature: "Feature removed"
	public bool NoFrillsMode;

	public bool Aborted = false;

	public bool AutoRun;

	public Viewport FocusedViewport;
	public bool EnableOvertype = false;

	public UIMode Mode;

	public List<Dialog> Dialogs = new List<Dialog>();

	public BasicParser Parser;

	public PlayProcessor PlayProcessor;

	bool IOvertypeFlag.Value
	{
		get => EnableOvertype;
		set => EnableOvertype = value;
	}

	void IOvertypeFlag.Toggle() => EnableOvertype = !EnableOvertype;

	public Program(Machine machine)
		: base(machine)
	{
		Machine = machine;

		PlayProcessor = new PlayProcessor(machine);
		PlayProcessor.StartProcessingThread();

		InitializeMenuBar();

		SetIDEVideoMode();

		TextLibrary = new TextLibrary(machine);
		TextLibrary.MovePhysicalCursor = false;
		TextLibrary.ProcessControlCharacters = false;

		TextLibrary.Clear();
		TextLibrary.HideCursor();

		SaveOutput();

		Parser = new BasicParser();

		PrimaryViewport =
			new Viewport(Parser)
			{
				IsFocused = true
			};

		ImmediateViewport =
			new Viewport(Parser)
			{
				Heading = "Immediate",
				ShowMaximize = false,
				Height = 2
			};

		FocusedViewport = PrimaryViewport;

		Mode = UIMode.TextEditor;

		StartNewProgram();

		string commandLine = Environment.CommandLine;

		int space = commandLine.IndexOf(' ');

		if (space >= 0)
			ProcessCommandLine(commandLine.Substring(space + 1).TrimStart());

		AttachBreakHandler();
	}

	private void ProcessCommandLine(string commandLine)
	{
		string? remainingCommandLine = commandLine;

		string? PullArgument()
		{
			if (remainingCommandLine == null)
				return null;

			string argument;

			int separator = remainingCommandLine.IndexOf(' ');

			if (separator < 0)
			{
				argument = remainingCommandLine;
				remainingCommandLine = null;
			}
			else
			{
				argument = remainingCommandLine.Substring(0, separator);
				remainingCommandLine = remainingCommandLine.Substring(separator + 1).TrimStart();

				if (remainingCommandLine.Length == 0)
					remainingCommandLine = null;
			}

			return argument;
		}

		string? loadFilePath = null;
		bool badArguments = false;

		while (PullArgument() is string argument)
		{
			if (argument.Equals("/AH", StringComparison.OrdinalIgnoreCase))
			{
				// Allow dynamic arrays to exceed 64KB
				ExecutionEngine.Execution.Array.MaximumSize = int.MaxValue;
			}
			else if (argument.Equals("/B", StringComparison.OrdinalIgnoreCase))
			{
				// Force black & white mode
				Configuration.DisplayAttributes.LoadBlackAndWhiteConfiguration();
			}
			else if (argument.StartsWith("/C:", StringComparison.OrdinalIgnoreCase))
			{
				// COM buffer size
			}
			else if (argument.Equals("/Ea", StringComparison.OrdinalIgnoreCase))
			{
				// Arrays in expanded memory
			}
			else if (argument.StartsWith("/E:", StringComparison.OrdinalIgnoreCase))
			{
				// Expanded memory usage limit
			}
			else if (argument.Equals("/Es", StringComparison.OrdinalIgnoreCase))
			{
				// Share expanded memory
			}
			else if (argument.Equals("/G", StringComparison.OrdinalIgnoreCase))
			{
				// CGA direct updates
			}
			else if (argument.Equals("/H", StringComparison.OrdinalIgnoreCase))
			{
				// Use the highest text resolution available
				Machine.VideoFirmware.SetCharacterRows(50);
				TextLibrary.RefreshParameters();
			}
			else if (argument.StartsWith("/K:", StringComparison.OrdinalIgnoreCase))
			{
				// Key mapping file (*.KEY)
				// TODO
			}
			else if (argument.Equals("/L", StringComparison.OrdinalIgnoreCase))
			{
				// Load QuickLibrary
				string qlbName;

				if (argument.Length > 2)
					qlbName = argument.Substring(2);
				else if ((remainingCommandLine != null) && !remainingCommandLine.StartsWith('/'))
					qlbName = PullArgument() ?? "QBX";
				else
					qlbName = "QBX";

				LoadQLB(qlbName);
			}
			else if (argument.Equals("/MBF", StringComparison.OrdinalIgnoreCase))
			{
				// Use Microsoft Binary Format for floating point
				// => QBX doesn't try to emulate this 100%. IEEE representation
				//    is still used for primary storage. But, MKS$(), MKD$(), CVS() and CVD()
				//    are mapped to MKSMBF$(), MKDMBF$(), CVSMBF() and CVDMBF(),
				//    respectively, and MBF representation is used for marshalling to
				//    NativeProcedures.
				// TODO
			}
			else if (argument.Equals("/NOF", StringComparison.OrdinalIgnoreCase)
			      || argument.Equals("/NOFRILLS", StringComparison.OrdinalIgnoreCase))
			{
				// No frills mode
				NoFrillsMode = true;
			}
			else if (argument.Equals("/NOHI", StringComparison.OrdinalIgnoreCase))
			{
				// No high-intensity colours
				Configuration.DisplayAttributes.LoadNoHighIntensityConfiguration();
			}
			else if (argument.Equals("/RUN", StringComparison.OrdinalIgnoreCase))
			{
				// Automatically start loaded program
				loadFilePath = PullArgument();

				if (loadFilePath == null)
				{
					badArguments = true;
					break;
				}

				AutoRun = true;
			}
			else if (argument.Equals("/CMD"))
			{
				// COMMAND$ value
				if (remainingCommandLine == null)
				{
					badArguments = true;
					break;
				}

				ProgramCommandLine = remainingCommandLine.ToUpperInvariant();
			}
			else if (argument.StartsWith('/'))
			{
				// Unrecognized
				badArguments = true;
			}
			else
			{
				// Bare argument: path to file to load
				if (loadFilePath != null)
				{
					badArguments = true;
					break;
				}

				if (!argument.StartsWith('"'))
					loadFilePath = argument;
				else
				{
					// Extension: support quoted filenames with spaces
					int endQuote = argument.IndexOf('"', 1);

					if (endQuote >= 0)
					{
						if (endQuote + 1 < argument.Length)
						{
							badArguments = true;
							break;
						}

						loadFilePath = argument.Substring(1, argument.Length - 2);
					}
					else
					{
						if (remainingCommandLine == null)
						{
							badArguments = true;
							break;
						}

						endQuote = remainingCommandLine.IndexOf('"');

						if (endQuote < 0)
							loadFilePath = argument.Substring(1) + ' ' + remainingCommandLine;
						else
						{
							loadFilePath = argument.Substring(1) + ' ' + remainingCommandLine.Substring(0, endQuote);

							remainingCommandLine = remainingCommandLine.Substring(endQuote);

							if (remainingCommandLine[0] != ' ')
							{
								badArguments = true;
								break;
							}

							remainingCommandLine = remainingCommandLine.TrimStart();
						}
					}
				}
			}
		}

		if (loadFilePath != null)
		{
			try
			{
				loadFilePath = Path.GetFullPath(loadFilePath);
			}
			catch
			{
				badArguments = true;
			}

			if (File.Exists(loadFilePath))
				LoadFile(loadFilePath, replaceExistingProgram: true);
			else
				LoadedFiles[0].FilePath = loadFilePath;
		}

		if (badArguments)
		{
			TextLibrary.WriteText("Valid options:");
			TextLibrary.NewLine();
			TextLibrary.WriteText("    /AH /B /C:n {/Ea | /Es} /E:n /G /H /K:[file] /L [lib]");
			TextLibrary.NewLine();
			TextLibrary.WriteText("    /MBF /NOF[RILLS] /NOHI [/RUN] file /CMD string");
			TextLibrary.NewLine();

			Aborted = true;
		}
	}

	private void LoadQLB(string qlbName)
	{
		while (true)
		{
			int dotIndex = qlbName.IndexOf('.');

			if (dotIndex < 0)
				qlbName += ".QLB";
			else if (dotIndex < qlbName.Length - 4)
			{
				qlbName = qlbName.Substring(0, dotIndex + 4);

				dotIndex = qlbName.IndexOf('.', dotIndex + 1);

				if (dotIndex > 0)
					qlbName = qlbName.Substring(0, dotIndex);
			}

			if (QuickLibrary.TryGetQuickLibrary(qlbName, Machine, out var qlb))
			{
				QLBs.Add(qlb);
				return;
			}

			TextLibrary.WriteText("Cannot find file (" + qlbName + ").  Input path: ");

			qlbName = TextLibrary.ReadLine(Machine.Keyboard);
		}
	}

	public override bool EnableMainLoop => true;

	public TDialog ShowDialog<TDialog>(TDialog dialog)
		where TDialog : Dialog
	{
		var previousMode = Mode;

		dialog.Y = (TextLibrary.Height - dialog.Height) / 2;

		int dialogPoint = Dialogs.Count;

		Dialogs.Add(dialog);

		dialog.Closed +=
			(_, _) =>
			{
				Dialogs.RemoveRange(dialogPoint, Dialogs.Count - dialogPoint);
				Mode = previousMode;
			};

		return dialog;
	}

	void SetWindowIcon()
	{
		using (var stream = typeof(Program).Assembly.GetManifestResourceStream("QBX.DevelopmentEnvironment.WindowIcon.ppm"))
		{
			if (stream == null)
				return;

			var reader = new StreamReader(stream);

			string buffer = "";
			int bufferIndex = 0;

			int ReadValue()
			{
				while (bufferIndex >= buffer.Length)
				{
					string? line = reader.ReadLine();

					if (line == null)
						return -1;

					buffer = line;
					bufferIndex = 0;

					while ((bufferIndex < buffer.Length) && char.IsWhiteSpace(buffer, bufferIndex))
						bufferIndex++;

					if ((bufferIndex < buffer.Length) && (buffer[bufferIndex] == '#'))
						bufferIndex = buffer.Length;
				}

				int tokenEnd = bufferIndex + 1;

				while ((tokenEnd < buffer.Length) && !char.IsWhiteSpace(buffer, tokenEnd))
				{
					if (buffer[tokenEnd] == '#')
						break;

					tokenEnd++;
				}

				int value;

				int.TryParse(buffer.Substring(bufferIndex, tokenEnd - bufferIndex), out value);

				bufferIndex = tokenEnd;

				if ((bufferIndex < buffer.Length) && (buffer[bufferIndex] == '#'))
					bufferIndex = buffer.Length;

				return value;
			}

			try
			{
				string? signature = reader.ReadLine();

				if ((signature == null)
				 || (signature.Length != 2)
				 || (signature[0] != 'P')
				 || !char.IsAscii(signature[1]))
					return;

				int width = ReadValue();
				int height = ReadValue();

				int maxColourValue = ReadValue();

				var icon = new Icon(width, height);

				for (int y=0, o=0; y < height; y++)
					for (int x=0; x < width; x++, o++)
					{
						int r = ReadValue() * 255 / maxColourValue;
						int g = ReadValue() * 255 / maxColourValue;
						int b = ReadValue() * 255 / maxColourValue;

						icon.Pixels[o] = (0xFF << 24) | (r << 16) | (g << 8) | (b << 0);
					}

				// Restore transparency -- this particular implementation works because the top-left pixel is transparent.
				for (int i = icon.Pixels.Length - 1; i >= 0; i--)
					if (icon.Pixels[i] == icon.Pixels[0])
						icon.Pixels[i] = 0;

				OnWindowIconChanged(icon);
			}
			catch { }
		}
	}

	bool _closeRequested;

	public override void Run(CancellationToken cancellationToken)
	{
		if (Aborted)
			return;

		cancellationToken.Register(
			() =>
			{
				Terminate();
			});

		SetWindowIcon();

		while (Machine.KeepRunning && !Machine.DOS.IsTerminated)
		{
			if (AutoRun)
			{
				AutoRun = false;
				Run();
			}

			if (_closeRequested)
			{
				_closeRequested = false;
				ExitWithSavePrompt();
			}

			Render();

			if (Machine.Keyboard.WaitForInput(cancellationToken))
			{
				var input = Machine.Keyboard.GetNextEvent();

				if (input != null)
				{
					var currentDialog = Dialogs.LastOrDefault();

					if (currentDialog != null)
						currentDialog.ProcessKey(input, overtypeFlag: this);
					else
					{
						switch (Mode)
						{
							case UIMode.TextEditor: ProcessTextEditorKey(input); break;
							case UIMode.Menu: ProcessMenuKey(input); break;
							case UIMode.MenuBar: ProcessMenuBarKey(input); break;
						}
					}
				}
			}
		}
	}

	void WaitForKey()
	{
		if (!Machine.KeepRunning)
			return;

		while (Machine.Keyboard.GetNextEvent() != null)
			;

		while (Machine.KeepRunning)
		{
			Machine.Keyboard.WaitForInput();

			var keyEvent = Machine.Keyboard.GetNextEvent();

			if ((keyEvent != null)
			 && !keyEvent.IsRelease
			 && !keyEvent.IsEphemeral)
				break;
		}
	}

	// Comes from another thread
	public override void RequestClose()
	{
		_closeRequested = true;

		_executionContext?.Controls.Break();
		Machine.Keyboard.InterruptWait();
	}

	public void Exit()
	{
		Machine.KeepRunning = false;
	}

	public void ExitWithSavePrompt()
	{
		if (CommitViewportsOrPresentError())
			PromptToSaveChanges(continuation: Exit);
	}
}
