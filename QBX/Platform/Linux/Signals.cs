namespace QBX.Platform.Linux;

public static class Signals
{
	public const int NSIG = 32;

	public const int SIGHUP          =  1;
	public const int SIGINT          =  2;
	public const int SIGQUIT         =  3;
	public const int SIGILL          =  4;
	public const int SIGTRAP         =  5;
	public const int SIGABRT         =  6;
	public const int SIGIOT          =  6;
	public const int SIGBUS          =  7;
	public const int SIGFPE          =  8;
	public const int SIGKILL         =  9;
	public const int SIGUSR1         = 10;
	public const int SIGSEGV         = 11;
	public const int SIGUSR2         = 12;
	public const int SIGPIPE         = 13;
	public const int SIGALRM         = 14;
	public const int SIGTERM         = 15;
	public const int SIGSTKFLT       = 16;
	public const int SIGCHLD         = 17;
	public const int SIGCONT         = 18;
	public const int SIGSTOP         = 19;
	public const int SIGTSTP         = 20;
	public const int SIGTTIN         = 21;
	public const int SIGTTOU         = 22;
	public const int SIGURG          = 23;
	public const int SIGXCPU         = 24;
	public const int SIGXFSZ         = 25;
	public const int SIGVTALRM       = 26;
	public const int SIGPROF         = 27;
	public const int SIGWINCH        = 28;
	public const int SIGIO           = 29;
	public const int SIGPOLL         = SIGIO;
	public const int SIGPWR          = 30;
	public const int SIGSYS          = 31;
	public const int SIGUNUSED       = 31;
}
