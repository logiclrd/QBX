using System;

namespace QBX.Platform.Linux;

[Flags]
public enum waitoptions_t
{
	None = 0,

	WNOHANG = 1, // non-blocking
	WUNTRACED = 2, // also return on child SIGSTOP
	WCONTINUED = 8, // also return on child SIGCONT
}
