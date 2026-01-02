using QBX.Firmware;
using QBX.Hardware;

namespace QBX.DevelopmentEnvironment;

public partial class Program : HostedProgram
{
	public Machine Machine;
	public Video Video;
	public TextLibrary TextLibrary;

	public Configuration Configuration = new Configuration();

	public List<Watch> Watches = new List<Watch>();
	public Viewport? HelpViewport = null; // new Viewport() { HelpPage = new HelpPage(), IsEditable = false };
	public Viewport PrimaryViewport = new Viewport() { IsFocused = true };
	public Viewport? SplitViewport;
	public Viewport ImmediateViewport = new Viewport() { Heading = "Immediate", ShowMaximize = false, Height = 2 };
	public ReferenceBarAction[]? ReferenceBarActions;
	public int SelectedReferenceBarAction = -1;
	public string? ReferenceBarText;

	public Viewport? FocusedViewport;

	public UIMode Mode;

	public Dialog? CurrentDialog;

	public Program(Machine machine, Video video)
	{
		Machine = machine;
		Video = video;

		video.SetMode(3);

		if (machine.GraphicsArray.Sequencer.CharacterWidth == 9)
			video.SetCharacterWidth(8);

		TextLibrary = new TextLibrary(machine.GraphicsArray);
		TextLibrary.MovePhysicalCursor = false;

		FocusedViewport = PrimaryViewport;

		Mode = UIMode.TextEditor;
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
