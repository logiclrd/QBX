using System.IO;

using QBX.Hardware;
using QBX.Terminal;

namespace QBX.ExecutionEngine.Compiled.Statements.Shell.Windows.ConsoleAPI;

public class ProxyInjector : InputInjector
{
	BinaryWriter _writer;

	public ProxyInjector(Stream stream)
	{
		_writer = new BinaryWriter(stream);
	}

	public override void Inject(KeyEvent evt)
	{
		evt.Serialize(_writer);
	}

	public override void Dispose()
	{
		_writer.Close();
	}
}

