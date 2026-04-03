using System.Globalization;

namespace QBX.CodeModel;

public class BasicCulture : CultureInfo
{
	public BasicCulture()
		: base("")
	{
		NumberFormat.CurrencySymbol = "$";
	}

	public static BasicCulture Instance { get; } = new BasicCulture();
}
