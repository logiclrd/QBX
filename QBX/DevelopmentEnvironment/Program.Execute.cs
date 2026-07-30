using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;

using QBX.CodeModel;
using QBX.ExecutionEngine;
using QBX.ExecutionEngine.Compiled;
using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Events;
using QBX.Firmware;
using QBX.LexicalAnalysis;
using QBX.Parser;
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

	public bool AbortOnBreak = false;

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
		if (Restart())
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

			ClearNextStatement();
		}
		catch { }
	}

	[MemberNotNullWhen(true, nameof(_executionContext))]
	public bool Restart(Action<Compilation>? configureCompilation = null)
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

		configureCompilation?.Invoke(_compilation);

		AssociateWatches(_compilation);

		RestoreOutput();

		if (Machine.VideoFirmware.LastModeNumber != 3)
			Machine.VideoFirmware.SetMode(3);

		var drawProcessor = _executionContext?.DrawProcessor ?? new DrawProcessor();

		_executionContext = new ExecutionContext(Machine, PlayProcessor, drawProcessor, EventHub, _compilation.CommonBlocks, _executionContext?.CommonBlockStorage);
		_executionContext.EventCheckGranularity = EventCheckGranularity;
		_executionContext.CommandLine.Set(ProgramCommandLine.ToUpperInvariant());
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

		_executionContext.WaitForRootFrame();

		return true;
	}

	bool ParseAndExecuteDirect(TextReader directCodeTextReader)
	{
		var immediateUnit = new CompilationUnit();
		var immediateElement = new CompilationElement(immediateUnit);

		immediateElement.Type = CompilationElementType.Main;
		immediateElement.Name = Routine.MainRoutineName;

		immediateUnit.AddElement(immediateElement);

		if (_nextStatementRoutine == null)
		{
			var mainModule = LoadedFiles.OfType<CompilationUnit>().FirstOrDefault();

			if (mainModule == null)
				return true;

			immediateElement.AttachToUnit(mainModule);
		}
		else
		{
			var nextStatementElement = _nextStatementRoutine.Source;
			var nextStatementUnit = nextStatementElement.Owner;

			immediateElement.AttachToUnit(nextStatementUnit);
		}

		var identifierRepository = immediateElement.Owner.IdentifierRepository;

		var parser = new BasicParser(identifierRepository);

		identifierRepository.IsLocked = true;

		try
		{
			var lexer = new Lexer(directCodeTextReader, immediateElement);

			var parsedCodeLine = parser.ParseCodeLines(lexer).SingleOrDefault();

			if (parsedCodeLine != null)
			{
				immediateElement.AddLine(parsedCodeLine);

				return ExecuteDirect(parsedCodeLine, immediateUnit, immediateElement);
			}
		}
		finally
		{
			identifierRepository.IsLocked = false;
		}

		return true;
	}

	bool ExecuteDirect(CodeLine line, CompilationUnit ephemeralUnit, CompilationElement ephemeralElement)
	{
		// Shouldn't ever happen, but just in case (and to satisfy the analyzer) :-)
		if (line.CompilationElement == null)
			return true;

		foreach (var statement in line.Statements)
		{
			if (!statement.IsLegalInDirectMode)
			{
				// This error does not highlight the associated statement in the Immediate window.
				PresentError(RuntimeException.IllegalInDirectMode(statement: null));
				return false;
			}
		}

		var unit = line.CompilationElement.Owner;

		// Is the program currently running?
		//   No => Compile the entire program, make the direct statement a line
		//         in a transient SUB and run it.
		//   Yes => Compile the direct statement to a Sequence and execute it in
		//          the context of the current next line

		if ((_executionContext == null) || (_compilation == null) || (_nextStatementRoutine == null))
		{
			// Dedicated execution
			bool success = false;

			try
			{
				success = Restart(
					compilation =>
					{
						var module = compilation.Modules[0];

						if (module.MainRoutine == null)
							throw new Exception("Internal error: Module has no main routine");

						var moduleMapper = module.MainRoutine.Mapper;

						var immediateRoutine = new Routine(compilation.Modules[0], moduleMapper, ephemeralElement, detached: true);

						var compiler = new Compiler(unit.IdentifierRepository);

						compiler.DetectDelayLoops = DetectDelayLoops;

						compiler.Compile(ephemeralUnit, compilation);

						compilation.EntrypointRoutine = compilation.Modules.Last().MainRoutine;
					});
			}
			catch (Exception e)
			{
				PresentError(e);
				return false;
			}

			if (success)
				Continue();
		}
		else
		{
			// Integrated execution
			var sequence = new Sequence();

			try
			{
				var compiler = new Compiler(unit.IdentifierRepository);

				compiler.DetectDelayLoops = DetectDelayLoops;

				var executingFrame = _executionContext.ExecutionState.Stack.First();

				compiler.CompileDirect(line, _compilation, _nextStatementRoutine, sequence, executingFrame);
			}
			catch (Exception e)
			{
				PresentError(e);
				return false;
			}

			using (Machine.DOS.EnableBreak())
				_executionContext.Controls.ExecuteDirect(sequence);
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
				if (ClearNextStatement())
					_executionContext.Controls.IgnoreBreakFromNextStatement();

				action();

				using (Machine.DOS.EnableBreak())
					_executionContext.Controls.WaitForInterruption();
			}
		} while (_executionContext.ExecutionState.ChainExecution);

		// Purge input buffer
		while (Machine.Keyboard.GetNextEvent() is not null)
			;

		if (AbortOnBreak || (_executionContext.ExitAutoRunToSystem && AutoRun))
			Machine.KeepRunning = false;
		else
		{
			// Having entered break mode, SYSTEM should no longer exit to system.
			AutoRun = false;

			if (_executionContext.ExecutionState.IsTerminated)
				ExecutionEpilogue();
			else
			{
				SaveOutput();
				SetIDEVideoMode();

				UpdateAfterBreak();

				if (_executionContext.ExecutionState.CurrentError != null)
					PresentError(_executionContext.ExecutionState.CurrentError);
			}
		}
	}

	void UpdateAfterBreak()
	{
		if (_executionContext == null)
			throw new Exception("Internal error: UpdateAfterBreak called with no execution context");

		EvaluateWatches(out _);

		ShowNextStatement(_executionContext.ExecutionState.Stack);
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
		{
			if (AbortOnBreak || (_executionContext.ExitAutoRunToSystem && AutoRun))
				Machine.KeepRunning = false;
			else
				ExecutionEpilogue();
		}
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

				UpdateAfterBreak();
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
		outputLibrary.UpdateCharacterLineWindow(outputLibrary.CharacterHeight - 1, outputLibrary.CharacterHeight - 1);
		outputLibrary.ClearCharacterLineWindow();
		outputLibrary.WriteText("Press any key to continue");

		WaitForKey();

		DisassociateWatches();

		SetIDEVideoMode();

		_executionContext = null;
	}
}
