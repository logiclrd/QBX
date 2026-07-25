using System;

namespace QBX.Firmware;

[Flags]
public enum BDAEquipmentFlags1 : byte
{
	None = 0,

	InitialProgramLoadDiskPresent = 0x01,
	MathCoprocessorPresent = 0x02,
	MouseInstalled = 0x04,
	InitialVideoModeMask = 0b11_0000,
	InitialVideoMode_EGAOrVGA = 0b00_0000,
	InitialVideoMode_CGA40x25 = 0b01_0000,
	InitialVideoMode_CGA80x25 = 0b10_0000,
	InitialVideoMode_MGA80x25 = 0b11_0000,
	DiskDriveCountMask = 0xC0,
}
