using System.Runtime.Versioning;

using DQD.ForkPTY;

using QBX.ExecutionEngine.Compiled.Statements.Shell.Unix;
using QBX.Utility;

using ExecutionContext = QBX.ExecutionEngine.Execution.ExecutionContext;

namespace QBX.ExecutionEngine.Compiled.Statements;

public partial class ShellStatement : Executable
{
	[UnsupportedOSPlatform(PlatformNames.Windows)]
	public void RunChildProcessUnix(ExecutionContext context, string fileName, string[] arguments)
	{
		var ptyConfiguration = new PTYConfiguration();

		ptyConfiguration.CharacterSize = (80, 25);
		ptyConfiguration.PixelSize = (640, 400);

		string?[] argv = new string[arguments.Length + 2];

		argv[0] = fileName;
		arguments.CopyTo(argv, 1);

		using (var result = Forker.ForkPTYAndExec(ptyConfiguration, fileName, argv))
		{
			var strategy = new TTYStrategy();

			strategy.Execute(
				context,
				result.PTYStream,
				result.ProcessExit);
		}
	}
}
