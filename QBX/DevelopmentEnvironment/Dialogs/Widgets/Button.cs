using QBX.Firmware;

namespace QBX.DevelopmentEnvironment.Dialogs.Widgets;

public class Button : Widget
{
	public string Text = "";
	public int AcceleratorKeyIndex;
	public bool IsEnabled;

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

		var acceleratorAttr = configuration.DisplayAttributes.DialogBoxAccessKeys;

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

			for (int i = 0; (i < textAreaWidth) && (x < bounds.X2); i++, x++)
			{
				int index = i - textOffset;

				if ((index < 0) || (index >= Text.Length))
					visual.WriteText(' ');
				else if (index == AcceleratorKeyIndex)
				{
					acceleratorAttr.Set(visual);
					visual.WriteText(Text[index]);
					textAttr.Set(visual);
				}
				else
					visual.WriteText(Text[index]);
			}

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
}
