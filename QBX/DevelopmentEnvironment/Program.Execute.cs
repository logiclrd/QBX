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
using QBX.LexicalAnalysis;
using QBX.Parser;
using QBX.QuickLibraries;
using QBX.Utility;

using Thread = System.Threading.Thread;

namespace QBX.DevelopmentEnvironment;

public partial class Program
{
	PersistentRuntimeState _persistentRuntimeState;

	Thread? _executionThread;
	ExecutionContext? _executionContext;
	Compilation? _compilation;

	int _savedLastScreenMode;

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

	public void Terminate(bool keepOutput = false)
	{
		try
		{
			if ((_executionThread != null)
			 && _executionThread.IsAlive)
				_executionThread.IsBackground = true;

			if (_executionContext != null)
			{
				_savedLastScreenMode = _executionContext.RuntimeState.LastScreenMode;

				_executionContext.Controls.Terminate();
				_executionContext.CloseAllFiles();
			}

			_executionContext = null;
			_executionThread = null;

			if (!keepOutput)
				ResetProgramScreen();

			ClearNextStatement();
		}
		catch { }
	}

	public void ResetProgramScreen()
	{
		if ((_savedLastScreenMode == 0)
		 && (_savedVisualLibrary != null)
		 && (_savedVisualLibrary.CharacterWidth == 80)
		 && (_savedVisualLibrary.CharacterHeight == 25))
			ShowTextCursorInSavedOutput();
		else
		{
			_savedLastScreenMode = 0;

			ResetOutput();
		}

		_savedCharacterLineWindowStart = 0;
		_savedCharacterLineWindowEnd = _savedVisualLibrary.CharacterHeight - 2;

		SetIDEVideoMode();
		Render();
	}

	ExecutionContext? _chainFromContext = null;

	[MemberNotNull(nameof(_compilation))]
	public bool Compile(ExecutionContext? chainFromContext, out bool chainExecution, ref StatementPath? startingLineNumber, Action? prepareToPresentError = null)
	{
		_compilation = new Compilation();

		chainExecution = false;

		_chainFromContext = chainFromContext ?? _executionContext;

		if (_chainFromContext != null)
		{
			chainExecution = _chainFromContext.ExecutionState.ChainExecution;
			startingLineNumber = _chainFromContext.ExecutionState.PeekStartingLineNumber();

			if (chainExecution)
				_compilation.CommonBlocks = _chainFromContext.CommonBlocks;
			else
			{
				_savedLastScreenMode = _chainFromContext.RuntimeState.LastScreenMode;
				_executionContext = null; // Disconnect from previous context
			}
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
			prepareToPresentError?.Invoke();

			PresentError(e);
			return false;
		}

		_compilation.SetDefaultEntrypoint();

		return true;
	}

	[MemberNotNullWhen(true, nameof(_executionContext))]
	public bool Restart(bool keepOutput = false, StatementPath? startingLineNumber = null, Routine? embeddedInRoutine = null, ExecutionContext? chainFromContext = null, Action? prepareToPresentError = null)
	{
		Terminate(keepOutput);

		if (!EnsureAllCodeIsParsed())
			return false;

		if (!Compile(chainFromContext, out bool chainExecution, ref startingLineNumber, prepareToPresentError))
			return false;

		return StartExecution(chainExecution, startingLineNumber, embeddedInRoutine);
	}

	public bool StartExecution(bool chainExecution, StatementPath? startingLineNumber = null, Routine? embeddedRoutine = null)
	{
		if (_compilation == null)
			throw new Exception("Internal error: Start called with no ambient compilation");

		return StartExecution(_compilation, chainExecution, startingLineNumber, embeddedRoutine);
	}

	public bool StartExecution(Compilation compilation, bool chainExecution, StatementPath? startingLineNumber = null, Routine? embeddedRoutine = null)
	{
		AssociateWatches(compilation);

		RestoreOutput();

		if (!chainExecution)
		{
			if (Machine.VideoFirmware.LastModeNumber != 3)
				Machine.VideoFirmware.SetMode(3);
		}

		var drawProcessor = _executionContext?.DrawProcessor ?? new DrawProcessor();

		_executionContext = new ExecutionContext(Machine, _persistentRuntimeState, drawProcessor, EventHub, compilation.CommonBlocks, _executionContext?.CommonBlockStorage);
		_executionContext.RuntimeState.LastScreenMode = _savedLastScreenMode;
		_executionContext.EventCheckGranularity = EventCheckGranularity;
		_executionContext.CommandLine.Set(ProgramCommandLine.ToUpperInvariant());
		_executionContext.Controls.Break();

		if (_chainFromContext != null)
		{
			if (chainExecution)
				_executionContext.ChainFrom(_chainFromContext);

			_chainFromContext = null;
		}

		foreach (var qlb in QLBs)
			qlb.ExecutionContext = _executionContext;

		if (startingLineNumber != null)
			_executionContext.SetStartingLineNumber(startingLineNumber);

		_executionThread = new Thread(
			() =>
			{
				try
				{
					Thread.CurrentThread.CurrentCulture = BasicCulture.Instance;
					EventHub.ClearAllEvents();
					_executionContext.Run(compilation, chainExecution, embeddedRoutine);
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

	static readonly Identifier ImmediateRoutineName = Identifier.Standalone("@Immediate");

	bool ParseAndExecuteDirect(TextReader directCodeTextReader)
	{
		var immediateUnit = new CompilationUnit();
		var immediateElement = new CompilationElement(immediateUnit);

		immediateElement.Type = CompilationElementType.Main;
		immediateElement.Name = ImmediateRoutineName;

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

	Compilation? CompileDirect(CompilationUnit unit)
	{
		var compilation = new Compilation();

		try
		{
			var compiler = new Compiler(unit.IdentifierRepository);

			compiler.DetectDelayLoops = DetectDelayLoops;

			compiler.Compile(unit, compilation);

			if (!compilation.ResolveUnresolvedCalls(out var errorModule))
				return null;
		}
		catch
		{
			return null;
		}

		compilation.SetDefaultEntrypoint();

		return compilation;
	}

	bool ExecuteDirect(CodeLine line, CompilationUnit ephemeralUnit, CompilationElement ephemeralElement)
	{
		// Shouldn't ever happen, but just in case (and to satisfy the analyzer) :-)
		if (line.CompilationElement == null)
			return true;

		if (ephemeralElement.Name is not Identifier ephemeralElementName)
			throw new Exception("Internal error: ephemeral unit has no assigned name");

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

		Routine? main = null;

		if ((_executionContext == null) || (_compilation == null) || (_nextStatementRoutine == null))
		{
			_executionContext = null;
			_nextStatement = null;
			_nextStatementRoutine = null;

			// Optimistically try to compile & run the statement with no embedding.
			ephemeralElement.Type = CompilationElementType.Direct;

			var isolatedCompilation = CompileDirect(ephemeralUnit);

			if ((isolatedCompilation == null)
			 || (isolatedCompilation.Modules.Count == 0)
			 || !isolatedCompilation.Modules[0].Routines.TryGetValue(ephemeralElementName, out var embeddedRoutine)
			 || !embeddedRoutine.CanExecuteDirectWithoutEmbedding)
			{
				// We can't execute this line without embedding it into the main/current module. Start over.

				if (!EnsureAllCodeIsParsed())
					return false;

				StatementPath? ignored = null;

				if (!Compile(chainFromContext: null, out bool chainExecution, startingLineNumber: ref ignored))
					return false;

				if (chainExecution)
					throw new Exception("Internal error: Unexpected chain execution");

				var mainModule = _compilation.Modules[0];

				main = _compilation.EntrypointRoutine
					?? throw new Exception("Internal error: Module has no entrypoint routine");

				ephemeralElement.Type = main.Source.Type;

				try
				{
					var compiler = new Compiler(unit.IdentifierRepository);

					compiler.DetectDelayLoops = DetectDelayLoops;

					compiler.Compile(ephemeralUnit, _compilation, embedIn: main);
				}
				catch (Exception e)
				{
					PresentError(e);
					return false;
				}

				embeddedRoutine = _compilation.Modules.Last().Routines[ephemeralElementName];

				if (main.HasDuplicateLabels(embeddedRoutine))
					throw CompilerException.DuplicateLabel(context: null);
			}

			// This is tested here because a collision with an existing line number takes precedence.
			if ((line.LineNumber != null) || (line.Label != null))
				throw RuntimeException.IllegalInDirectMode(statement: null);

			bool success = StartExecution(
				isolatedCompilation ?? _compilation ?? throw new Exception("Internal error"),
				chainExecution: false,
				embeddedRoutine: embeddedRoutine);

			if (success)
				Continue(embeddedInRoutine: main);
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

				ephemeralElement.Type = _nextStatementRoutine.Source.Type;

				compiler.CompileDirect(line, _compilation, _nextStatementRoutine, sequence, executingFrame);

				// This is tested here because a collision with an existing line number takes precedence.
				if ((line.LineNumber != null) || (line.Label != null))
					throw RuntimeException.IllegalInDirectMode(statement: null);

				_nextStatementRoutine.ResolveJumpStatements(sequence);
			}
			catch (Exception e)
			{
				PresentError(e);
				return false;
			}

			RestoreOutput();

			_executionContext.Controls.ExecuteDirectOnResume(sequence);

			UnpauseExecution(
				action: () => _executionContext.Controls.ContinueExecution());

			// Still running? Do a pretend epilogue. :-)
			if ((_executionContext != null)
			 && !_executionContext.ExecutionState.IsTerminated
			 && _executionContext.ExecutionState.CollectDirectSequenceCompletedFlag())
				PauseOnOutput();
		}

		return true;
	}

	void UnpauseExecution(Action action)
	{
		void PurgeInputBuffer()
		{
			while (Machine.Keyboard.GetNextEvent() is not null)
				;
		}

		// During chain execution, when the new module is loaded, the current
		// execution state is completely cleared. We still need a reference to
		// that object, though.
		var executionContext = _executionContext!;

		bool alreadyPresentedError = false;

		do
		{
			if (executionContext.ExecutionState.ReplaceRunningProgram is ReplaceRunningProgram replacementProgram)
			{
				if (replacementProgram.ReplacementFilePath is string replacementFilePath)
				{
					int? replacementFileHandle = replacementProgram.ReplacementFileHandle;
					var errorContext = replacementProgram.ErrorContext;

					StreamReader reader;

					try
					{
						reader = DOSOpenFileReader(replacementFilePath, replacementFileHandle, errorContext);
					}
					catch (Exception e)
					{
						PurgeInputBuffer();

						SaveOutput();
						SetIDEVideoMode();

						if (_executionContext != null)
							UpdateAfterBreak();

						PresentError(e);

						return;
					}

					using (reader)
					{
						Load(
							reader,
							replacementFilePath,
							replaceExistingProgram: true,
							chainExecution: true,
							errorContext: errorContext);
					}
				}

				SaveOutput();

				bool success = Restart(
					chainFromContext: executionContext,
					startingLineNumber: replacementProgram.StartingLineNumber,
					keepOutput: true,
					prepareToPresentError:
						() =>
						{
							PurgeInputBuffer();

							SaveOutput();
							SetIDEVideoMode();

							alreadyPresentedError = true;
						});

				if (!success)
					break;

				executionContext = _executionContext ?? throw new Exception("Internal error: Restart did not create an ambient execution context");
			}

			lock (executionContext.Controls.Sync)
			{
				if (ClearNextStatement())
					executionContext.Controls.IgnoreExplicitBreakFromNextStatement();

				action();

				using (Machine.DOS.EnableBreak())
					executionContext.Controls.WaitForInterruption();
			}
		} while (executionContext.ExecutionState.ReplaceRunningProgram != null);

		PurgeInputBuffer();

		if (AbortOnBreak || (executionContext.ExitAutoRunToSystem && AutoRun))
			Machine.KeepRunning = false;
		else
		{
			// Having entered break mode, SYSTEM should no longer exit to system.
			AutoRun = false;

			if (!alreadyPresentedError)
			{
				if (executionContext.ExecutionState.IsTerminated)
					ExecutionEpilogue();
				else
				{
					SaveOutput();
					SetIDEVideoMode();

					UpdateAfterBreak();

					if (executionContext.ExecutionState.CurrentError != null)
						PresentError(executionContext.ExecutionState.CurrentError);
				}
			}
		}
	}

	void UpdateAfterBreak()
	{
		if (_executionContext == null)
			throw new Exception("Internal error: UpdateAfterBreak called with no execution context");

		RebuildCallsMenu();

		EvaluateWatches(out _);

		ShowNextStatement(_executionContext.ExecutionState.Stack);
	}

	public void Continue(Routine? embeddedInRoutine = null)
	{
		if (_executionContext == null)
		{
			if (!Restart(embeddedInRoutine: embeddedInRoutine))
				return;
		}
		else
			RestoreOutput();

		if (_executionContext.ExecutionState.IsTerminated
		 && (_executionContext.ExecutionState.ReplaceRunningProgram == null))
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
		var executionContext = _executionContext;

		if (executionContext == null)
			return; // ??

		if (!Machine.KeepRunning || _closeRequested)
			return;

		executionContext.CloseAllFiles();

		foreach (var watch in _watches)
		{
			watch.LastValue = null;
			watch.LastValueFormatted = null;
		}

		var outputLibrary = executionContext.VisualLibrary;

		outputLibrary.SetActivePage(Machine.VideoFirmware.VisiblePageNumber);

		/* Did I observe it QuickBASIC doing this at one point??
		 *
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
		*/

		SaveOutput();

		PauseOnOutput();

		DisassociateWatches();

		_savedLastScreenMode = executionContext.RuntimeState.LastScreenMode;
		_executionContext = null;
	}

	void PauseOnOutput()
	{
		if (_executionContext == null)
			return; // ??

		RestoreOutput();

		var outputLibrary = _executionContext.VisualLibrary;

		outputLibrary.SetActivePage(Machine.VideoFirmware.VisiblePageNumber);
		outputLibrary.MoveCursor(0, outputLibrary.CharacterHeight - 1);
		outputLibrary.UpdateCharacterLineWindow(outputLibrary.CharacterHeight - 1, outputLibrary.CharacterHeight - 1);
		outputLibrary.ClearCharacterLineWindow();
		outputLibrary.WriteText("Press any key to continue");

		WaitForKey();

		SetIDEVideoMode();
	}
}
