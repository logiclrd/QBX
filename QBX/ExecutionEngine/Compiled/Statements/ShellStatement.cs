using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

using QBX.ExecutionEngine.Execution.Variables;
using QBX.Firmware;
using QBX.Firmware.Fonts;
using QBX.Hardware;
using QBX.Terminal;

using ExecutionContext = QBX.ExecutionEngine.Execution.ExecutionContext;
using StackFrame = QBX.ExecutionEngine.Execution.StackFrame;

namespace QBX.ExecutionEngine.Compiled.Statements;

public partial class ShellStatement(CodeModel.Statements.Statement source) : Executable(source)
{
	public Evaluable? CommandStringExpression;

	public override void Execute(ExecutionContext context, StackFrame stackFrame)
	{
		string commandString = "";

		if (CommandStringExpression != null)
		{
			var commandStringResult = (StringVariable)CommandStringExpression.Evaluate(context, stackFrame);

			commandString = commandStringResult.ValueString;
		}

		string shellEnvironmentVariable;
		string fallbackShell;
		string commandSwitch;

		if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
		{
			shellEnvironmentVariable = "COMSPEC";
			fallbackShell = "CMD.EXE";
			commandSwitch = "/C";
		}
		else
		{
			shellEnvironmentVariable = "SHELL";
			fallbackShell = "/bin/sh";
			commandSwitch = "-c";
		}

		var shell = Environment.GetEnvironmentVariable(shellEnvironmentVariable) ?? fallbackShell;

		try
		{
			string arguments = commandString == "" ? "" : (commandSwitch + " " + commandString);

			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
				RunChildProcessWindows(context, shell, arguments);
			else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
				RunChildProcessLinux(context, shell, arguments);
		}
		catch
		{
			context.VisualLibrary.WriteText("Bad command or file name");
			context.VisualLibrary.NewLine();
		}
	}
}

