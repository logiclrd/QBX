namespace QBX.Terminal;

public struct TerminalAttributeState
{
	public int Foreground;
	public int Background;
	public Intensity Intensity;
	public bool Italic;
	public bool Underline;
	public bool Blink;
	public bool Reverse;

	public TerminalAttributeState()
	{
		Foreground = 7;
		Intensity = Intensity.Regular;
	}

	public byte BuildAttributeByte()
	{
		int foreground = Foreground & 7;
		int background = Background & 7;

		if (Reverse)
			(foreground, background) = (background, foreground);
		else
		{
			if (Intensity == Intensity.Bold)
				foreground |= 8;
			if (Blink)
				background |= 8;
		}

		if (Reverse)
			(foreground, background) = (background & 7, foreground & 7);

		return unchecked((byte)(foreground | (background << 4)));
	}
}
