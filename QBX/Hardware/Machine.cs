using System;

using QBX.OperatingSystem;
using QBX.Firmware;
using QBX.Interrupts;
using QBX.Hardware.AdLib;

namespace QBX.Hardware;

public class Machine
{
	public SystemClock SystemClock { get; }
	public SystemMemory SystemMemory { get; }
	public GraphicsArray GraphicsArray { get; }
	public FirmwareROM FirmwareROM { get; }
	public Adapter Display { get; }
	public Video VideoFirmware { get; }
	public KeyboardDriver KeyboardDriver { get; }
	public MouseDriver MouseDriver { get; }
	public Keyboard Keyboard { get; }
	public Mouse Mouse { get; }
	public Speaker Speaker { get; }
	public GravisUltraSound GravisUltraSound { get; }
	public AdLibGold AdLibGold { get; }
	public TimerChip Timer { get; }

	public DOS DOS { get; }

	public MemoryBus MemoryBus { get; }
	public InterruptHandler?[] InterruptHandlers { get; } = new InterruptHandler[256];

	public bool KeepRunning = true;
	public int ExitCode = 0;

	internal IDriveInfoProvider? OverrideDriveInfoProvider;
	internal IFileDateTimeProvider? OverrideFileDateTimeProvider;

	public Machine()
		: this(overrideDriveInfoProvider: null, overrideFileDateTimeProvider: null)
	{
	}

	internal Machine(IDriveInfoProvider? overrideDriveInfoProvider = null, IFileDateTimeProvider? overrideFileDateTimeProvider = null)
	{
		OverrideDriveInfoProvider = overrideDriveInfoProvider;
		OverrideFileDateTimeProvider = overrideFileDateTimeProvider;

		SystemClock = new SystemClock();

		SystemMemory = new SystemMemory(this);

		MemoryBus = new MemoryBus();

		VideoFirmware = new Video(this);

		GraphicsArray = new GraphicsArray();
		FirmwareROM = new FirmwareROM(VideoFirmware);
		Display = new Adapter(GraphicsArray);
		Keyboard = new Keyboard(this);
		Mouse = new Mouse();
		Speaker = new Speaker(this);
		GravisUltraSound = new GravisUltraSound();
		AdLibGold = new AdLibGold();
		Timer = new TimerChip(Speaker);

		MemoryBus.MapRange(0x0000, SystemMemory.Length, SystemMemory);
		MemoryBus.MapRange(0xA000, GraphicsArray.VRAM.Length, GraphicsArray);
		MemoryBus.MapRange(0xF000, FirmwareROM.Length, FirmwareROM);

		SystemMemory.InitializeBIOSDataArea();

		SystemMemory.BIOSDataArea.VideoDisplayCombinationCode =
			BDAVideoDisplayCombinationCode.VGA_ColourDisplay;

		KeyboardDriver = new KeyboardDriver(this);
		MouseDriver = new MouseDriver(this);

		InterruptHandlers[0x08] = new Interrupt0x08(this);
		InterruptHandlers[0x10] = new Interrupt0x10(this);
		InterruptHandlers[0x21] = new Interrupt0x21(this);
		InterruptHandlers[0x33] = new Interrupt0x33(this);

		Timer.Timer0.Control(0x36);
		Timer.Timer0.WriteData(0);
		Timer.Timer0.WriteData(0);

		Timer.Timer1.Control(0x54);
		Timer.Timer1.WriteData(0);

		Timer.Timer2.Control(0xB6);
		Timer.Timer2.WriteData(0);
		Timer.Timer2.WriteData(0);

		KeyboardDriver.InferLayoutFromSDLState();

		VideoFirmware.SetMode(3);

		if (VideoFirmware.VisualLibrary is TextLibrary textLibrary)
			textLibrary.ShowCursor();

		DOS = new DOS(this);
	}

	public void OutPort(int portNumber, byte data)
	{
		GraphicsArray.OutPort(portNumber, data);
		Keyboard.OutPort(portNumber, data);
		Timer.OutPort(portNumber, data);
		Speaker.OutPort(portNumber, data);
		GravisUltraSound.OutPort(portNumber, data);
		AdLibGold.OutPort(portNumber, data);
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

		value = GravisUltraSound.InPort(portNumber, out handled);
		if (handled)
			return value;

		value = AdLibGold.InPort(portNumber, out handled);
		if (handled)
			return value;

		// ISA bus I/O is pulled up, so if nothing responds, we see all bits set.
		return 0xFF;
	}

	double[]? s_mixBuffer;

	public void GetMoreSound(Span<short> buffer)
	{
		if ((s_mixBuffer == null) || (s_mixBuffer.Length < buffer.Length))
			s_mixBuffer = new double[buffer.Length];

		Span<double> mixBuffer = s_mixBuffer.AsSpan().Slice(0, buffer.Length);

		mixBuffer.Clear();

		Speaker.GetMoreSound(mixBuffer);
		GravisUltraSound.GetMoreSound(mixBuffer);
		AdLibGold.GetMoreSound(mixBuffer);

		for (int i = 0; i < buffer.Length; i++)
			buffer[i] = (short)double.Clamp(mixBuffer[i], short.MinValue, short.MaxValue);
	}
}
