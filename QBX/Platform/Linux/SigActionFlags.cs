using System;

namespace QBX.Platform.Linux;

[Flags]
public enum SigActionFlags : uint
{
	SA_NOCLDSTOP  = 1,          // Don't send SIGCHLD when children stop.
	SA_NOCLDWAIT  = 2,          // Don't create zombie on child death.
	SA_SIGINFO    = 4,          // Invoke signal-catching function with three arguments instead of one.
	SA_ONSTACK    = 0x08000000, // Use signal stack by using `sa_restorer'.
	SA_RESTART    = 0x10000000, // Restart syscall on signal return.
	SA_INTERRUPT  = 0x20000000, // Historical no-op.
	SA_NODEFER    = 0x40000000, // Don't automatically block the signal when its handler is being executed.
	SA_RESETHAND  = 0x80000000, // Reset to SIG_DFL on entry to handler.

	// Some aliases for the SA_ constants.
	SA_NOMASK    = SA_NODEFER,
	SA_ONESHOT   = SA_RESETHAND,
	SA_STACK     = SA_ONSTACK,
}
