using System;

namespace QBX.Firmware;

[Flags]
public enum BDAEquipmentFlags2 : byte
{
	None = 0,

	DMAControllerPresent = 0x01,
	SerialPortCountMask = 0x0E,
	GameAdapterInstalled = 0x10,
	InternalModemInstalled = 0x20,
	PrinterPortCountMask = 0xC0,
}
