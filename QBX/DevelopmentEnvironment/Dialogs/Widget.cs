using QBX.Firmware;
using QBX.Hardware;
using System;

namespace QBX.DevelopmentEnvironment.Dialogs;

public abstract class Widget
{
	public int X, Y;
	public int Width, Height = 1;

	public bool IsFocused;
	public bool IsTabStop = false;

	public Action? GotFocus;
	public Action? LostFocus;

	internal virtual void NotifyGotFocus() => GotFocus?.Invoke();
	internal virtual void NotifyLostFocus() => LostFocus?.Invoke();

	public virtual char AccessKeyCharacter => '\0';

	public virtual ScanCode GetAccessKey(out bool shifted)
		=> AccessKeyCharacter.GetScanCode(out shifted);

	public abstract void Render(TextLibrary visual, IntegerRect bounds, Configuration configuration);
	public abstract void PlaceCursorForFocus(TextLibrary visual, IntegerRect bounds);

	string _spaces = "                                                                                ";

	protected void WriteTextWithAccessKey(
		TextLibrary visual,
		int x, int y,
		int textAreaWidth, int textOffset,
		ReadOnlySpan<char> chars,
		int accessKeyIndex,
		DisplayAttribute normalAttr,
		DisplayAttribute accessKeyAttr)
	{
		if (textAreaWidth > _spaces.Length)
			_spaces = new string(' ', textAreaWidth * 2);

		if (x < 0)
		{
			int trimLeftChars = -x;

			chars = chars.Slice(trimLeftChars);
			accessKeyIndex -= trimLeftChars;
			textAreaWidth -= trimLeftChars;
			textOffset -= trimLeftChars;

			x = 0;
		}

		int remainingChars = visual.CharacterWidth - x;

		if (textAreaWidth > remainingChars)
			textAreaWidth = remainingChars;

		visual.MoveCursor(x, y);

		normalAttr.Set(visual);

		if (textOffset > 0)
		{
			visual.WriteText(_spaces.AsSpan().Slice(0, textOffset));
			textAreaWidth -= textOffset;
		}
		else if (textOffset < 0)
		{
			int trimLeftChars = -textOffset;

			chars = chars.Slice(trimLeftChars);
			accessKeyIndex -= trimLeftChars;
			textAreaWidth -= trimLeftChars;

			textOffset = 0;
		}

		if (chars.Length > textAreaWidth)
			chars = chars.Slice(0, textAreaWidth);

		if ((accessKeyIndex < 0) || (accessKeyIndex >= chars.Length))
		{
			visual.WriteText(chars);
			textAreaWidth -= chars.Length;

			if (textAreaWidth > 0)
				visual.WriteText(_spaces.AsSpan().Slice(0, textAreaWidth));
		}
		else
		{
			visual.WriteText(chars.Slice(0, accessKeyIndex));
			textAreaWidth -= accessKeyIndex;

			accessKeyAttr.Set(visual);
			visual.WriteText(chars[accessKeyIndex]);
			textAreaWidth--;
			normalAttr.Set(visual);

			visual.WriteText(chars.Slice(accessKeyIndex + 1));
			textAreaWidth -= (chars.Length - accessKeyIndex - 1);

			if (textAreaWidth > 0)
				visual.WriteText(_spaces.AsSpan().Slice(0, textAreaWidth));
		}
	}

	public virtual bool ProcessKey(KeyEvent input, IOvertypeFlag overtypeFlag)
	{
		return false;
	}

	public virtual bool Activate()
	{
		return false;
	}
}
