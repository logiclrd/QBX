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
					if (FocusedViewport.CompilationElement != element)
					{
						if (PrimaryViewport.CompilationElement == element)
							FocusedViewport = PrimaryViewport;
						else if (SplitViewport?.CompilationElement == element)
							FocusedViewport = SplitViewport;

						if (FocusedViewport.CompilationElement != element)
							FocusedViewport.SwitchTo(element);
					}

					FocusedViewport.CursorX = _nextStatement.SourceColumn;
					FocusedViewport.CursorY = line.SourceLineIndex.Value;
				}

				// We are invoked as part of a key handler in ProcessTextEditorKey.
				// The caller will ensure that the viewport scroll is adjusted as
				// necessary.
			}
		}
	}

	public void PresentError(SyntaxErrorException error)
	{
		PresentError(error.Message, error.Token, avoidContext: true);
	}

	public void PresentError(CompilerException error)
	{
		PresentError(error.Message, error.Context, avoidContext: true);

		_errorToken = error.Context;
	}

	public void PresentError(RuntimeException error)
	{
		PresentError(error.Message, error.Context, avoidContext: true);

		_errorToken = error.Context;
	}

	public void PresentError(string errorMessage, Token? context = null, bool avoidContext = false)
	{
		if ((context?.OwnerStatement is Statement statement)
		 && (statement.CodeLine is CodeLine line)
		 && (line.CompilationElement is CompilationElement element))
		{
			if (FocusedViewport.CompilationElement != element)
				FocusedViewport.SwitchTo(element);

			FocusedViewport.ScrollCursorIntoView(
				newCursorX: context.Column, newCursorY: context.Line,
				FocusedViewport.ScrollX, FocusedViewport.ScrollY,
				ViewportPositioningPriority.Cursor,
				viewportWidth: TextLibrary.CharacterWidth - 2);
		}

		var dialog = ShowDialog(new ErrorDialog(Machine, Configuration, errorMessage));

		if (avoidContext)
		{
			if ((TextLibrary.CursorY >= dialog.Y)
			 && (TextLibrary.CursorY <= dialog.Y + dialog.Height)) // include the shadow
				dialog.Y = TextLibrary.Height - dialog.Height - 1;
		}
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
		if ((FocusedViewport.CompilationUnit == null)
		 || (FocusedViewport.CompilationElement == null))
			return;

		try
		{
			var dialog = new InstantWatchDialog(Machine, Configuration);

			dialog.SetExpression(subject);

			var instantWatch = new Watch(
				FocusedViewport.CompilationUnit,
				FocusedViewport.CompilationElement,
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
			PresentError("Invalid expression for Instant Watch");
		}
	}

	public void InteractiveAddWatchpoint()
	{
		var viewport = FocusedViewport;

		if ((FocusedViewport.CompilationUnit == null)
		 || (FocusedViewport.CompilationElement == null))
			viewport = PrimaryViewport;

		var dialog = new WatchpointDialog(Machine, Configuration);

		dialog.AddWatch +=
			() =>
			{
				var watchpoint = new Watch(
					viewport.CompilationUnit!,
					viewport.CompilationElement!,
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

		if ((FocusedViewport.CompilationUnit == null)
		 || (FocusedViewport.CompilationElement == null))
			return;

		var watch = new Watch(
			FocusedViewport.CompilationUnit,
			FocusedViewport.CompilationElement,
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
		var routines = compilation.AllRegisteredRoutines.ToDictionary(key => key.Source);

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

		if ((_compiler != null)
		 && (_compilation != null)
		 && (_executionContext != null)
		 && (currentRoutine != null))
		{
			foreach (var watch in watches)
			{
				watch.LastValueFormatted = null;

				if (ReferenceEquals(watch.Routine, currentRoutine))
				{
					try
					{
						var lexer = new Lexer(watch.Expression);

						var tokens = lexer.ToList();

						tokens.RemoveAll(token => token.Type == TokenType.Whitespace);

						var parsedSubject = Parser.ParseExpression(tokens, lexer.EndToken);

						var evaluable = _compiler.CompileExpression(parsedSubject, currentRoutine.Mapper, _compilation);

						watch.LastValue = evaluable.Evaluate(_executionContext, stackFrame);

						if (watch.IsWatchPoint)
							@break |= !watch.LastValue.IsZero;
						else
						{
							var emitter = new PrintEmitter(_executionContext);

							var value = new StringValue();

							emitter.CaptureOutputTo(value);
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
