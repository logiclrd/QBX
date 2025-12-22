using QBX.Hardware;

namespace QBX.Firmware;

public class Video(Machine machine)
{
	public void SetMode(int modeNumber)
	{
		switch (modeNumber)
		{
			case 0:
				break;
			case 13:
				machine.GraphicsArray.OutPort2(
					GraphicsArray.Graphics.IndexPort,
					GraphicsArray.Graphics.GraphicsMode,
					GraphicsArray.Graphics.GraphicsMode_Shift256);

				break;
			default:
				throw new Exception("Screen mode not implemented: " + modeNumber);
		}
	}
}
