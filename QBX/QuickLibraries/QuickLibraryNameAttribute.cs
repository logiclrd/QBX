using System;

namespace QBX.QuickLibraries;

[AttributeUsage(AttributeTargets.Class)]
public class QuickLibraryNameAttribute(string name) : Attribute
{
	public string Name => name;
}
