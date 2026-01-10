using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

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
			Dispatch(entrypoint, _rootFrame);

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

	public void Dispatch(Executable? executable, StackFrame stackFrame)
	{
		if (executable != null)
		{
			if (executable.CanBreak)
				_state.NextStatement(executable.Source);

			executable.Execute(this, stackFrame);
		}
	}

	public void Dispatch(Sequence? sequence, StackFrame stackFrame)
	{
		if (sequence != null)
			for (int i = 0; i < sequence.Count; i++)
				Dispatch(sequence[i], stackFrame);
	}

	static Variable s_dummyVariable = new DummyVariable();

	public Variable Call(Routine routine, Variable[] arguments)
	{
		var frame = CreateFrame(routine.Module, routine, arguments);

		_state.EnterRoutine(routine, frame);

		try
		{
			Dispatch(routine, frame);

			if (routine.ReturnType != null)
				return frame.Variables[0];
			else
				return s_dummyVariable;
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
