using System.Collections.Generic;

using QBX.ExecutionEngine.Compiled;

namespace QBX.ExecutionEngine;

public class ForwardReferenceList(string identifier, RoutineType routineType, IReadOnlyList<DataType> parameterTypes, DataType? returnType)
{
	public readonly string Identifier = identifier;
	public readonly RoutineType RoutineType = routineType;
	public readonly IReadOnlyList<DataType> ParameterTypes = parameterTypes;
	public readonly DataType? ReturnType = returnType;
	public List<IUnresolvedCall> UnresolvedCalls = new List<IUnresolvedCall>();
}
