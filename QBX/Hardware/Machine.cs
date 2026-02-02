using QBX.Firmware;
using QBX.Interrupts;

namespace QBX.Hardware;

public class Machine
{
	public SystemMemory SystemMemory { get; }
	public GraphicsArray GraphicsArray { get; }
	public Adapter Display { get; }
	public Video VideoFirmware { get; }
	public MouseDriver MouseDriver { get; }
	public Keyboard Keyboard { get; }
	public Mouse Mouse { get; }
	public Speaker Speaker { get; }
	public TimerChip Timer { get; }

	public MemoryBus MemoryBus { get; }
	public InterruptHandler?[] InterruptHandlers { get; } = new InterruptHandler[256];

	public bool KeepRunning = true;
	public int ExitCode = 0;

	public Machine()
	{
		SystemMemory = new SystemMemory();

		MemoryBus = new MemoryBus();

		GraphicsArray = new GraphicsArray();
		Display = new Adapter(GraphicsArray);
		Keyboard = new Keyboard(this);
		Mouse = new Mouse();
		Speaker = new Speaker(this);
		Timer = new TimerChip(Speaker);

		MemoryBus.MapRange(0x0000, SystemMemory.Length, SystemMemory);
		MemoryBus.MapRange(0xA000, GraphicsArray.VRAM.Length, GraphicsArray);

		VideoFirmware = new Video(this);
		MouseDriver = new MouseDriver(this);

		InterruptHandlers[0x33] = new Interrupt0x33(this);
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
