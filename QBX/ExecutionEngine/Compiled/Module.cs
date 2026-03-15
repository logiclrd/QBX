using System;
using System.Collections.Generic;

using QBX.ExecutionEngine.Execution;

namespace QBX.ExecutionEngine.Compiled;

public class Module
{
	public Routine? MainRoutine;
	public Dictionary<string, Routine> Routines = new Dictionary<string, Routine>();
	public DataParser DataParser = new DataParser();
	public UnresolvedReferences UnresolvedReferences;

	public Module(Compilation compilation)
	{
		UnresolvedReferences = new UnresolvedReferences(compilation, this);
	}

	public bool IsRegistered(string identifier)
		=> Routines.ContainsKey(identifier);
}
