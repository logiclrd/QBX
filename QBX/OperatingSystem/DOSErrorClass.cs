namespace QBX.OperatingSystem;

public enum DOSErrorClass
{
	None = 0x00,

	OutOfResource = 0x01,
	TemporarySituation = 0x02,
	AuthorizationProblem = 0x03,
	InternalError = 0x04,
	HardwareFailure = 0x05,
	SystemFailure = 0x06,
	ApplicationError = 0x07,
	NotFound = 0x08,
	BadFormat = 0x09,
	Locked = 0x0A,
	Media = 0x0B,
	Already = 0x0C,
	Unknown = 0x0D,
}
