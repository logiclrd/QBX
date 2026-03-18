using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using QBX.ExecutionEngine.Compiled;
using QBX.Parser;

namespace QBX.ExecutionEngine;

public class Compilation
{
	public List<Module> Modules;
	public Dictionary<Identifier, CommonBlock> CommonBlocks;
	public TypeRepository TypeRepository;
	public Dictionary<Identifier, Routine> Subs;
	public Dictionary<Identifier, Routine> Functions;
	public List<NativeProcedure> NativeProcedures;
	public bool UseStaticArrays = true;
	public bool UseDirectMarshalling = true;

	public Routine? EntrypointRoutine;

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
		CommonBlocks = new Dictionary<Identifier, CommonBlock>();
		TypeRepository = new TypeRepository();
		Subs = new Dictionary<Identifier, Routine>();
		Functions = new Dictionary<Identifier, Routine>();
		NativeProcedures = new List<NativeProcedure>();
	}

	public bool ResolveUnresolvedCalls([NotNullWhen(false)] out Module? errorModule)
	{
		foreach (var module in Modules)
			if (!module.UnresolvedReferences.ResolveCalls())
			{
				errorModule = module;
				return false;
			}

		errorModule = null;
		return true;
	}

	public CommonBlock GetCommonBlock(Identifier name)
	{
		if (!CommonBlocks.TryGetValue(name, out var block))
			block = CommonBlocks[name] = new CommonBlock(name);

		return block;
	}

	public bool IsRegistered(Identifier name)
	{
		return
			Subs.ContainsKey(name) ||
			Functions.ContainsKey(name) ||
			NativeProcedures.Any(nativeProcedure => nativeProcedure.Name == name);
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
		NativeProcedures.Add(procedure);
	}

	public bool TryGetSub(Identifier name, [NotNullWhen(true)] out Routine? sub)
		=> Subs.TryGetValue(name, out sub);

	public bool TryGetFunction(Identifier name, [NotNullWhen(true)] out Routine? function)
		=> Functions.TryGetValue(Mapper.UnqualifyIdentifier(name), out function);

	public bool TryGetRoutine(Identifier name, [NotNullWhen(true)] out Routine? routine)
		=> TryGetSub(name, out routine) || TryGetFunction(name, out routine);

	public void SetDefaultEntrypoint()
	{
		EntrypointRoutine = Modules[0].MainRoutine;
	}

	public bool IsEmpty =>
		(EntrypointRoutine == null) ||
		(EntrypointRoutine.Statements.All(statement => !statement.CanBreak));
}

