using System;
using System.Collections.Generic;
using System.Diagnostics;

using QBX.ExecutionEngine.Compiled;
using QBX.ExecutionEngine.Compiled.Statements;
using QBX.ExecutionEngine.Execution.Variables;
using QBX.Firmware;
using QBX.Hardware;

namespace QBX.ExecutionEngine.Execution;

// ON [LOCAL] ERROR:
// - error handler state lives in the stack frame, which is by definition
//   the root frame if "LOCAL" isn't specified
// - handler must live in the same Routine as the stack frame it's registered in
// - ON ERROR (non-local) can be run at any time in any context and changes the
//   registered handler in the root frame
// - execution of the handler re-enters the associated stack frame if necessary
// - the execution of the handler is modelled as a type of Call
// - RESUME retries the statement that failed
// - RESUME NEXT also goes back but skips the statement that failed
// - if an error happens and there's already a return location from a previous
//   error being handled, then it doesn't get handled
// - if code flows off the end of the routine (as opposed to the program being explicitly
//   ENDed or EXIT SUB/FUNCTIONed) then an error "No RESUME" is raised

public class ExecutionContext
{
	public Machine Machine;
	public VisualLibrary VisualLibrary;
	public PlayProcessor PlayProcessor;

	public IReadOnlyExecutionState ExecutionState => _executionState;
	public IExecutionControls Controls => _executionState;

	public RuntimeState RuntimeState => _runtimeState;

	ExecutionState _executionState;

	StackFrame? _rootFrame;
	StatementPath? _goTo;

	ErrorHandler? _mainErrorHandler = null;
	Stack<ErrorHandler> _localErrorHandlers = new Stack<ErrorHandler>();

	RuntimeState _runtimeState = new RuntimeState();

	public readonly ErrorNumberVariable ErrVariable = new ErrorNumberVariable();
	public readonly LongVariable ErlVariable = new LongVariable();

	public ExecutionContext(Machine machine, PlayProcessor playProcessor)
	{
		_executionState = new ExecutionState();

		Machine = machine;
		VisualLibrary = new TextLibrary(machine);
		PlayProcessor = playProcessor;
	}

	public void SetErrorHandler(ErrorResponse response, StatementPath? handlerPath = null)
	{
		if ((response == ErrorResponse.ExecuteHandler) && (handlerPath == null))
			throw new Exception("Internal error: SetErrorHandler called with ErrorResponse.ExecuteHandler but no handlerPath");

		_mainErrorHandler ??= new ErrorHandler() { StackFrame = _rootFrame };
		_mainErrorHandler.Response = response;
		_mainErrorHandler.HandlerPath = handlerPath;
	}

	public void ClearErrorHandler(CodeModel.Statements.Statement source)
	{
		if (_rootFrame!.IsHandlingError)
		{
			// If this stack frame is already handling an error, the dispatch
			// has pulled the handler off of the error handlers stack and
			// will be reinstalling it on resume. That reinstallation doesn't
			// support clearing the handler. This matches QuickBASIC's behaviour.
			throw RuntimeException.IllegalFunctionCall(source);
		}

		_mainErrorHandler = null;
	}

	public void SetLocalErrorHandler(StackFrame stackFrame, ErrorResponse response, StatementPath? handlerPath = null)
	{
		if ((response == ErrorResponse.ExecuteHandler) && (handlerPath == null))
			throw new Exception("Internal error: SetLocalErrorHandler called with ErrorResponse.ExecuteHandler but no handlerPath");

		if (stackFrame.IsHandlingError)
		{
			// If this stack frame is already handling an error, then
			//
			// * We don't want to be able to re-entrantly catch errors
			//   that happen during the handling.
			// * The dispatch of the error handler has pulled the
			//   handler off the stack and will be reinstalling it on
			//   exit.
			//
			// So, we just stash the change and defer processing.

			var newHandler = new ErrorHandler();

			newHandler.StackFrame = stackFrame;
			newHandler.Response = response;
			newHandler.HandlerPath = handlerPath;

			stackFrame.NewErrorHandler = newHandler;

			return;
		}

		// If the top of the stack is not a match for the stack frame,
		// then it is further up the stack, so we need to make a new
		// error handler frame.

		if (_localErrorHandlers.TryPeek(out var handler))
		{
			if (handler.StackFrame != stackFrame)
				handler = null;
		}

		if (handler == null)
		{
			handler = new ErrorHandler();
			handler.StackFrame = stackFrame;
		}

		handler.Response = response;
		handler.HandlerPath = handlerPath;
	}

	public void ClearLocalErrorHandler(StackFrame stackFrame, CodeModel.Statements.Statement? source)
	{
		if (stackFrame.IsHandlingError)
		{
			// If this stack frame is already handling an error, the dispatch
			// has pulled the handler off of the error handlers stack and
			// will be reinstalling it on resume. That reinstallation doesn't
			// support clearing the handler. This matches QuickBASIC's behaviour.
			throw RuntimeException.IllegalFunctionCall(source);
		}

		if (_localErrorHandlers.TryPeek(out var handler)
		 && (handler.StackFrame == stackFrame))
			_localErrorHandlers.Pop();
	}

	public int Run(Compilation compilation)
	{
		var entrypoint = compilation.EntrypointRoutine;

		if (entrypoint == null)
			throw new Exception("The Compilation's EntrypointRoutine is not set");

		_rootFrame = CreateFrame(
			entrypoint.Module,
			entrypoint,
			System.Array.Empty<Variable>());

		_executionState.StartExecution(_rootFrame);

		try
		{
			try
			{
				Call(entrypoint, _rootFrame);
			}
			catch (EndProgram) { }

			int exitCode = _rootFrame.Variables[0].CoerceToInt(context: null);

			return exitCode;
		}
		catch (GoTo)
		{
			Debugger.Break();
			throw new Exception("Internal error: GoTo was thrown with a TargetFrame that didn't match anything");
		}
		catch (TerminatedException)
		{
			return -1;
		}
		finally
		{
			_rootFrame = null;

			_executionState.EndExecution();
		}
	}

	public void SetExitCode(int exitCode)
	{
		_rootFrame?.Variables[0].SetData(exitCode);
	}

	public void Dispatch(Executable? executable, StackFrame stackFrame)
	{
		if (executable != null)
		{
			if (_goTo != null)
			{
				int subsequenceIndex = _goTo.Pop();

				// I don't think this should actually happen, it should always
				// end on an index into a sequence. Better safe than sorry, though.
				if (_goTo.Count == 0)
					_goTo = null;

				if (executable.SelfSequenceDispatch)
					executable.Dispatch(this, stackFrame, subsequenceIndex, ref _goTo);
				else
				{
					var subsequence = executable.GetSequenceByIndex(subsequenceIndex);

					if (subsequence == null)
						throw new Exception("Internal Error: ExecutionPath specified subsequence " + subsequenceIndex + " within a " + executable.GetType().Name + " and it does not exist");

					Dispatch(subsequence, stackFrame);
				}
			}
			else
			{
				bool retryStatement = false;

				do
				{
					if (executable.CanBreak)
						_executionState.NextStatement(executable.Source);

					RuntimeException.LastLineNumber = executable.LineNumberForErrorReporting;

					try
					{
						executable.Execute(this, stackFrame);
					}
					catch (RuntimeException error)
					{
						ErrorHandler? handler = null;

						if (!stackFrame.IsHandlingError)
						{
							if (_localErrorHandlers.TryPeek(out handler))
							{
								if (handler.StackFrame == stackFrame)
									_localErrorHandlers.Pop();
								else
									handler = null;
							}

							handler ??= _mainErrorHandler;
						}

						if (handler != null)
							CallErrorHandler(handler, error, switchFrame: handler.StackFrame != stackFrame, out retryStatement);
						else
						{
							_executionState.Error(error);
							retryStatement = true;
						}
					}
				} while (retryStatement);
			}
		}
	}

	public void Dispatch(Sequence? sequence, StackFrame stackFrame)
	{
		if (sequence != null)
		{
			int startIndex = 0;

			if (_goTo != null)
			{
				startIndex = _goTo.Pop();
				if (_goTo.Count == 0)
					_goTo = null;
			}
			else
			{
				foreach (var statement in sequence.InjectedStatements)
					Dispatch(statement, stackFrame);
			}

			for (int i = startIndex; i < sequence.Count; i++)
				Dispatch(sequence[i], stackFrame);
		}
	}

	private void CallErrorHandler(ErrorHandler handler, RuntimeException error, bool switchFrame, out bool retryStatement)
	{
		retryStatement = false;

		if (handler.Response == ErrorResponse.SkipStatement)
			return;

		var stackFrame = handler.StackFrame ?? throw new Exception("Internal error: ErrorHandler does not have StackFrame set");
		var handlerPath = handler.HandlerPath ?? throw new Exception("Internal error: ErrorHandler's Response is not SkipStatement but it has no HandlerPath set");

		try
		{
			stackFrame.IsHandlingError = true;

			ErrVariable.Value = (short)error.ErrorNumber;
			ErlVariable.Value = error.LineNumber;

			_goTo = handler.HandlerPath.Clone();

			Call(stackFrame.Routine, stackFrame, enterRoutine: switchFrame, handlingError: true);

			// TODO: find end token of the compilation element
			throw RuntimeException.NoResume(stackFrame.CurrentStatement);
		}
		catch (ExitRoutine exitRoutine)
		{
			exitRoutine.StackFrame = stackFrame;
			throw;
		}
		catch (Resume resume)
		{
			stackFrame.IsHandlingError = false;

			if (handler.StackFrame == _rootFrame)
				_mainErrorHandler = _rootFrame.NewErrorHandler ?? handler;
			else
				_localErrorHandlers.Push(stackFrame.NewErrorHandler ?? handler);

			if (resume.GoTo != null)
				throw resume.GoTo;

			retryStatement = resume.RetryStatement;
		}
		finally
		{
			stackFrame.IsHandlingError = false;

			ErrVariable.Value = 0;
			ErlVariable.Value = 0;
		}
	}

	static Variable s_dummyVariable = new DummyVariable();

	public Variable Call(Routine routine, Variable[] arguments)
	{
		StackFrame frame;

		if (routine.UseRootFrame)
		{
			frame = _rootFrame ?? throw new Exception("No root frame");

			for (int i=0; i < arguments.Length; i++)
				frame.Variables[routine.ParameterVariableIndices[i]] = arguments[i];
		}
		else
			frame = CreateFrame(routine.Module, routine, arguments);

		try
		{
			return Call(routine, frame);
		}
		finally
		{
			ClearLocalErrorHandler(frame, source: null);
		}
	}

	Variable Call(Routine routine, StackFrame frame, bool enterRoutine = true, bool handlingError = false)
	{
		if (enterRoutine)
			_executionState.EnterRoutine(routine, frame);

		try
		{
		goTo_:
			try
			{
				Dispatch(routine, frame);
			}
			catch (GoTo goTo)
			{
				if ((goTo.TargetFrame != null) && (goTo.TargetFrame != frame))
					throw; // "RESUME linenumber/label" in a different stack frame -- keep searching

				_goTo = goTo.StatementPath.Clone();
				goto goTo_;
			}
			catch (ExitRoutine exitRoutine)
			{
				if ((exitRoutine.StackFrame != null)
				 && (exitRoutine.StackFrame != frame))
				{
					// This ExitRoutine came from an EXIT SUB/EXIT FUNCTION inside an
					// event handler that was handling an error from further down the
					// call stack.
					//
					//   SUB a                                1
					//   |- ON LOCAL ERROR GOTO x             1
					//   |- b                                 1
					//   `- SUB b                             2
					//      :                                 2
					//      :  d                              2
					//      `- SUB d                          3
					//         |- ERROR                       3
					//         `- SUB a:handler               1
					//            `- EXIT SUB => ExitRoutine  1
					//
					// (2 here represents any number of intermediary calls, including
					// possibly none)
					//
					// Since StackFrame doesn't match, we're frame 3, and we need
					// to keep walking up the stack so that 1 can process this
					// ExitRoutine.
					throw;
				}
			}

			if (routine.ReturnType != null)
				return frame.Variables[routine.ReturnValueVariableIndex];
			else
				return s_dummyVariable;
		}
		finally
		{
			if (enterRoutine)
				_executionState.ExitRoutine();
		}
	}

	StackFrame CreateFrame(Module module, Routine routine, Variable[] arguments)
	{
		var variableTypes = routine.VariableTypes;
		var linkedVariables = routine.LinkedVariables;

		if (arguments.Length > variableTypes.Count)
			throw new Exception("Internal error: Variable slots not properly allocated for arguments");

		int totalSlots = variableTypes.Count;

		var variables = new Variable[totalSlots];

		int argumentOffset = routine.ReturnType != null ? 1 : 0;

		arguments.CopyTo(variables, argumentOffset);

		if (_rootFrame != null)
		{
			foreach (var link in linkedVariables)
				variables[link.LocalIndex] = _rootFrame.Variables[link.RootIndex];
		}
		else
		{
			if (linkedVariables.Count > 0)
				throw new Exception("Internal error: Creating frame with linked variables when there is no root frame");
		}

		for (int i = 0; i < variableTypes.Count; i++)
		{
			if (variables[i] == null)
			{
				var type = variableTypes[i];

				if (type.IsArray)
					variables[i] = Variable.ConstructArray(type);
				else
					variables[i] = Variable.Construct(type);
			}
		}

		return new StackFrame(routine, variables);
	}
}
