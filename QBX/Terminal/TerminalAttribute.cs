using QBX.Firmware;

namespace QBX.Terminal;

public class TerminalAttribute(VisualLibrary visual)
{
	public TerminalAttributeState State;

	public void Reset()
	{
		State = new TerminalAttributeState();
	}

	public void SetRegularIntensity() { State.Intensity = Intensity.Regular; }
	public void SetBold() { State.Intensity = Intensity.Bold; }
	public void SetHalfBright() { State.Intensity = Intensity.HalfBright; }

	public void SetItalic() { State.Italic = true; }
	public void ClearItalic() { State.Italic = false; }

	public void SetUnderline() { State.Underline = true; }
	public void ClearUnderline() { State.Underline = false; }

	public void SetBlink() { State.Blink = true; }
	public void ClearBlink() { State.Blink = false; }

	public void SetReverse() { State.Reverse = true; }
	public void ClearReverse() { State.Reverse = false; }

	public void SetForeground(int c) { State.Foreground = c & 7; }
	public void SetBackground(int c) { State.Background = c & 7; }

	public void Commit()
	{
		visual.CurrentAttributeByte = State.BuildAttributeByte();
	}
}
