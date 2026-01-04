using QBX.ExecutionEngine.Compiled;

using System.Diagnostics.CodeAnalysis;

namespace QBX.ExecutionEngine.Execution;

public class ExecutionContext
{
	public Variable[] GlobalVariables;
	public StackFrame RootFrame;

	public StackFrame CurrentFrame;

	// TODO: SHARED
	// [x] variables DIM SHARED get pushed down into every frame
	//     => consuming code must track this and supply appropriate GlobalVariables information
	// [ ] the creation of a frame needs to be able to pull shared variables down

	public readonly Stack<StackFrame> CallStack = new Stack<StackFrame>();

	public void Run(RunMode mode)
	{
		while (true)
		{
			while (CurrentFrame.NextStatement == null)
			{
				CurrentFrame.NextStatementIndex++;

				if (CurrentFrame.NextStatementIndex >= CurrentFrame.CurrentSequence.Count)
				{
					// TODO: return values
					if (CallStack.Count == 0)
						return;

					PopFrame();

					if (mode == RunMode.StepOut)
						return;
				}

				CurrentFrame.NextStatement = CurrentFrame.CurrentSequence[CurrentFrame.NextStatementIndex];
			}

			var statement = CurrentFrame.NextStatement;

			CurrentFrame.NextStatement = null;

			statement.Execute(this, mode == RunMode.StepInto);

			if (mode != RunMode.Continuous)
				break;
		}
	}

	public ExecutionContext(Module mainModule, IEnumerable<DataType> variableTypes, IEnumerable<DataType> globalVariableTypes)
	{
		if (mainModule.MainRoutine == null)
			throw new Exception("Module does not have a MainRoutine");

		GlobalVariables = globalVariableTypes
			.Select(type => new Variable(type))
			.ToArray();

		CreateFrame(
			mainModule,
			mainModule.MainRoutine,
			DataType.Long,
			System.Array.Empty<Variable>(),
			variableTypes);

		RootFrame = CurrentFrame;
	}

	public int ExitCode
	{
		get => RootFrame.Variables[0].CoerceToInt();
		set => RootFrame.Variables[0].Data = value;
	}

	public void PushFrame(Module module, Routine routine, DataType? returnType, Variable[] arguments, List<DataType> variableTypes)
	{
		CallStack.Push(CurrentFrame);

		CreateFrame(module, routine, returnType, arguments, variableTypes);
	}

	[MemberNotNull(nameof(CurrentFrame))]
	void CreateFrame(Module module, Routine routine, DataType? returnType, Variable[] arguments, IEnumerable<DataType> variableTypes)
	{
		var variableTypesList = variableTypes.ToList();

		int totalSlots = 1 + arguments.Length + variableTypesList.Count;

		var variables = new Variable[totalSlots];

		variables[0] = new Variable(returnType ?? DataType.Integer);

		arguments.CopyTo(variables, 1);

		int index = variables.Length + 1;

		foreach (var type in variableTypesList)
			variables[index++] = new Variable(type);

		CurrentFrame = new StackFrame(
			module,
			routine,
			variables);

		CurrentFrame.NextStatement = routine.Statements.First();
		CurrentFrame.NextStatementIndex = 0;
	}

	public void PopFrame()
	{
		CurrentFrame = CallStack.Pop();
	}
}
