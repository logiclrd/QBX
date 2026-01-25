using System;

using QBX.Firmware;

namespace QBX.DevelopmentEnvironment.Dialogs;

public static class DialogPaint
{
	const string Spaces = "                                                                                ";
	const string HorizontalRule = "────────────────────────────────────────────────────────────────────────────────";

	const char TopLeft = '┌';
	const char TopRight = '┐';

	const char LeftRight = '│';

	const char ConnectLeft = '├';
	const char ConnectRight = '┤';

	const char BottomLeft = '└';
	const char BottomRight = '┘';

	public static void DrawBox(int x, int y, int width, int height, string title, Configuration configuration, TextLibrary visual)
	{
		int x1 = x;
		int y1 = y;
		int x2 = x + width - 1;
		int y2 = y + height - 1;

		int innerWidth = width - 2;

		visual.MoveCursor(x1, y1);

		int midX = (x1 + x2) / 2;

		int titleX1 = midX - title.Length / 2 - 1; // Title characters plus flanking spaces
		int titleX2 = titleX1 + title.Length + 1;

		if (title.Length == 0)
			titleX2 = titleX1 - 1; // Remove flanking spaces

		visual.WriteText(TopLeft);
		visual.WriteText(HorizontalRule.AsSpan().Slice(0, titleX1 - x1 - 1));

		if (title.Length > 0)
		{
			visual.WriteText((byte)' ');
			visual.WriteText(title);
			visual.WriteText((byte)' ');
		}

		visual.WriteText(HorizontalRule.AsSpan().Slice(0, x2 - titleX2 - 1));
		visual.WriteText(TopRight);

		for (y = y1 + 1; y < y2; y++)
		{
			visual.MoveCursor(x1, y);

			visual.WriteText(LeftRight);
			visual.WriteText(Spaces.AsSpan().Slice(0, innerWidth));
			visual.WriteText(LeftRight);
		}

		visual.MoveCursor(x1, y2);

		visual.WriteText(BottomLeft);
		visual.WriteText(HorizontalRule.AsSpan().Slice(0, innerWidth));
		visual.WriteText(BottomRight);
	}

	public static void DrawDialogFrame(int y, int width, int height, string title, Configuration configuration, TextLibrary visual, out IntegerRect bounds)
	{
		int midX = visual.CharacterWidth / 2;

		int x1 = midX - width / 2;
		int x2 = x1 + width - 1;

		int y1 = y;
		int y2 = y + height - 1;
		int dividerY = y2 - 2;

		configuration.DisplayAttributes.DialogBoxNormalText.Set(visual);

		visual.MoveCursor(x1, y1);

		int titleX1 = midX - title.Length / 2 - 1; // Title characters plus flanking spaces
		int titleX2 = titleX1 + title.Length + 1;

		if (title.Length == 0)
			titleX2 = titleX1 - 1; // Remove flanking spaces

		visual.WriteText(TopLeft);
		visual.WriteText(HorizontalRule.AsSpan().Slice(0, titleX1 - x1 - 1));

		if (title.Length > 0)
		{
			visual.WriteText((byte)' ');
			visual.WriteText(title);
			visual.WriteText((byte)' ');
		}

		visual.WriteText(HorizontalRule.AsSpan().Slice(0, x2 - titleX2 - 1));
		visual.WriteText(TopRight);

		for (y = y1 + 1; y < y2; y++)
		{
			visual.MoveCursor(x1, y);

			if (y == dividerY)
			{
				visual.WriteText(ConnectLeft);
				visual.WriteText(HorizontalRule.AsSpan().Slice(0, width - 2));
				visual.WriteText(ConnectRight);
			}
			else
			{
				visual.WriteText(LeftRight);
				visual.WriteText(Spaces.AsSpan().Slice(0, width - 2));
				visual.WriteText(LeftRight);
			}

			configuration.DisplayAttributes.PullDownMenuandDialogBoxShadow.Set(visual);
			visual.WriteAttributes(2);
			configuration.DisplayAttributes.DialogBoxNormalText.Set(visual);
		}

		visual.MoveCursor(x1, y2);

		visual.WriteText(BottomLeft);
		visual.WriteText(HorizontalRule.AsSpan().Slice(0, width - 2));
		visual.WriteText(BottomRight);
		configuration.DisplayAttributes.PullDownMenuandDialogBoxShadow.Set(visual);
		visual.WriteAttributes(2);

		visual.MoveCursor(x1 + 2, y2 + 1);
		visual.WriteAttributes(width);

		configuration.DisplayAttributes.DialogBoxNormalText.Set(visual);

		bounds = new IntegerRect();

		bounds.X1 = x1 + 1;
		bounds.Y1 = y1 + 1;
		bounds.X2 = x2 - 1;
		bounds.Y2 = y2 - 1;
	}
}
