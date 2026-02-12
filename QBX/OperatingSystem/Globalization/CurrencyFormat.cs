using System;

namespace QBX.OperatingSystem.Globalization;

[Flags]
public enum CurrencyFormat : byte
{
	CurrencySymbolAfter = 1,
	SpaceBetweenAmountAndSymbol = 2,
}
