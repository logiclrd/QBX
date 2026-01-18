using System.Collections.Generic;

using QBX.ExecutionEngine.Compiled;

namespace QBX.ExecutionEngine;

public class ForwardReferenceList(string identifier, RoutineType routineType, DataType returnType)
{
	public readonly string Identifier = identifier;
	public readonly RoutineType RoutineType = routineType;
	public readonly DataType ReturnType = returnType;
	public List<IUnresolvedCall> UnresolvedCalls = new List<IUnresolvedCall>();
}
