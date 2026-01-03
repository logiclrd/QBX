using QBX.ExecutionEngine.Compiled;
using System;
using System.Diagnostics.CodeAnalysis;

namespace QBX.ExecutionEngine.Execution;

public class ExecutionContext
{
	public Variable[] Variables;
	public Variable[] GlobalVariables;
	public Variable[] RootFrame;

	// TODO: SHARED
	// [x] variables DIM SHARED get pushed down into every frame
	//     => consuming code must track this and supply appropriate GlobalVariables information
	// [ ] the creation of a frame needs to be able to pull shared variables down

	public readonly Stack<Variable[]> CallStack = new Stack<Variable[]>();

	public ExecutionContext(IEnumerable<DataType> variableTypes, IEnumerable<DataType> globalVariableTypes)
	{
		GlobalVariables = globalVariableTypes
			.Select(type => new Variable(type))
			.ToArray();

		CreateFrame(DataType.Long, System.Array.Empty<Variable>(), variableTypes);

		RootFrame = Variables;
	}

	public int ExitCode
	{
		get => RootFrame[0].CoerceToInt();
		set => RootFrame[0].Data = value;
	}

	public void PushFrame(DataType? returnType, Variable[] arguments, List<DataType> variableTypes)
	{
		CallStack.Push(Variables);

		CreateFrame(returnType, arguments, variableTypes);
	}

	[MemberNotNull(nameof(Variables))]
	void CreateFrame(DataType? returnType, Variable[] arguments, IEnumerable<DataType> variableTypes)
	{
		var variableTypesList = variableTypes.ToList();

		int totalSlots = 1 + arguments.Length + variableTypesList.Count;

		Variables = new Variable[totalSlots];

		Variables[0] = new Variable(returnType ?? DataType.Integer);

		arguments.CopyTo(Variables, 1);

		int index = Variables.Length + 1;

		foreach (var type in variableTypesList)
			Variables[index++] = new Variable(type);
	}

	public void PopFrame()
	{
		Variables = CallStack.Pop();
	}
}
