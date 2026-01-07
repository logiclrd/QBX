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

	public bool EnablePaletteRemapping = true;

	public StackFrame RootFrame;

	public StackFrame CurrentFrame;

	public readonly Stack<StackFrame> CallStack = new Stack<StackFrame>();

	public void Execute(IExecutable? executable, bool stepInto)
	{
		if (!stepInto)
			executable?.Execute(this, false);
		else
		{
			PushScope();

			CurrentFrame.NextStatement = executable;
			CurrentFrame.CurrentSequence = null;
		}
	}

	public void Run(RunMode mode)
	{
		while ((CurrentFrame.NextStatement != null) || (CurrentFrame.CurrentSequence != null))
		{
			while (CurrentFrame.NextStatement == null)
			{
				int statementIndex = CurrentFrame.NextStatementIndex++;

				if (statementIndex >= CurrentFrame.CurrentSequence!.Count)
				{
					// TODO: return values
					if (CallStack.Count == 0)
						return;

					PopFrame();

					if (mode == RunMode.StepOut)
						return;

					continue;
				}

				CurrentFrame.NextStatement = CurrentFrame.CurrentSequence[statementIndex];
			}

			var statement = CurrentFrame.NextStatement;

			CurrentFrame.NextStatement = null;

			statement.Execute(this, mode == RunMode.StepInto);

			if (mode != RunMode.Continuous)
				break;
		}
	}

	public ExecutionContext(Machine machine, Module mainModule)
	{
		if (mainModule.MainRoutine == null)
			throw new BadModelException("Module does not have a MainRoutine");

		Machine = machine;
		VisualLibrary = new TextLibrary(machine);

		CreateFrame(
			mainModule,
			mainModule.MainRoutine,
			System.Array.Empty<Variable>());

		RootFrame = CurrentFrame;
	}

	public int ExitCode
	{
		get => RootFrame.Variables[0].CoerceToInt();
		set => RootFrame.Variables[0].SetData(value);
	}

	public void PushFrame(Module module, Routine routine, DataType? returnType, Variable[] arguments, List<DataType> variableTypes)
	{
		CallStack.Push(CurrentFrame);

		CreateFrame(module, routine, arguments);
	}

	[MemberNotNull(nameof(CurrentFrame))]
	void CreateFrame(Module module, Routine routine, Variable[] arguments)
	{
		var variableTypes = routine.VariableTypes;
		var linkedVariables = routine.LinkedVariables;

		int totalSlots = arguments.Length + variableTypes.Count;

		var variables = new Variable[totalSlots];

		arguments.CopyTo(variables);

		int index = arguments.Length;

		foreach (var link in linkedVariables)
			variables[link.LocalIndex] = RootFrame.Variables[link.RootIndex];

		foreach (var type in variableTypes)
		{
			if (variables[index] == null)
				variables[index] = Variable.Construct(type);

			index++;
		}

		CurrentFrame = new StackFrame(
			module,
			routine,
			variables);

		CurrentFrame.NextStatement = routine.Statements.First();
		CurrentFrame.NextStatementIndex = 1;
	}

	public void PushScope()
	{
		CallStack.Push(CurrentFrame);

		CurrentFrame = CurrentFrame.Clone();
		CurrentFrame.CurrentSequence = null;
		CurrentFrame.NextStatement = null;
		CurrentFrame.NextStatementIndex = 0;
	}

	public void PopFrame()
	{
		CurrentFrame = CallStack.Pop();
	}
}
