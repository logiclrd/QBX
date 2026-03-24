using System.IO;
using System.Threading;

using QBX.Hardware;

namespace QBX.ExecutionEngine.Compiled.Statements.Shell.Windows.ConsoleAPI;

class ProxyKeyEventReceiver(BinaryReader reader) : IKeyEventSource
{
	public ProxyKeyEventReceiver(Stream stream)
		: this(new BinaryReader(stream))
	{
	}

	public KeyEvent? ReceiveNextEvent(CancellationToken cancellationToken)
	{
		try
		{
			return KeyEvent.Deserialize(reader);
		}
		catch
		{
			return null;
		}
	}
}
