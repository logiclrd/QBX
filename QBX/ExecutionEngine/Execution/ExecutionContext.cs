using System;

using QBX.ExecutionEngine.Compiled;
using QBX.ExecutionEngine.Compiled.Statements;
using QBX.ExecutionEngine.Execution.Variables;
using QBX.Firmware;
using QBX.Hardware;

namespace QBX.ExecutionEngine.Execution;

// ON ERROR:
// - error handler state is global, not tied to stack frames
// - handler itself must live in the main module
// - ON ERROR can be run at any time in any context and changes the registered handler in the global state
// - execution of the handler uses the root stack frame, re-entering it if necessary
// - when an error occurs and a handler is active, the statement that caused the error
//   is stashed
// - RESUME retries the statement that failed
// - RESUME NEXT also goes back but skips the statement that failed
// - if an error happens and there's already a return location from a previous
//   error being handled, then it doesn't get handled
// - if code flows off the end of the main module (as opposed to the program being explicitly ENDed),
//   then an error "No RESUME" is raised

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

	RuntimeState _runtimeState = new RuntimeState();

	public ExecutionContext(Machine machine, PlayProcessor playProcessor)
	{
		_executionState = new ExecutionState();

		Machine = machine;
		VisualLibrary = new TextLibrary(machine);
		PlayProcessor = playProcessor;
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

			int exitCode = _rootFrame.Variables[0].CoerceToInt();

			return exitCode;
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
				if (executable.CanBreak)
					_executionState.NextStatement(executable.Source);

				executable.Execute(this, stackFrame);
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

		return Call(routine, frame);
	}

	Variable Call(Routine routine, StackFrame frame)
	{
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
				_goTo = goTo.StatementPath.Clone();
				goto goTo_;
			}
			catch (ExitRoutine) { }

			if (routine.ReturnType != null)
				return frame.Variables[routine.ReturnValueVariableIndex];
			else
				return s_dummyVariable;
		}
		finally
		{
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

		return new StackFrame(variables);
	}
}
