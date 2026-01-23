using System.Threading;
using System.Collections.Generic;

using QBX.CodeModel;
using QBX.Firmware;
using QBX.Hardware;
using QBX.Parser;
using QBX.ExecutionEngine;
using QBX.DevelopmentEnvironment.Dialogs;

namespace QBX.DevelopmentEnvironment;

public partial class Program : HostedProgram
{
	public Machine Machine;
	public TextLibrary TextLibrary;

	public Configuration Configuration = new Configuration();

	public List<CompilationUnit> LoadedFiles = new List<CompilationUnit>();
	public int MainModuleIndex;

	public List<Watch> Watches = new List<Watch>();
	public Viewport? HelpViewport = null; // new Viewport() { HelpPage = new HelpPage(), IsEditable = false };
	public Viewport PrimaryViewport;
	public Viewport? SplitViewport;
	public Viewport ImmediateViewport;
	public ReferenceBarAction[]? ReferenceBarActions;
	public int SelectedReferenceBarAction = -1;
	public string? ReferenceBarText;

	public Viewport? FocusedViewport;
	public bool EnableOvertype = false;

	public UIMode Mode;

	public Dialog? CurrentDialog;

	public BasicParser Parser;

	public PlayProcessor PlayProcessor;

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
	}

	public override bool EnableMainLoop => true;

	public void ShowDialog(Dialog dialog)
	{
		var previousMode = Mode;

		CurrentDialog = dialog;
		Mode = UIMode.Dialog;

		dialog.Close +=
			(_, _) =>
			{
				CurrentDialog = null;
				Mode = previousMode;
			};
	}

	public override void Run(CancellationToken cancellationToken)
	{
		while (Machine.KeepRunning)
		{
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
		while (Machine.Keyboard.GetNextEvent() != null)
			;

		while (true)
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
