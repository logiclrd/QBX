using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using QBX.ExecutionEngine.Compiled;

namespace QBX.ExecutionEngine;

public class UnresolvedReferences(Compilation compilation)
{
	// TODO: track uses of these SUBs/FUNCTIONs so they can be fixed up once
	//       all modules have been compiled
	public Dictionary<string, ForwardReferenceList> ForwardReferences =
		new Dictionary<string, ForwardReferenceList>(StringComparer.OrdinalIgnoreCase);

	public bool TryGetDeclaration(string identifier, [NotNullWhen(true)] out ForwardReferenceList? forwardReference)
		=> ForwardReferences.TryGetValue(identifier, out forwardReference);

	public void DeclareSymbol(string identifier, Mapper mapper, CodeModel.Statements.Statement? statement, RoutineType routineType, DataType[] parameterTypes, DataType? returnType)
	{
		if (ForwardReferences.ContainsKey(identifier)
		 || compilation.IsRegistered(identifier))
			throw CompilerException.DuplicateDefinition(statement);

		ForwardReferences[identifier] = new ForwardReferenceList(
			identifier,
			routineType,
			parameterTypes,
			returnType);
	}

	public bool ResolveCalls()
	{
		var resolvedIdentifiers = new List<string>();

		try
		{
			foreach (var forwardReference in ForwardReferences.Values)
			{
				if (compilation.TryGetRoutine(forwardReference.Identifier, out var routine))
				{
					while (forwardReference.UnresolvedCalls.Count > 0)
						forwardReference.UnresolvedCalls.Last().Resolve(routine);

					resolvedIdentifiers.Add(forwardReference.Identifier);
				}
			}
		}
		finally
		{
			foreach (var identifier in resolvedIdentifiers)
				ForwardReferences.Remove(identifier);
		}

		return (ForwardReferences.Count == 0);
	}
}
