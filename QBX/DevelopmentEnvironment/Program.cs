using System;
using System.IO;
using System.Threading;
using System.Collections.Generic;

using QBX.CodeModel;
using QBX.Firmware;
using QBX.Hardware;
using QBX.Parser;
using QBX.ExecutionEngine;
using QBX.DevelopmentEnvironment.Dialogs;
using System.Linq;

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

	public Viewport FocusedViewport;
	public bool EnableOvertype = false;

	public UIMode Mode;

	public Dialog? CurrentDialog;

	public BasicParser Parser;

	public PlayProcessor PlayProcessor;

	bool IOvertypeFlag.Value
	{
		get => EnableOvertype;
		set => EnableOvertype = value;
	}

	void IOvertypeFlag.Toggle() => EnableOvertype = !EnableOvertype;

	public Program(Machine machine)
	{
		Machine = machine;

		PlayProcessor = new PlayProcessor(machine);
		PlayProcessor.StartProcessingThread();

		InitializeMenuBar();

		SetIDEVideoMode();

		TextLibrary = new TextLibrary(machine);
		TextLibrary.MovePhysicalCursor = false;

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
		{
			string initialFilePath = commandLine.Substring(space + 1).TrimStart();

			if (File.Exists(initialFilePath))
				LoadFile(initialFilePath, replaceExistingProgram: true);
		}
	}

	public override bool EnableMainLoop => true;

	public TDialog ShowDialog<TDialog>(TDialog dialog)
		where TDialog : Dialog
	{
		var previousMode = Mode;

		dialog.Y = (TextLibrary.Height - dialog.Height) / 2;

		CurrentDialog = dialog;
		Mode = UIMode.Dialog;

		dialog.Closed +=
			(_, _) =>
			{
				CurrentDialog = null;
				Mode = previousMode;
			};

		return dialog;
	}

	public override void Run(CancellationToken cancellationToken)
	{
		cancellationToken.Register(
			() =>
			{
				Terminate();
			});

		while (Machine.KeepRunning)
		{
			EvaluateWatches(_watches, out bool @break);

			Render();

			if (Machine.Keyboard.WaitForInput(cancellationToken))
			{
				var input = Machine.Keyboard.GetNextEvent();

				if (input != null)
				{
					switch (Mode)
					{
						case UIMode.TextEditor: ProcessTextEditorKey(input); break;
						case UIMode.Menu: ProcessMenuKey(input); break;
						case UIMode.MenuBar: ProcessMenuBarKey(input); break;
						case UIMode.Dialog: CurrentDialog?.ProcessKey(input); break;
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
}
