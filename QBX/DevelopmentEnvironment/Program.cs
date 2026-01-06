using System.Threading;
using System.Collections.Generic;

using QBX.CodeModel;
using QBX.Firmware;
using QBX.Hardware;
using QBX.LexicalAnalysis;
using QBX.Parser;

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

	public Program(Machine machine)
	{
		Machine = machine;

		InitializeMenuBar();

		machine.VideoFirmware.SetMode(3);

		if (machine.GraphicsArray.Sequencer.CharacterWidth == 9)
			machine.VideoFirmware.SetCharacterWidth(8);

		TextLibrary = new TextLibrary(machine.GraphicsArray);
		TextLibrary.MovePhysicalCursor = false;

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
}
