using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

using QBX.ExecutionEngine.Execution.Events;
using QBX.Hardware;

namespace QBX.QuickLibraries;

public class QuickLibraryManager(Machine machine, EventHub eventHub)
{
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

	Dictionary<string, QuickLibrary> _libraries =
		new Dictionary<string, QuickLibrary>(StringComparer.OrdinalIgnoreCase);

	public bool TryGetQuickLibrary(string name, [NotNullWhen(true)] out QuickLibrary? qlb)
	{
		if (!s_libraryTypes.TryGetValue(name, out var type))
		{
			qlb = null;
			return false;
		}

		if (!_libraries.TryGetValue(name, out qlb))
		{
			qlb = (QuickLibrary)Activator.CreateInstance(type, machine, eventHub)!;

			_libraries.Add(name, qlb);
		}

		return true;
	}
}
