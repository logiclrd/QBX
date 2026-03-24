using System.Threading;

using QBX.Hardware;

namespace QBX.ExecutionEngine.Compiled.Statements.Shell;

public class KeyboardKeyEventSource(Keyboard keyboard) : IKeyEventSource
{
	public KeyEvent? ReceiveNextEvent(CancellationToken cancellationToken)
	{
		keyboard.WaitForInput(cancellationToken);

		return keyboard.GetNextEvent();
	}
}
