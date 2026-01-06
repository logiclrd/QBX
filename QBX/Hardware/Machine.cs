using QBX.Firmware;

namespace QBX.Hardware;

public class Machine
{
	public GraphicsArray GraphicsArray { get; }
	public Adapter Display { get; }
	public Video VideoFirmware { get; }
	public Keyboard Keyboard { get; }

	public bool KeepRunning = true;
	public int ExitCode = 0;

	public Machine()
	{
		GraphicsArray = new GraphicsArray();
		Display = new Adapter(GraphicsArray);
		VideoFirmware = new Video(this);
		Keyboard = new Keyboard();
	}
}
