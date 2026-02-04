using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using QBX.ExecutionEngine.Compiled;
using QBX.ExecutionEngine.Execution;

namespace QBX.ExecutionEngine;

public class Compilation
{
	public List<Module> Modules;
	public TypeRepository TypeRepository;
	public UnresolvedReferences UnresolvedReferences;
	public Dictionary<string, Routine> Subs;
	public Dictionary<string, Routine> Functions;
	public Dictionary<string, NativeProcedure> NativeProcedures;
	public bool UseStaticArrays = true;
	public bool UseDirectMarshalling = true;

	public Routine? EntrypointRoutine;

	// TODO: COMMAND$ function
	public StringValue CommandLine = new StringValue();

	public IEnumerable<Routine> AllRegisteredRoutines
	{
		get
		{
			foreach (var module in Modules)
				if (module.MainRoutine != null)
					yield return module.MainRoutine;

			foreach (var sub in Subs.Values)
				yield return sub;

			foreach (var function in Functions.Values)
				yield return function;
		}
	}

	public Compilation()
	{
		Modules = new List<Module>();
		TypeRepository = new TypeRepository();
		UnresolvedReferences = new UnresolvedReferences(this);
		Subs = new Dictionary<string, Routine>(StringComparer.OrdinalIgnoreCase);
		Functions = new Dictionary<string, Routine>(StringComparer.OrdinalIgnoreCase);
		NativeProcedures = new Dictionary<string, NativeProcedure>(StringComparer.OrdinalIgnoreCase);
	}

	public bool IsRegistered(string name)
	{
		return
			Subs.ContainsKey(name) ||
			Functions.ContainsKey(name) ||
			NativeProcedures.ContainsKey(name);
	}

	public void RegisterSub(Routine routine)
	{
		Subs[routine.Name] = routine;
	}

	public void RegisterFunction(Routine routine)
	{
		Functions[Mapper.UnqualifyIdentifier(routine.Name)] = routine;
	}

	public void RegisterNativeProcedure(NativeProcedure procedure)
	{
		NativeProcedures[procedure.Name] = procedure;
	}

	public bool TryGetSub(string name, [NotNullWhen(true)] out Routine? sub)
		=> Subs.TryGetValue(name, out sub);

	public bool TryGetFunction(string name, [NotNullWhen(true)] out Routine? function)
		=> Functions.TryGetValue(Mapper.UnqualifyIdentifier(name), out function);

	public bool TryGetRoutine(string name, [NotNullWhen(true)] out Routine? routine)
		=> TryGetSub(name, out routine) || TryGetFunction(name, out routine);

	public bool TryGetNativeProcedure(string name, [NotNullWhen(true)] out NativeProcedure? procedure)
		=> NativeProcedures.TryGetValue(name, out procedure);

	public void SetDefaultEntrypoint()
	{
		EntrypointRoutine = Modules[0].MainRoutine;
	}
}

