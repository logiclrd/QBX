namespace QBX.Platform.Linux;

public enum ProcMaskHow
{
	SIG_BLOCK, /* for blocking signals */
	SIG_UNBLOCK, /* for unblocking signals */
	SIG_SETMASK, /* for setting the signal mask */
}
