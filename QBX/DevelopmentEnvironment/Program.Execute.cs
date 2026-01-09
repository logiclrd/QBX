using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

using QBX.CodeModel.Statements;
using QBX.ExecutionEngine;
using QBX.ExecutionEngine.Execution;
using QBX.Firmware;

namespace QBX.DevelopmentEnvironment;

public partial class Program
{
	ExecutionContext? _executionContext;
	Dictionary<Statement, SourceLocation> _statementLocation = new Dictionary<Statement, SourceLocation>();

	[MemberNotNullWhen(true, nameof(_executionContext))]
	public bool IsExecuting => (_executionContext != null);

	public void Run()
	{
		Restart();
		Continue();
	}

	[MemberNotNull(nameof(_executionContext))]
	public void Restart()
	{
		_executionContext?.Controls.Terminate();

		_statementLocation.Clear();

		var compilation = new Compilation();

		var compiler = new Compiler();

		foreach (var file in LoadedFiles)
		{
			foreach (var element in file.Elements)
				for (int lineIndex = 0; lineIndex < element.Lines.Count; lineIndex++)
				{
					var line = element.Lines[lineIndex];

					foreach (var statement in line.Statements)
						_statementLocation[statement] = new SourceLocation(file, element, line, statement, lineIndex);
				}

			compiler.Compile(file, compilation);
		}

		compilation.SetDefaultEntrypoint();

		RestoreOutput();

		_executionContext = new ExecutionContext(
			Machine
			);

		_executionContext.Controls.Break();

		Task.Run(
			() =>
			{
				_executionContext.Run(compilation);
			});
	}

	void UnpauseExecution()
	{
		_executionContext!.Controls.WaitForInterruption();

		if (_executionContext.ExecutionState.IsTerminated)
			ExecutionEpilogue();
		else
		{
			SaveOutput();
			SetIDEVideoMode();
			ShowNextStatement(_executionContext.ExecutionState.Stack);
		}
	}

	public void Continue()
	{
		if (_executionContext == null)
			Restart();
		else
			RestoreOutput();

		_executionContext.Controls.ContinueExecution();

		UnpauseExecution();
	}

	public void Step()
	{
		if (_executionContext == null)
		{
			Restart();

			_executionContext.Controls.WaitForInterruption();

			ShowNextStatement(_executionContext.ExecutionState.Stack);
		}
		else
		{
			RestoreOutput();

			_executionContext.Controls.ExecuteOneStatement();

			UnpauseExecution();
		}
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
