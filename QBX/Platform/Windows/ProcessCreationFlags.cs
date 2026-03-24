namespace QBX.Platform.Windows;

public enum ProcessCreationFlags
{
	DebugProcess = 0x00000001,
	DebugOnlyThisProcess = 0x00000002,
	Suspended = 0x00000004,
	DetachedProcess = 0x00000008,
	NewConsole = 0x00000010,
	NewProcessGroup = 0x00000200,
	UnicodeEnvironment = 0x00000400,
	SeparateWOWVDM = 0x00000800,
	SharedWOWVDM = 0x00001000,
	InheritParentAffinity = 0x00010000,
	ProtectedProcess = 0x00040000,
	ExtendedStartupInfoPresent = 0x00080000,
	SecureProcess = 0x00400000,
	BreakAwayFromJob = 0x01000000,
	PreserveCodeAuthorizationLevel = 0x02000000,
	DefaultErrorMode = 0x04000000,
	NoWindow = 0x08000000,
}
