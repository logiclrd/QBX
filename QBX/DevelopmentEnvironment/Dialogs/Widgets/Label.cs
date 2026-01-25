using System;

using QBX.Firmware;

namespace QBX.DevelopmentEnvironment.Dialogs.Widgets;

public class Label : Widget
{
	public string Text = "";

	public override void Render(TextLibrary visual, IntegerRect bounds, Configuration configuration)
	{
		int x = X + bounds.X1;
		int y = Y + bounds.Y1;

		if ((y < bounds.Y1) || (y > bounds.Y2))
			return;

		var chars = Text.AsSpan();

		if (x < bounds.X1)
		{
			int clipped = bounds.X1 - x;

			if (clipped >= chars.Length)
				return;

			chars = chars.Slice(clipped);
			x += clipped;
		}

		if (x + chars.Length - 1 > bounds.X2)
		{
			int remaining = bounds.X2 - x + 1;

			if (remaining <= 0)
				return;

			chars = chars.Slice(remaining);
		}

		visual.WriteTextAt(x, y, chars);
	}

	public override void PlaceCursorForFocus(TextLibrary visual, IntegerRect bounds)
	{
		// ??
	}
}
