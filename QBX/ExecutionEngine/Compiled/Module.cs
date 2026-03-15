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

	// SUBs and FUNCTIONs:
	//   In each module, each SUB/FUNCTION can have its own declared signature.
	//   These must line up in order to be compatible, but the names of
	//   parameters and any user-defined type facades can vary.
	public Dictionary<string, RoutineFacade> SubFacades;
	public Dictionary<string, RoutineFacade> FunctionFacades;
	public Dictionary<string, NativeProcedure> NativeProcedures;

	public Module(Compilation compilation)
	{
		UnresolvedReferences = new UnresolvedReferences(compilation, this);

		SubFacades = new Dictionary<string, RoutineFacade>(StringComparer.OrdinalIgnoreCase);
		FunctionFacades = new Dictionary<string, RoutineFacade>(StringComparer.OrdinalIgnoreCase);

		NativeProcedures = compilation.NativeProcedures.ToDictionary(
			key => key.Name,
			element => element.Clone(),
			StringComparer.OrdinalIgnoreCase);
	}

	public bool IsRegistered(string identifier)
		=> Routines.ContainsKey(identifier) || NativeProcedures.ContainsKey(identifier);

	public void AddSubFacade(string name, RoutineFacade facade)
		=> SubFacades.Add(name, facade);
	public void AddFunctionFacade(string name, RoutineFacade facade)
		=> FunctionFacades.Add(name, facade);

	public bool TryGetSubFacade(string name, [NotNullWhen(true)] out RoutineFacade? facade)
		=> SubFacades.TryGetValue(name, out facade);
	public bool TryGetFunctionFacade(string name, [NotNullWhen(true)] out RoutineFacade? facade)
		=> FunctionFacades.TryGetValue(name, out facade);

	public bool TryGetNativeProcedure(string name, [NotNullWhen(true)] out NativeProcedure? procedure)
		=> NativeProcedures.TryGetValue(name, out procedure);
}
