using System;

namespace QBX.Firmware;

[Flags]
public enum BDAVideoModeOptions : byte
{
	AlphanumericCursorEmulationEnabled = 0x01,
	MonochromeDisplayAttached = 0x02,
	Inactive = 0x04,
	RAMSizeMask = 0x60,
	RAMSize_64KB = 0x00,
	RAMSize_128KB = 0x20,
	RAMSize_192KB = 0x40,
	RAMSize_256KB = 0x60,
	PreserveVRAMOnModeSwitch = 0x80,
}
