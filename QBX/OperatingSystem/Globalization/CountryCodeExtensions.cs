using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace QBX.OperatingSystem.Globalization;

public static class CountryCodeExtensions
{
	static Dictionary<CountryCode, string> s_cultureNameByCountryCode =
		typeof(CountryCode).GetFields(BindingFlags.Static | BindingFlags.Public)
		.Select(field => (Value: (CountryCode)field.GetValue(null)!, CultureNameAttribute: field.GetCustomAttribute<CultureNameAttribute>()))
		.Where(item => item.CultureNameAttribute != null)
		.ToDictionary(
			key => key.Value,
			value => value.CultureNameAttribute!.CultureName);

	public static string ToCultureName(this CountryCode code)
	{
		if (!s_cultureNameByCountryCode.TryGetValue(code, out var cultureName))
			cultureName = "en-US";

		return cultureName;
	}
}
