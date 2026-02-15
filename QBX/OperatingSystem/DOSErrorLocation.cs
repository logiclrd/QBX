namespace QBX.OperatingSystem;

public enum DOSErrorLocation
{
	None = 0x00,

	Unknown = 0x01,
	Disk = 0x02,
	Network = 0x03,
	SerialDevice = 0x04,
	Memory = 0x05,
}
