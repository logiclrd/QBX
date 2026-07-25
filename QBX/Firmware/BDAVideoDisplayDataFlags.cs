using System;

namespace QBX.Firmware;

[Flags]
public enum BDAVideoDisplayDataFlags : byte
{
	None = 0,

	VGAActive = 0x01,
	GrayScaleEnabled = 0x02,
	MonochromeDisplayAttached = 0x04,
	PreservePaletteOnModeSwitch = 0x08,
	DisplaySwitchingEnabled = 0x40,
	ScanLinesMask = 0x90,
	ScanLines_350 = 0x00,
	ScanLines_400 = 0x10,
	ScanLines_200 = 0x80,
}
