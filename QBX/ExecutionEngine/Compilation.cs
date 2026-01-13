using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using QBX.ExecutionEngine.Compiled;

namespace QBX.ExecutionEngine;

public class Compilation
{
	public List<Module> Modules;
	public TypeRepository TypeRepository;
	public UnresolvedReferences UnresolvedReferences;
	public Dictionary<string, Routine> Subs;
	public Dictionary<string, Routine> Functions;

	public Routine? EntrypointRoutine;

	public IEnumerable<Routine> AllRegisteredRoutines => Subs.Values.Concat(Functions.Values);

	public Compilation()
	{
		Modules = new List<Module>();
		TypeRepository = new TypeRepository();
		UnresolvedReferences = new UnresolvedReferences(this);
		Subs = new Dictionary<string, Routine>();
		Functions = new Dictionary<string, Routine>();
	}

	public bool IsRegistered(string name)
	{
		return Subs.ContainsKey(name) || Functions.ContainsKey(name);
	}

	public void RegisterSub(Routine routine)
	{
		Subs[routine.Name] = routine;
	}

	public void RegisterFunction(Routine routine)
	{
		Functions[routine.Name] = routine;
	}

	public bool TryGetSub(string name, [NotNullWhen(true)] out Routine? sub)
		=> Subs.TryGetValue(name, out sub);

	public bool TryGetFunction(string name, [NotNullWhen(true)] out Routine? function)
		=> Functions.TryGetValue(name, out function);

	public bool TryGetRoutine(string name, [NotNullWhen(true)] out Routine? routine)
		=> TryGetSub(name, out routine) || TryGetFunction(name, out routine);

	public void SetDefaultEntrypoint()
	{
		EntrypointRoutine = Modules[0].MainRoutine;
	}
}

