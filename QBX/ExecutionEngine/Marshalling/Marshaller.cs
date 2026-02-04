using System;
using System.Collections.Generic;
using System.Reflection;

namespace QBX.ExecutionEngine.Marshalling;

public abstract class Marshaller
{
	public abstract void Map(object from, ref object? to);

	protected static IEnumerable<MemberInfo> EnumerateDataMembers(Type type)
	{
		// Enumerate inherited members first
		if (type.BaseType != null)
		{
			foreach (var inheritedMember in EnumerateDataMembers(type.BaseType))
				yield return inheritedMember;
		}

		// Then enumerate all public instance fields and properties in this type directly.
		foreach (var member in type.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
		{
			switch (member.MemberType)
			{
				case MemberTypes.Field:
				case MemberTypes.Property:
					yield return member;
					break;
			}
		}
	}
}

