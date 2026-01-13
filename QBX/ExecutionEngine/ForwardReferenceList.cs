using System.Collections.Generic;

namespace QBX.ExecutionEngine;

public class ForwardReferenceList(string identifier, RoutineType routineType)
{
	public readonly string Identifier = identifier;
	public readonly RoutineType RoutineType = routineType;
	public List<IUnresolvedCall> UnresolvedCalls = new List<IUnresolvedCall>();
}
