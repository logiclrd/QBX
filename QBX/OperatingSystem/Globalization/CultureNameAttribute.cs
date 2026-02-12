using System;

namespace QBX.OperatingSystem.Globalization;

[AttributeUsage(AttributeTargets.Field)]
public class CultureNameAttribute(string cultureName) : Attribute
{
	public string CultureName => cultureName;
}
