using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using QBX.CodeModel;
using QBX.ExecutionEngine;
using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Events;
using QBX.Firmware;
using QBX.QuickLibraries;
using QBX.Utility;

namespace QBX.DevelopmentEnvironment;

public partial class Program
{
	System.Threading.Thread? _executionThread;
	ExecutionContext? _executionContext;
	Compiler? _compiler;
	Compilation? _compilation;

	public EventHub EventHub;

	public List<QuickLibrary> QLBs = new List<QuickLibrary>();

	public bool DetectDelayLoops = new SystemDetector().IsLaptop();

	public EventCheckGranularity EventCheckGranularity = EventCheckGranularity.EveryStatement;

	public string ProgramCommandLine = "";

	[MemberNotNullWhen(true, nameof(_executionContext))]
	public bool IsExecuting => (_executionContext != null);

	void AttachBreakHandler()
	{
		Machine.DOS.Break +=
			() =>
			{
				_executionContext?.Controls.Break();
			};
	}

	public void Run()
	{
		Restart();
		Continue();
	}

	public void Terminate()
	{
		try
		{
			if ((_executionThread != null)
			 && _executionThread.IsAlive)
				_executionThread.IsBackground = true;

			if (_executionContext != null)
			{
				_executionContext.Controls.Terminate();
				_executionContext.CloseAllFiles();
			}
		}
		catch { }
	}

	[MemberNotNullWhen(true, nameof(_executionContext))]
	public bool Restart()
	{
		Terminate();

		if (!EnsureAllCodeIsParsed())
			return false;

		_compiler = new Compiler();
		_compilation = new Compilation();

		_compiler.DetectDelayLoops = DetectDelayLoops;

		try
		{
			foreach (var nativeProcedure in QLBs.SelectMany(qlb => qlb.Exports))
				_compilation.RegisterNativeProcedure(nativeProcedure);

			foreach (var file in LoadedFiles)
				if (file.IncludeInBuild && (file is CompilationUnit unit))
					_compiler.Compile(unit, _compilation);

			if (!_compilation.ResolveUnresolvedCalls(out var errorModule))
			{
				throw CompilerException.SubprogramNotDefined(
					errorModule.UnresolvedReferences.GetFirstUnresolvedStatementSourceToken());
			}
		}
		catch (CompilerException error)
		{
			PresentError(error);
			return false;
		}

		_compilation.SetDefaultEntrypoint();

		AssociateWatches(_compilation);

		RestoreOutput();

		_executionContext = new ExecutionContext(Machine, PlayProcessor, EventHub);
		_executionContext.EventCheckGranularity = EventCheckGranularity;
		_executionContext.CommandLine.Set(ProgramCommandLine);
		_executionContext.Controls.Break();

		foreach (var qlb in QLBs)
			qlb.ExecutionContext = _executionContext;

		_executionThread = new System.Threading.Thread(
			() =>
			{
				try
				{
					_executionContext.Run(_compilation);
				}
				catch (Exception e)
				{
					PresentError("Internal error: " + e.ToString());
				}
			});

		_executionThread.IsBackground = false;
		_executionThread.Name = "Program Execution Thread";

		if (_watches.Any(watch => watch.IsWatchPoint))
			EnableWatchpointChecks();

		_executionThread.Start();

		if (_compilation.IsEmpty)
			_executionContext.Controls.Terminate();
		else
		{
			if (_executionContext.WaitForRootFrame())
			{
				var rootFrame = _executionContext.RootFrame;

				if (rootFrame != null)
					EvaluateWatches(rootFrame);
			}
		}

		return true;
	}

	void UnpauseExecution()
	{
		using (Machine.DOS.EnableBreak())
			_executionContext!.Controls.WaitForInterruption();

		if (_executionContext.ExecutionState.IsTerminated)
			ExecutionEpilogue();
		else
		{
			SaveOutput();
			SetIDEVideoMode();

			EvaluateWatches(out _);

			ShowNextStatement(_executionContext.ExecutionState.Stack);

			if (_executionContext.ExecutionState.CurrentError != null)
				PresentError(_executionContext.ExecutionState.CurrentError);
		}
	}

	public void Continue()
	{
		if (_executionContext == null)
		{
			if (!Restart())
				return;
		}
		else
			RestoreOutput();

		if (_executionContext.ExecutionState.IsTerminated)
			ExecutionEpilogue();
		else
		{
			_executionContext.Controls.ContinueExecution();

			UnpauseExecution();
		}
	}

	public void Step()
	{
		if (_executionContext == null)
		{
			if (!Restart())
				return;

			_executionContext.Controls.WaitForInterruption();

			if (_executionContext.ExecutionState.IsTerminated)
				ExecutionEpilogue();
			else
			{
				SaveOutput();
				SetIDEVideoMode();

				ShowNextStatement(_executionContext.ExecutionState.Stack);
			}
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

		if (!Machine.KeepRunning)
			return;

		_executionContext.CloseAllFiles();

		foreach (var watch in _watches)
		{
			watch.LastValue = null;
			watch.LastValueFormatted = null;
		}

		var outputLibrary = _executionContext.VisualLibrary;

		var (savedCursorX, savedCursorY) = (outputLibrary.CursorX, outputLibrary.CursorY);

		if (outputLibrary is TextLibrary outputTextLibrary)
		{
			outputTextLibrary.UpdateCharacterLineWindow(
				0,
				outputTextLibrary.Height - 1);

			if (savedCursorY == outputLibrary.CharacterHeight - 1)
			{
				outputLibrary.ScrollText();
				savedCursorY--;
			}
		}

		outputLibrary.MoveCursor(savedCursorX, savedCursorY);

		SaveOutput();

		outputLibrary.MoveCursor(0, outputLibrary.CharacterHeight - 1);
		outputLibrary.WriteText("Press any key to continue");

		WaitForKey();

		DisassociateWatches();

		SetIDEVideoMode();

		_executionContext = null;
	}
}
