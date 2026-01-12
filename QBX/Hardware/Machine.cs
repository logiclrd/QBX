using QBX.Firmware;

namespace QBX.Hardware;

public class Machine
{
	public SystemMemory SystemMemory { get; }
	public GraphicsArray GraphicsArray { get; }
	public Adapter Display { get; }
	public Video VideoFirmware { get; }
	public Keyboard Keyboard { get; }
	public TimerChip Timer { get; }

	public bool KeepRunning = true;
	public int ExitCode = 0;

	public Machine()
	{
		SystemMemory = new SystemMemory();
		GraphicsArray = new GraphicsArray();
		Display = new Adapter(GraphicsArray);
		VideoFirmware = new Video(this);
		Keyboard = new Keyboard(this);
		Timer = new TimerChip();
	}

	public void OutPort(int portNumber, byte data)
	{
		GraphicsArray.OutPort(portNumber, data);
		Keyboard.OutPort(portNumber, data);
		Timer.OutPort(portNumber, data);
	}

	public byte InPort(int portNumber)
	{
		bool handled;
		byte value;

		value = GraphicsArray.InPort(portNumber, out handled);
		if (handled)
			return value;

		value = Timer.InPort(portNumber, out handled);
		if (handled)
			return value;

		value = Keyboard.InPort(portNumber, out handled);
		if (handled)
			return value;

		return 0;
	}
}
