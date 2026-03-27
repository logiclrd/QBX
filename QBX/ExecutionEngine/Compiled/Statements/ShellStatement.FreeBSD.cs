using System;
using System.Runtime.Versioning;

using QBX.Utility;

using ExecutionContext = QBX.ExecutionEngine.Execution.ExecutionContext;

namespace QBX.ExecutionEngine.Compiled.Statements;

public partial class ShellStatement : Executable
{
	[SupportedOSPlatform(PlatformNames.FreeBSD)]
	[SupportedOSPlatform(PlatformNames.MacOS)]
	public void RunChildProcessFreeBSD(ExecutionContext context, string fileName, string arguments)
	{
		throw new NotImplementedException();
	}
}

