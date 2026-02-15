namespace QBX.OperatingSystem;

public enum DOSErrorAction
{
	None = 0x00,

	Retry = 0x01,
	DelayAndRetry = 0x02,
	PromptUserForNewInput = 0x03,
	Abort = 0x04,
	Panic = 0x05,
	Ignore = 0x06,
	IntercedeAndRetry = 0x07,
}
