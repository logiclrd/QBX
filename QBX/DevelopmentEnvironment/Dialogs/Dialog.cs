using System;
using System.Collections.Generic;

using QBX.Firmware;
using QBX.Hardware;

namespace QBX.DevelopmentEnvironment.Dialogs;

public abstract class Dialog(Configuration configuration)
{
	public int Width = 40;
	public int Height = 7;

	// X is implicit :-)
	public int Y = 12;

	public string Title = "";

	public List<Widget> Widgets = new List<Widget>();

	public Widget? FocusedWidget = null;

	public event EventHandler? Close;

	const string Spaces = "                                                                                ";
	const string HorizontalRule = "────────────────────────────────────────────────────────────────────────────────";

	const char TopLeft = '┌';
	const char TopRight = '┐';

	const char LeftRight = '│';

	const char ConnectLeft = '├';
	const char ConnectRight = '┤';

	const char BottomLeft = '└';
	const char BottomRight = '┘';

	public void Render(TextLibrary visual)
	{
		DrawFrame(visual, out var bounds);
		RenderWidgets(visual, bounds);
	}

	void DrawFrame(TextLibrary visual, out IntegerRect bounds)
	{
		int midX = visual.CharacterWidth / 2;

		int x1 = midX - Width / 2;
		int x2 = x1 + Width - 1;

		int y1 = Y;
		int y2 = Y + Height - 1;
		int dividerY = y2 - 2;

		configuration.DisplayAttributes.DialogBoxNormalText.Set(visual);

		visual.MoveCursor(x1, y1);

		int titleX1 = midX - Title.Length / 2 - 1; // Title characters plus flanking spaces
		int titleX2 = titleX1 + Title.Length + 1;

		if (Title.Length == 0)
			titleX2 = titleX1 - 1; // Remove flanking spaces

		visual.WriteText(TopLeft);
		visual.WriteText(HorizontalRule.AsSpan().Slice(0, titleX1 - x1 - 1));

		if (Title.Length > 0)
		{
			visual.WriteText((byte)' ');
			visual.WriteText(Title);
			visual.WriteText((byte)' ');
		}

		visual.WriteText(HorizontalRule.AsSpan().Slice(0, x2 - titleX2 - 1));
		visual.WriteText(TopRight);

		for (int y = y1 + 1; y < y2; y++)
		{
			visual.MoveCursor(x1, y);

			if (y == dividerY)
			{
				visual.WriteText(ConnectLeft);
				visual.WriteText(HorizontalRule.AsSpan().Slice(0, Width - 2));
				visual.WriteText(ConnectRight);
			}
			else
			{
				visual.WriteText(LeftRight);
				visual.WriteText(Spaces.AsSpan().Slice(0, Width - 2));
				visual.WriteText(LeftRight);
			}

			configuration.DisplayAttributes.PullDownMenuandDialogBoxShadow.Set(visual);
			visual.WriteAttributes(2);
			configuration.DisplayAttributes.DialogBoxNormalText.Set(visual);
		}

		visual.MoveCursor(x1, y2);

		visual.WriteText(BottomLeft);
		visual.WriteText(HorizontalRule.AsSpan().Slice(0, Width - 2));
		visual.WriteText(BottomRight);
		configuration.DisplayAttributes.PullDownMenuandDialogBoxShadow.Set(visual);
		visual.WriteAttributes(2);

		visual.MoveCursor(x1 + 2, y2 + 1);
		visual.WriteAttributes(Width);

		configuration.DisplayAttributes.DialogBoxNormalText.Set(visual);

		bounds = new IntegerRect();

		bounds.X1 = x1 + 1;
		bounds.Y1 = y1 + 1;
		bounds.X2 = x2 - 1;
		bounds.Y2 = y2 - 1;
	}

	public void RenderWidgets(TextLibrary visual, IntegerRect bounds)
	{
		foreach (var widget in Widgets)
		{
			configuration.DisplayAttributes.DialogBoxNormalText.Set(visual);

			widget.IsFocused = (widget == FocusedWidget);
			widget.Render(visual, bounds, configuration);
		}

		FocusedWidget?.PlaceCursorForFocus(visual, bounds);

		visual.UpdatePhysicalCursor();
	}

	public void ProcessKey(KeyEvent input)
	{
		// TODO
		if (input.ScanCode == ScanCode.Escape)
			Close?.Invoke(this, EventArgs.Empty);
	}
}
