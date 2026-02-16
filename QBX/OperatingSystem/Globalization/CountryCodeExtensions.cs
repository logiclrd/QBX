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

	static Dictionary<string, CountryCode> s_countryCodeByRegionName =
		typeof(CountryCode).GetFields(BindingFlags.Public | BindingFlags.Static)
		.Select(field =>
			(
				CountryCode: (CountryCode)field.GetValue(null)!,
				CultureNameAttribute: field.GetCustomAttribute<CultureNameAttribute>()!
			))
		.Select(field => (field.CountryCode, RegionName: field.CultureNameAttribute.CultureName.Split('-').Last()))
		.GroupBy(field => field.RegionName)
		.ToDictionary(grouping => grouping.Key, grouping => grouping.First().CountryCode, StringComparer.OrdinalIgnoreCase);

	public static CountryCode ToCountryCode(this CultureInfo culture)
	{
		if (culture.IetfLanguageTag == "ar")
			return CountryCode.AreaSouth;
		else if (culture.IetfLanguageTag == "fr-CA")
			return CountryCode.CanadianFrench;
		else
		{
			try
			{
				var region = new RegionInfo(culture.LCID);

				return s_countryCodeByRegionName[region.Name];
			}
			catch
			{
				return CountryCode.UnitedStates;
			}
		}
	}
}
