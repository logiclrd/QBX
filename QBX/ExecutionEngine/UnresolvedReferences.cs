using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using QBX.ExecutionEngine.Compiled;
using QBX.LexicalAnalysis;
using QBX.Parser;

namespace QBX.ExecutionEngine;

public class UnresolvedReferences(Compilation compilation, Module module)
{
	public Dictionary<Identifier, ForwardReferenceList> ForwardReferences = new();

	public bool TryGetDeclaration(Identifier identifier, [NotNullWhen(true)] out ForwardReferenceList? forwardReference)
		=> ForwardReferences.TryGetValue(identifier, out forwardReference);

	public ForwardReferenceList DeclareSymbol(Identifier identifier, Mapper mapper, CodeModel.Statements.Statement? statement, RoutineType routineType, DataType[] parameterTypes, DataType? returnType)
	{
		if (ForwardReferences.ContainsKey(identifier)
		 || module.IsRegistered(identifier))
			throw CompilerException.DuplicateDefinition(statement);

		var forwardReference = new ForwardReferenceList(
			identifier,
			routineType,
			parameterTypes,
			returnType);

		ForwardReferences[identifier] = forwardReference;

		return forwardReference;
	}

	public bool ResolveCalls()
	{
		var resolvedIdentifiers = new List<Identifier>();

		try
		{
			foreach (var forwardReference in ForwardReferences.Values)
			{
				if (compilation.TryGetRoutine(forwardReference.Identifier, out var routine))
				{
					while (forwardReference.UnresolvedCalls.Count > 0)
					{
						forwardReference.UnresolvedCalls.Last().Resolve(routine);
						forwardReference.UnresolvedCalls.RemoveAt(forwardReference.UnresolvedCalls.Count - 1);
					}
				}

				if (forwardReference.UnresolvedCalls.Count == 0)
					resolvedIdentifiers.Add(forwardReference.Identifier);
			}
		}
		finally
		{
			foreach (var identifier in resolvedIdentifiers)
				ForwardReferences.Remove(identifier);
		}

		return (ForwardReferences.Count == 0);
	}

	public Token? GetFirstUnresolvedStatementSourceToken()
	{
		foreach (var forwardReference in ForwardReferences.Values)
			if (forwardReference.UnresolvedCalls.Any())
				return forwardReference.UnresolvedCalls[0].SourceToken;

		return null;
	}
}
