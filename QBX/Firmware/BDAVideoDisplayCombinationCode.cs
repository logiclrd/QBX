namespace QBX.Firmware;

public enum BDAVideoDisplayCombinationCode : byte
{
	NoDisplayAdapter = 0,
	MDA_MonochromeDisplay = 1,
	CGA_ColourDisplay = 2,
	EGA_ColourDisplay = 4,
	EGA_MonochromeDisplay = 5,
	VGA_MonochromeDisplay = 7,
	VGA_ColourDisplay = 8,
}
