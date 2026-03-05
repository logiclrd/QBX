using System;
using System.Collections.Generic;
using System.Globalization;
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

	static Dictionary<string, Dictionary<string, CountryCode>> s_countryCodeByLanguageTagByRegionName =
		typeof(CountryCode).GetFields(BindingFlags.Public | BindingFlags.Static)
		.Select(field =>
			(
				CountryCode: (CountryCode)field.GetValue(null)!,
				CultureNameAttribute: field.GetCustomAttribute<CultureNameAttribute>()!
			))
		.Select(
			field =>
			(
				field.CountryCode,
				LanguageTag: field.CultureNameAttribute.CultureName.Split('-').First(),
				RegionName: field.CultureNameAttribute.CultureName.Split('-').Last()
			))
		.GroupBy(field => field.RegionName, StringComparer.OrdinalIgnoreCase)
		.ToDictionary(
			grouping => grouping.Key,
			grouping => grouping.ToDictionary(set => set.LanguageTag, set => set.CountryCode, StringComparer.OrdinalIgnoreCase),
			StringComparer.OrdinalIgnoreCase);

	public static CountryCode ToCountryCode(this CultureInfo culture)
	{
		if (culture.IetfLanguageTag == "ar")
			return CountryCode.AreaSouth;
		else if (culture.IetfLanguageTag == "fr-CA")
			return CountryCode.CanadianFrench;
		else if (!culture.IsNeutralCulture)
		{
			try
			{
				var region = new RegionInfo(culture.LCID);

				if (s_countryCodeByLanguageTagByRegionName.TryGetValue(region.Name, out var countryCodesByLanguageTag))
				{
					if (countryCodesByLanguageTag.TryGetValue(culture.TwoLetterISOLanguageName, out var countryCodeForLanguage))
						return countryCodeForLanguage;
					if (countryCodesByLanguageTag.TryGetValue("en", out var countryCodeForEnglish))
						return countryCodeForEnglish;
				}
			}
			catch { }
		}

		return CountryCode.UnitedStates;
	}
}
