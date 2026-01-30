using QBX.Firmware;
using QBX.Interrupts;

namespace QBX.Hardware;

public class Machine
{
	public SystemMemory SystemMemory { get; }
	public GraphicsArray GraphicsArray { get; }
	public Adapter Display { get; }
	public Video VideoFirmware { get; }
	public Keyboard Keyboard { get; }
	public Speaker Speaker { get; }
	public TimerChip Timer { get; }

	public MemoryBus MemoryBus { get; }
	public InterruptHandler?[] InterruptHandlers { get; } = new InterruptHandler[256];

	public bool KeepRunning = true;
	public int ExitCode = 0;

	public Machine()
	{
		SystemMemory = new SystemMemory();
		GraphicsArray = new GraphicsArray();
		Display = new Adapter(GraphicsArray);
		VideoFirmware = new Video(this);
		Keyboard = new Keyboard(this);
		Speaker = new Speaker(this);
		Timer = new TimerChip(Speaker);

		MemoryBus = new MemoryBus();

		MemoryBus.MapRange(0x0000, SystemMemory.Length, SystemMemory);
		MemoryBus.MapRange(0xA000, GraphicsArray.VRAM.Length, GraphicsArray);
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
