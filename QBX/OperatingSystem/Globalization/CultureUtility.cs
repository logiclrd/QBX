using System.Globalization;

namespace QBX.OperatingSystem.Globalization;

public class CultureUtility
{
	public static CultureInfo? GetCultureInfoForCodePageAndCountry(int codePage, CountryCode country)
	{
		foreach (var cultureInfo in CultureInfo.GetCultures(CultureTypes.AllCultures))
		{
			if ((cultureInfo.TextInfo.OEMCodePage == codePage)
			 && (cultureInfo.ToCountryCode() == country))
				return cultureInfo;
		}

		return null;
	}
}
