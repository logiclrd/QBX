using System.Collections.Generic;
using System.Reflection;

using QBX.ExecutionEngine;
using QBX.ExecutionEngine.Execution;

namespace QBX.QuickLibraries;

public abstract class QuickLibrary
{
	public ExecutionContext? ExecutionContext;

	public IReadOnlyList<NativeProcedure> Exports => _exports;

	List<NativeProcedure> _exports = new List<NativeProcedure>();

	protected QuickLibrary()
	{
		foreach (var method in GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly))
			if (method.GetCustomAttribute<ExportAttribute>() != null)
				_exports.Add(new NativeProcedure(this, method));
	}
}
