using System;

using QBX.Firmware;
using QBX.Utility;

namespace QBX.DevelopmentEnvironment.Dialogs.Widgets;

public class Button : Widget
{
	public string Text = "";
	public int AccessKeyIndex = -1;

	public override bool IsEnabled
	{
		get => base.IsEnabled;
		set => base.IsEnabled = IsTabStop = value;
	}

	public override char AccessKeyCharacter
		=> ((AccessKeyIndex >= 0) && (AccessKeyIndex < Text.Length))
			? Text[AccessKeyIndex]
			: '\0';

	public Button()
	{
		IsTabStop = true;
	}

	public Action? Activated;

	public override bool Activate()
	{
		if (IsEnabled)
			Activated?.Invoke();
		return true;
	}

	public override void Render(TextLibrary visual, IntegerRect bounds, Configuration configuration)
	{
		var textAttr =
			IsEnabled
			? configuration.DisplayAttributes.DialogBoxCommandButtons
			: configuration.DisplayAttributes.PullDownMenuandDialogBoxDisabledItems;

		var borderAttr =
			IsFocused
			? configuration.DisplayAttributes.DialogBoxActiveCommandButtonBorderCharacters
			: textAttr;

		var accessKeyAttr =
			IsEnabled
			? configuration.DisplayAttributes.DialogBoxAccessKeys
			: configuration.DisplayAttributes.PullDownMenuandDialogBoxDisabledItems;

		int x = X + bounds.X1;
		int y = Y + bounds.Y1;

		if (bounds.Contains(x, y))
		{
			visual.MoveCursor(x, y);

			borderAttr.Set(visual);
			visual.WriteText('<');
			textAttr.Set(visual);

			x++;

			int textAreaWidth = Width - 2;
			int textOffset = (textAreaWidth - Text.Length) / 2;

			WriteTextWithAccessKey(
				visual,
				x, y,
				textAreaWidth,
				textOffset,
				Text.AsSpan(),
				AccessKeyIndex,
				textAttr,
				accessKeyAttr);

			x += textAreaWidth;

			if (x < bounds.X2)
			{
				borderAttr.Set(visual);
				visual.WriteText('>');
			}
		}
	}

	public override void PlaceCursorForFocus(TextLibrary visual, IntegerRect bounds)
	{
		int textAreaWidth = Width - 2;
		int textOffset = (textAreaWidth - Text.Length) / 2;

		visual.MoveCursor(
			bounds.X1 + X + 1 + textOffset,
			bounds.Y1 + Y);
	}

	public override void AccessKeyUsed(IFocusContext focusContext)
	{
		Activate();
	}
}
