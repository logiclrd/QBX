using System;

using QBX.Firmware;

namespace QBX.DevelopmentEnvironment.Dialogs.Widgets;

public class Label : Widget
{
	public string Text = "";
	public int AccessKeyIndex = -1;
	public Widget? FocusTarget;

	public override char AccessKeyCharacter
		=> ((AccessKeyIndex >= 0) && (AccessKeyIndex < Text.Length))
			? Text[AccessKeyIndex]
			: '\0';

	public Label AutoSize()
	{
		Width = Text.Length;
		Height = 1;

		return this;
	}

	public override void Render(TextLibrary visual, IntegerRect bounds, Configuration configuration)
	{
		int x = X + bounds.X1;
		int y = Y + bounds.Y1;

		if ((y < bounds.Y1) || (y > bounds.Y2))
			return;

		var chars = Text.AsSpan();
		int accessKeyIndex = AccessKeyIndex;

		if (x < bounds.X1)
		{
			int clipped = bounds.X1 - x;

			if (clipped >= chars.Length)
				return;

			chars = chars.Slice(clipped);
			x += clipped;
			AccessKeyIndex -= clipped;
		}

		if (x + chars.Length - 1 > bounds.X2)
		{
			int remaining = bounds.X2 - x + 1;

			if (remaining <= 0)
				return;

			chars = chars.Slice(0, remaining);
		}

		WriteTextWithAccessKey(
			visual,
			x, y,
			textAreaWidth: Width,
			textOffset: 0,
			chars,
			AccessKeyIndex,
			configuration.DisplayAttributes.DialogBoxNormalText,
			configuration.DisplayAttributes.DialogBoxAccessKeys);
	}

	public override void PlaceCursorForFocus(TextLibrary visual, IntegerRect bounds)
	{
		// ??
	}
}
