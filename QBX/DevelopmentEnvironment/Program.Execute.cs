using System.Diagnostics.CodeAnalysis;

using QBX.ExecutionEngine;
using QBX.ExecutionEngine.Execution;
using QBX.Firmware;
using QBX.Hardware;

namespace QBX.DevelopmentEnvironment;

public partial class Program
{
	ExecutionContext? _executionContext;

	public void Run()
	{
		Restart();
		Continue();
	}

	[MemberNotNull(nameof(_executionContext))]
	public void Restart()
	{
		var typeRepository = new ExecutionEngine.TypeRepository();

		var module = new Compiler().Compile(LoadedFiles[0], typeRepository);

		_executionContext = new ExecutionContext(
			Machine,
			module);

		Machine.VideoFirmware.SetMode(3);

		SaveOutput();
	}

	public void Continue()
	{
		if (_executionContext == null)
			Restart();

		RestoreOutput();

		_executionContext.Run(RunMode.Continuous);

		ExecutionEpilogue();
	}

	public void Step(bool stepInto)
	{
	}

	void ExecutionEpilogue()
	{
		if (_executionContext == null)
			return; // ??

		var outputLibrary = _executionContext.VisualLibrary;

		var (savedCursorX, savedCursorY) = (outputLibrary.CursorX, outputLibrary.CursorY);

		if ((outputLibrary is TextLibrary)
		 && (savedCursorY == outputLibrary.CharacterHeight - 1))
			outputLibrary.ScrollText();

		outputLibrary.MoveCursor(0, outputLibrary.CharacterHeight - 1);
		outputLibrary.WriteText("Press any key to continue");

		WaitForKey();

		outputLibrary.MoveCursor(0, outputLibrary.CharacterHeight - 1);
		outputLibrary.WriteText("                         ");
		outputLibrary.MoveCursor(savedCursorX, savedCursorY);

		SaveOutput();

		SetIDEVideoMode();

		_executionContext = null;
	}
}
