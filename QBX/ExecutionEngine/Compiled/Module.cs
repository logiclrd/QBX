using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using QBX.ExecutionEngine.Execution;

namespace QBX.ExecutionEngine.Compiled;

public class Module
{
	public Routine? MainRoutine;
	public Dictionary<string, Routine> Routines = new Dictionary<string, Routine>();
	public DataParser DataParser = new DataParser();
	public UnresolvedReferences UnresolvedReferences;

	public Dictionary<string, NativeProcedure> NativeProcedures;

	public Module(Compilation compilation)
	{
		UnresolvedReferences = new UnresolvedReferences(compilation, this);

		NativeProcedures = compilation.NativeProcedures.ToDictionary(
			key => key.Name,
			element => element.Clone(),
			StringComparer.OrdinalIgnoreCase);
	}

	public bool IsRegistered(string identifier)
		=> Routines.ContainsKey(identifier) || NativeProcedures.ContainsKey(identifier);

	public bool TryGetNativeProcedure(string name, [NotNullWhen(true)] out NativeProcedure? procedure)
		=> NativeProcedures.TryGetValue(name, out procedure);
}
