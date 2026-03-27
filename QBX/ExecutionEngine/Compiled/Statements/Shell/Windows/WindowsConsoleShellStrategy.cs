using System.IO.Pipes;

using QBX.Platform.Windows;

using ExecutionContext = QBX.ExecutionEngine.Execution.ExecutionContext;

namespace QBX.ExecutionEngine.Compiled.Statements.Shell.Windows;

public abstract class WindowsConsoleShellStrategy : ShellStrategy
{
	public virtual ProcessCreationFlags AdditionalProcessCreationFlags => 0;

	public abstract void Execute(ExecutionContext context, SafePseudoConsoleHandle hPC, AnonymousPipeServerStream stdinPipe, AnonymousPipeServerStream stdoutPipe, PROCESS_INFORMATION processInformation);
}
