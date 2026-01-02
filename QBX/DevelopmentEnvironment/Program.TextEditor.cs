using QBX.Hardware;

namespace QBX.DevelopmentEnvironment;

public partial class Program
{
	void ProcessTextEditorKey(KeyEvent input)
	{
		if ((input.ScanCode == ScanCode.Alt) && !input.IsRelease)
		{
			Mode = UIMode.MenuBar;
			AltReleaseAction = AltRelease.ActivateMenuBar;
			SelectedMenu = -1;

			return;
		}

		// TODO
	}
}
