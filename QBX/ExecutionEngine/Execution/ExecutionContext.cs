using System;
using System.Collections.Generic;

using QBX.ExecutionEngine.Compiled;
using QBX.ExecutionEngine.Compiled.Statements;
using QBX.ExecutionEngine.Execution.Variables;
using QBX.Firmware;
using QBX.Hardware;

namespace QBX.ExecutionEngine.Execution;

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

		int totalSlots = arguments.Length + variableTypes.Count;

		var variables = new Variable[totalSlots];

		arguments.CopyTo(variables);

		int index = arguments.Length;

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

		foreach (var type in variableTypes)
		{
			if (variables[index] == null)
			{
				if (type.IsArray)
					variables[index] = Variable.ConstructArray(type);
				else
					variables[index] = Variable.Construct(type);
			}

			index++;
		}

		return new StackFrame(variables);
	}
}
