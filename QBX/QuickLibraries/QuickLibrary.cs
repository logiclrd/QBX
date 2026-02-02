using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

using QBX.ExecutionEngine;
using QBX.Hardware;

namespace QBX.QuickLibraries;

public abstract class QuickLibrary
{
	public static bool TryGetQuickLibrary(string name, Machine machine, [NotNullWhen(true)] out QuickLibrary? qlb)
	{
		if (!s_libraryTypes.TryGetValue(name, out var type))
		{
			qlb = null;
			return false;
		}

		qlb = (QuickLibrary)Activator.CreateInstance(type, machine)!;

		s_libraries.Add(name, qlb);

		return true;
	}

	static Dictionary<string, Type> s_libraryTypes =
		typeof(QuickLibrary).Assembly.GetTypes()
		.Where(type => typeof(QuickLibrary).IsAssignableFrom(type))
		.Select(type =>
			(
				Type: type,
				NameAttribute: type.GetCustomAttribute<QuickLibraryNameAttribute>()
			))
		.Where(library => library.NameAttribute != null)
		.ToDictionary(key => key.NameAttribute!.Name, value => value.Type, StringComparer.OrdinalIgnoreCase);

	static Dictionary<string, QuickLibrary> s_libraries =
		new Dictionary<string, QuickLibrary>(StringComparer.OrdinalIgnoreCase);

	public IReadOnlyList<NativeProcedure> Exports => _exports;

	List<NativeProcedure> _exports = new List<NativeProcedure>();

	protected QuickLibrary()
	{
		foreach (var method in GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly))
			if (method.GetCustomAttribute<ExportAttribute>() != null)
				_exports.Add(new NativeProcedure(this, method));
	}
}
