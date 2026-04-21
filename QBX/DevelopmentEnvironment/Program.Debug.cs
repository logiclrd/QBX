using System;
using System.Collections.Generic;
using System.Linq;

using QBX.CodeModel;
using QBX.CodeModel.Statements;
using QBX.DevelopmentEnvironment.Dialogs;
using QBX.ExecutionEngine;
using QBX.ExecutionEngine.Compiled;
using QBX.ExecutionEngine.Compiled.Statements;
using QBX.ExecutionEngine.Execution;
using QBX.Firmware;
using QBX.LexicalAnalysis;
using QBX.Parser;

namespace QBX.DevelopmentEnvironment;

public partial class Program
{
	public Statement? NextStatement => _nextStatement;
	public Routine? NextStatementRoutine => _nextStatementRoutine;
	public Token? ErrorToken => _errorToken;
	public IReadOnlySet<CodeLine> Breakpoints => _breakpoints;
	public IReadOnlyList<Watch> Watches => _watches;

	public const int MaxWatches = 16;

	Statement? _nextStatement = null;
	Routine? _nextStatementRoutine = null;
	Token? _errorToken = null;
	HashSet<CodeLine> _breakpoints = new HashSet<CodeLine>();
	List<Watch> _watches = new List<Watch>();

	void ActivateViewportForElement(IEditableElement element)
	{
		if (FocusedViewport.EditableElement != element)
		{
			if (PrimaryViewport.EditableElement == element)
				FocusedViewport = PrimaryViewport;
			else if (SplitViewport?.EditableElement == element)
				FocusedViewport = SplitViewport;

			if (FocusedViewport == HelpViewport)
				FocusedViewport = PrimaryViewport;

			if (FocusedViewport.EditableElement != element)
				FocusedViewport.SwitchTo(element);
		}
	}

	private bool ClearNextStatement()
	{
		bool hadNextStatement = (_nextStatement != null);

		_nextStatement = null;
		_nextStatementRoutine = null;

		return hadNextStatement;
	}

	private void ShowNextStatement(IEnumerable<StackFrame> stack)
	{
		var currentFrame = stack.FirstOrDefault();

		if (currentFrame != null)
		{
			_nextStatement = currentFrame.CurrentStatement;
			_nextStatementRoutine = currentFrame.Routine;

			if (_nextStatement != null)
			{
				if ((_nextStatement.CodeLine is CodeLine line)
				 && (line.CompilationElement is CompilationElement element))
				{
					ActivateViewportForElement(element);

					FocusedViewport.CursorX = _nextStatement.SourceColumn;
					FocusedViewport.CursorY = line.SourceLineIndex.Value;
				}

				// We are invoked as part of a key handler in ProcessTextEditorKey.
				// The caller will ensure that the viewport scroll is adjusted as
				// necessary.
			}
		}
	}

	public void PresentError(Exception e, ErrorSource errorSource = ErrorSource.Program)
	{
		if (e is SyntaxErrorException syntaxError)
			PresentError(syntaxError);
		else if (e is CompilerException compileError)
			PresentError(compileError);
		else if (e is RuntimeException runtimeError)
			PresentError(runtimeError, errorSource);
		else
			PresentError(e.Message);
	}

	public void PresentError(SyntaxErrorException error)
	{
		PresentError(error.Message, error.Token, avoidContext: true);
	}

	public void PresentError(CompilerException error)
	{
		PresentError(error.Message, error.Context, avoidContext: true);
	}

	public void PresentError(RuntimeException error, ErrorSource source = ErrorSource.Program)
	{
		PresentError(error.Message, error.ErrorNumber, error.Context, source, avoidContext: true);
	}

	public void PresentError(string errorMessage, Token? context = null, bool avoidContext = false)
	{
		PresentError(errorMessage, errorNumber: null, context, ErrorSource.Program, avoidContext);
	}

	public void PresentError(string errorMessage, int? errorNumber, Token? context, ErrorSource source, bool avoidContext)
	{
		if ((context?.OwnerStatement is Statement statement)
		 && (statement.CodeLine is CodeLine line)
		 && (line.CompilationElement is CompilationElement element))
		{
			ActivateViewportForElement(element);

			FocusedViewport.ScrollCursorIntoView(
				newCursorX: context.Column, newCursorY: context.Line - element.FirstLineIndex,
				FocusedViewport.ScrollX, FocusedViewport.ScrollY,
				ViewportPositioningPriority.Cursor,
				viewportWidth: TextLibrary.CharacterWidth - 2);
		}

		_errorToken = context;

		var dialog = ShowDialog(new ErrorDialog(Machine, Configuration, errorMessage, errorNumber, source));

		if (avoidContext)
		{
			if ((TextLibrary.CursorY >= dialog.Y)
			 && (TextLibrary.CursorY <= dialog.Y + dialog.Height)) // include the shadow
				dialog.Y = TextLibrary.Height - dialog.Height - 1;
		}

		dialog.Closed +=
			(_, _) =>
			{
				_errorToken = null;
			};
	}

	public void ToggleBreakpoint(CodeLine codeLine)
	{
		if (!_breakpoints.Contains(codeLine))
			SetBreakpoint(codeLine);
		else
			ClearBreakpoint(codeLine);
	}

	public void SetBreakpoint(CodeLine codeLine)
	{
		foreach (var statement in codeLine.Statements)
			statement.IsBreakpoint = true;

		_breakpoints.Add(codeLine);
	}

	public void ClearBreakpoint(CodeLine codeLine)
	{
		foreach (var statement in codeLine.Statements)
			statement.IsBreakpoint = false;

		_breakpoints.Remove(codeLine);
	}

	public void ShowInstantWatch(Mapper? mapper, string subject)
	{
		if ((FocusedViewport.EditableUnit is not CompilationUnit unit)
		 || (FocusedViewport.EditableElement is not CompilationElement element))
			return;

		try
		{
			var dialog = new InstantWatchDialog(Machine, Configuration);

			dialog.SetExpression(subject);

			var instantWatch = new Watch(
				unit,
				element,
				subject);

			instantWatch.Routine = _nextStatementRoutine;

			EvaluateWatch(instantWatch);

			dialog.SetWatch(instantWatch);

			dialog.AddWatchClicked +=
				() =>
				{
					AddWatch(instantWatch);
					dialog.Close();
				};

			if (_watches.Count >= MaxWatches)
				dialog.EnableAddWatch = false;

			ShowDialog(dialog);
		}
		catch
		{
			PresentError("Invalid expression for Instant Watch", 315, context: null, ErrorSource.Program, avoidContext: false);
		}
	}

	public void InteractiveAddWatch()
	{
		var viewport = FocusedViewport;

		if ((FocusedViewport.EditableUnit is not CompilationUnit)
		 || (FocusedViewport.EditableElement is not CompilationElement))
			viewport = PrimaryViewport;

		if ((viewport.EditableUnit is not CompilationUnit unit)
		 || (viewport.EditableElement is not CompilationElement element))
		{
			PresentError("Internal error: Could not locate a CompilationElement to operate on.");
			return;
		}

		var dialog = new AddWatchDialog(Machine, Configuration);

		dialog.AddWatch +=
			() =>
			{
				var watchpoint = new Watch(
					unit,
					element,
					dialog.WatchExpression);

				AddWatch(watchpoint);
			};

		ShowDialog(dialog);
	}

	public void InteractiveAddWatchpoint()
	{
		var viewport = FocusedViewport;

		if ((FocusedViewport.EditableUnit == null)
		 || (FocusedViewport.EditableElement == null))
			viewport = PrimaryViewport;

		if ((viewport.EditableUnit is not CompilationUnit unit)
		 || (viewport.EditableElement is not CompilationElement element))
		{
			PresentError("Internal error: Could not locate a CompilationElement to operate on.");
			return;
		}

		var dialog = new WatchpointDialog(Machine, Configuration);

		dialog.AddWatch +=
			() =>
			{
				var watchpoint = new Watch(
					unit,
					element,
					dialog.WatchpointExpression);

				watchpoint.IsWatchPoint = true;

				AddWatch(watchpoint);
			};

		ShowDialog(dialog);
	}

	public void AddWatch(string expression, bool watchPoint)
	{
		if (_watches.Count == MaxWatches)
			return;

		if (FocusedViewport == HelpViewport)
			FocusedViewport = PrimaryViewport;

		if ((FocusedViewport.EditableUnit is not CompilationUnit unit)
		 || (FocusedViewport.EditableElement is not CompilationElement element))
			return;

		var watch = new Watch(
			unit,
			element,
			expression);

		watch.IsWatchPoint = watchPoint;

		_watches.Add(watch);

		if (watchPoint)
			EnableWatchpointChecks();
	}

	public void AddWatch(Watch watch)
	{
		if (_watches.Count < MaxWatches)
		{
			_watches.Add(watch);

			mnuDebugDeleteWatch.IsEnabled = true;
			mnuDebugDeleteAllWatch.IsEnabled = true;
		}
	}

	public void RemoveWatchAt(int index)
	{
		if ((index >= 0) && (index < _watches.Count))
		{
			_watches.RemoveAt(index);

			if (_watches.Count == 0)
			{
				mnuDebugDeleteWatch.IsEnabled = false;
				mnuDebugDeleteAllWatch.IsEnabled = false;
			}

			if (!_watches.Any(watch => watch.IsWatchPoint))
				DisableWatchpointChecks();
		}
	}

	public void ClearWatches()
	{
		_watches.Clear();

		mnuDebugDeleteWatch.IsEnabled = false;
		mnuDebugDeleteAllWatch.IsEnabled = false;
	}

	public void AssociateWatches(Compilation compilation)
	{
		var routines = compilation.AllRegisteredRoutines
			.Where(routine => !routine.IsDefFn)
			.ToDictionary(key => key.Source);

		foreach (var watch in _watches)
			routines.TryGetValue(watch.CompilationElement, out watch.Routine);
	}

	public void DisassociateWatches()
	{
		foreach (var watch in _watches)
			watch.Routine = null;
	}

	IReadOnlyExecutionState? _hooked = null;

	void EnableWatchpointChecks()
	{
		if ((_executionContext != null)
		 && (_hooked != _executionContext.Controls))
		{
			_hooked = _executionContext.ExecutionState;
			_hooked.CheckWatchpoints += EvaluateWatchPoints;
		}
	}

	void DisableWatchpointChecks()
	{
		if (_hooked != null)
		{
			_hooked.CheckWatchpoints -= EvaluateWatchPoints;
			_hooked = null;
		}
	}

	public bool EvaluateWatch(Watch watch)
	{
		var stackFrame = _executionContext?.ExecutionState.Stack.FirstOrDefault();

		bool @break = false;

		if (stackFrame != null)
		{
			EvaluateWatches(
				stackFrame,
				[watch],
				out @break);
		}

		return @break;
	}

	public void EvaluateWatches(StackFrame stackFrame)
	{
		EvaluateWatches(
			stackFrame,
			_watches,
			out _);
	}

	public void EvaluateWatches(out bool @break)
	{
		var stackFrame = _executionContext?.ExecutionState.Stack.FirstOrDefault();

		@break = false;

		if (stackFrame != null)
		{
			EvaluateWatches(
				stackFrame,
				_watches,
				out @break);
		}
	}

	public bool EvaluateWatchPoints(StackFrame stackFrame)
	{
		EvaluateWatches(
			stackFrame,
			_watches.Where(watch => watch.IsWatchPoint),
			out var @break);

		return @break;
	}

	public void EvaluateWatches(StackFrame stackFrame, IEnumerable<Watch> watches, out bool @break)
	{
		@break = false;

		var currentRoutine = stackFrame.Routine;

		foreach (var watch in _watches)
			watch.LastValueFormatted = null;

		if ((_compilation != null)
		 && (_executionContext != null)
		 && (currentRoutine != null))
		{
			var unit = currentRoutine.Source.Owner;

			var parser = new BasicParser(unit.IdentifierRepository);
			var compiler = new Compiler(unit.IdentifierRepository);

			foreach (var watch in watches)
			{
				watch.LastValueFormatted = null;

				if (ReferenceEquals(watch.Routine, currentRoutine))
				{
					try
					{
						var lexer = new Lexer(watch.Expression, currentRoutine.Source);

						var tokens = lexer.ToList();

						tokens.RemoveAll(token => token.Type == TokenType.Whitespace);

						var parsedSubject = parser.ParseExpression(tokens, lexer.EndToken);

						var evaluable = compiler.CompileExpression(parsedSubject, currentRoutine.Mapper, _compilation, currentRoutine.Module);

						watch.LastValue = evaluable.Evaluate(_executionContext, stackFrame);

						if (watch.IsWatchPoint)
							@break |= !watch.LastValue.IsZero;
						else
						{
							var value = new StringValue();

							var emitter = new CapturingPrintEmitter(Machine, value);

							emitter.Emit(watch.LastValue);

							watch.LastValueFormatted = value;
						}
					}
					catch { }
				}
			}
		}
	}
}
