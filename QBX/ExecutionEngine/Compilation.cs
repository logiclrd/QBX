using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using QBX.ExecutionEngine.Compiled;

namespace QBX.ExecutionEngine;

public class Compilation
{
	public List<Module> Modules = new List<Module>();
	public TypeRepository TypeRepository = new TypeRepository();
	public UnresolvedReferences UnresolvedReferences = new UnresolvedReferences();
	public Dictionary<string, Routine> Subs = new Dictionary<string, Routine>();
	public Dictionary<string, Routine> Functions = new Dictionary<string, Routine>();

	public Routine? EntrypointRoutine;

	public IEnumerable<Routine> AllRegisteredRoutines => Subs.Values.Concat(Functions.Values);

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

	public void SetDefaultEntrypoint()
	{
		EntrypointRoutine = Modules[0].MainRoutine;
	}
}

