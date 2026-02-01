using System;
using QBX.CodeModel;
using QBX.Firmware;
using QBX.Utility;

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

	const char UpArrow = '↑';
	const char DownArrow = '↓';
	const char LeftArrow = '←';
	const char RightArrow = '→';

	const char ScrollBarTrack = '░';
	const char ScrollBarHandle = '█';

	public static void WriteSpaces(int count, TextLibrary visual)
		=> visual.WriteText(Spaces.AsSpan().Slice(0, count));

	public static void WriteSpacesAt(int x, int y, int count, TextLibrary visual)
		=> visual.WriteTextAt(x, y, Spaces.AsSpan().Slice(0, count));

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

	[ThreadStatic]
	static char[]? s_ScrollBarTrackSpan;

	public static void DrawScrollableBox(int x, int y, int width, int height, string title, Configuration configuration, TextLibrary visual, int horizontalScrollValue = -1, int horizontalScrollMax = -1, int verticalScrollValue = -1, int verticalScrollMax = -1)
	{
		int x1 = x;
		int y1 = y;
		int x2 = x + width - 1;
		int y2 = y + height - 1;

		int clipX2 = x2;
		int clipY2 = y2;

		bool renderVerticalScrollBar = (verticalScrollValue >= 0) && (height >= 4);
		bool renderHorizontalScrollBar = (horizontalScrollValue >= 0) && (width >= 4);

		if (renderVerticalScrollBar)
			clipX2--;
		if (renderHorizontalScrollBar)
			clipY2--;

		using (visual.PushClipRect(x1, y1, clipX2, clipY2))
			DrawBox(x, y, width, height, title: "", configuration, visual);

		if (renderVerticalScrollBar)
		{
			if (verticalScrollMax < 1)
				verticalScrollMax = 1;

			int trackY1 = y1 + 2;
			int trackY2 = y2 - 2;

			int handleY = trackY1 + verticalScrollValue * (trackY2 - trackY1) / verticalScrollMax;

			visual.WriteTextAt(x2, y1, TopRight);
			visual.WriteTextAt(x2, y1 + 1, UpArrow);

			for (int trackY = trackY1; trackY <= trackY2; trackY++)
				visual.WriteTextAt(x2, trackY, (trackY == handleY) ? ScrollBarHandle : ScrollBarTrack);

			visual.WriteTextAt(x2, y2 - 1, DownArrow);
		}

		if (renderHorizontalScrollBar)
		{
			if (horizontalScrollMax < 1)
				horizontalScrollMax = 1;

			int trackX1 = x1 + 2;
			int trackX2 = x2 - 2;

			int trackWidth = trackX2 - trackX1 + 1;

			int handleOffset = horizontalScrollValue * (trackX2 - trackX1) / horizontalScrollMax;

			if (handleOffset < 0)
				handleOffset = 0;
			if (handleOffset > trackWidth)
				handleOffset = trackWidth;

			int trackLeft = handleOffset;
			int trackRight = trackWidth - handleOffset - 1;

			int requiredCharacters = Math.Max(trackLeft, trackRight);

			if ((s_ScrollBarTrackSpan == null) || (s_ScrollBarTrackSpan.Length < requiredCharacters))
			{
				s_ScrollBarTrackSpan = new char[requiredCharacters * 2];
				s_ScrollBarTrackSpan.AsSpan().Fill(ScrollBarTrack);
			}

			visual.MoveCursor(x1, y2);

			visual.WriteText(BottomLeft);
			visual.WriteText(LeftArrow);

			visual.WriteText(s_ScrollBarTrackSpan.AsSpan().Slice(0, trackLeft));
			visual.WriteText(ScrollBarHandle);
			visual.WriteText(s_ScrollBarTrackSpan.AsSpan().Slice(0, trackRight));

			visual.WriteText(RightArrow);
		}

		if (renderVerticalScrollBar || renderHorizontalScrollBar)
			visual.WriteTextAt(x2, y2, BottomRight);
	}
}
