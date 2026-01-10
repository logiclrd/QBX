using System;
using System.Collections.Generic;

using QBX.ExecutionEngine.Compiled;
using QBX.ExecutionEngine.Execution.Variables;
using QBX.Firmware;
using QBX.Hardware;

namespace QBX.ExecutionEngine.Execution;

public class ExecutionContext
{
	public Machine Machine;
	public VisualLibrary VisualLibrary;

	public IReadOnlyExecutionState ExecutionState => _state;
	public IExecutionControls Controls => _state;

	ExecutionState _state;

	StackFrame? _rootFrame;
	StatementPath? _goTo;

	public bool EnablePaletteRemapping = true;

	public ExecutionContext(Machine machine)
	{
		_state = new ExecutionState();

		Machine = machine;
		VisualLibrary = new TextLibrary(machine);
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

		_state.StartExecution(_rootFrame);

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

			_state.EndExecution();
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

				var subsequence = executable.GetSequenceByIndex(subsequenceIndex);

				if (subsequence == null)
					throw new Exception("Internal Error: ExecutionPath specified subsequence " + subsequenceIndex + " within a " + executable.GetType().Name + " and it does not exist");

				Dispatch(subsequence, stackFrame);
			}
			else
			{
				if (executable.CanBreak)
					_state.NextStatement(executable.Source);

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
		var frame = CreateFrame(routine.Module, routine, arguments);

		return Call(routine, frame);
	}

	Variable Call(Routine routine, StackFrame frame)
	{
		_state.EnterRoutine(routine, frame);

		try
		{
		goTo_:
			try
			{
				Dispatch(routine, frame);

				if (routine.ReturnType != null)
					return frame.Variables[0];
				else
					return s_dummyVariable;
			}
			catch (GoTo goTo)
			{
				_goTo = goTo.StatementPath.Clone();
				goto goTo_;
			}
		}
		finally
		{
			_state.ExitRoutine();
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
