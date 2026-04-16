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

using Thread = System.Threading.Thread;

namespace QBX.DevelopmentEnvironment;

public partial class Program
{
	Thread? _executionThread;
	ExecutionContext? _executionContext;
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

			_executionContext = null;
			_executionThread = null;
		}
		catch { }
	}

	[MemberNotNullWhen(true, nameof(_executionContext))]
	public bool Restart()
	{
		Terminate();

		if (!EnsureAllCodeIsParsed())
			return false;

		_compilation = new Compilation();

		bool chainExecution = false;

		if (_executionContext != null)
		{
			chainExecution = _executionContext.ExecutionState.ChainExecution;

			if (chainExecution)
				_compilation.CommonBlocks = _executionContext.CommonBlocks;
			else
				_executionContext = null; // Disconnect from previous context
		}

		try
		{
			foreach (var nativeProcedure in QLBs.SelectMany(qlb => qlb.Exports))
				_compilation.RegisterNativeProcedure(nativeProcedure);

			foreach (var file in LoadedFiles)
			{
				if (file.IncludeInBuild && (file is CompilationUnit unit))
				{
					var compiler = new Compiler(unit.IdentifierRepository);

					compiler.DetectDelayLoops = DetectDelayLoops;

					compiler.Compile(unit, _compilation);
				}
			}

			if (!_compilation.ResolveUnresolvedCalls(out var errorModule))
			{
				throw CompilerException.SubprogramNotDefined(
					errorModule.UnresolvedReferences.GetFirstUnresolvedStatementSourceToken());
			}
		}
		catch (Exception e)
		{
			PresentError(e);
			return false;
		}

		_compilation.SetDefaultEntrypoint();

		AssociateWatches(_compilation);

		RestoreOutput();
		Machine.VideoFirmware.SetMode(3);

		var drawProcessor = _executionContext?.DrawProcessor ?? new DrawProcessor();

		_executionContext = new ExecutionContext(Machine, PlayProcessor, drawProcessor, EventHub, _compilation.CommonBlocks, _executionContext?.CommonBlockStorage);
		_executionContext.EventCheckGranularity = EventCheckGranularity;
		_executionContext.CommandLine.Set(ProgramCommandLine);
		_executionContext.Controls.Break();

		foreach (var qlb in QLBs)
			qlb.ExecutionContext = _executionContext;

		_executionContext.ReplaceProgram +=
			(_, args) =>
			{
				// We're running on a different thread, but the DevelopmentEnvironment thread
				// is blocked inside a call to _executionContext.Controls.WaitForInterruption.
				Load(
					args.Reader,
					args.FilePath,
					replaceExistingProgram: true);
			};

		_executionThread = new Thread(
			() =>
			{
				try
				{
					Thread.CurrentThread.CurrentCulture = BasicCulture.Instance;
					EventHub.ClearAllEvents();
					_executionContext.Run(_compilation, chainExecution);
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

	void UnpauseExecution(Action action)
	{
		do
		{
			if (_executionContext!.ExecutionState.ChainExecution)
				Restart();

			lock (_executionContext!.Controls.Sync)
			{
				action();

				using (Machine.DOS.EnableBreak())
					_executionContext.Controls.WaitForInterruption();
			}
		} while (_executionContext.ExecutionState.ChainExecution);

		// Purge input buffer
		while (Machine.Keyboard.GetNextEvent() is not null)
			;

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
			UnpauseExecution(
				action: () => _executionContext.Controls.ContinueExecution());
		}
	}

	public void Step()
	{
		if (_executionContext == null)
		{
			if (!Restart())
				return;

			_executionContext.Controls.WaitForStartUp();

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

			UnpauseExecution(
				action: () => _executionContext.Controls.ExecuteOneStatement());
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

		outputLibrary.SetActivePage(Machine.VideoFirmware.VisiblePageNumber);

		var (savedCursorX, savedCursorY) = (outputLibrary.CursorX, outputLibrary.CursorY);

		if (outputLibrary is TextLibrary outputTextLibrary)
		{
			outputTextLibrary.UpdateCharacterLineWindow(
				0,
				outputTextLibrary.Height - 1);

			if (savedCursorY == outputLibrary.CharacterHeight - 1)
			{
				outputLibrary.ScrollTextUp();
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
