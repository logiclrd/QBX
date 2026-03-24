using System.Threading;

using QBX.Hardware;

namespace QBX.ExecutionEngine.Compiled.Statements.Shell;

public interface IKeyEventSource
{
	KeyEvent? ReceiveNextEvent(CancellationToken cancellationToken);
}
