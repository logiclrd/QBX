using QBX.Firmware;

namespace QBX.DevelopmentEnvironment;

public class DisplayAttribute(int fg, int bg, string name)
{
	public string Name => name;
	public int Foreground = fg;
	public int Background = bg;

	public void Set(TextLibrary library)
		=> library.SetAttributes(Foreground, Background);

	public void SetInverted(TextLibrary library)
		=> library.SetAttributes(Background, Foreground);

	public void CopyFrom(DisplayAttribute other)
	{
		Foreground = other.Foreground;
		Background = other.Background;
	}
}
