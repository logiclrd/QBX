using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using QBX.CodeModel;

namespace QBX.DevelopmentEnvironment;

public class SelectionManager(Viewport owner, Clipboard clipboard)
{
	Viewport _owner = owner;

	// NB: X is exclusive upper bound, Y is inclusive.
	//
	// - If you select characters 2-5 in a row, you've selected 3 characters.
	// - If you select rows 1-3, you've selected 3 rows.
	//
	// In other words, _clipEndX is the first character that isn't selected,
	// while _clipEndY is the last row that is selected.

	int _clipStartX, _clipStartY;
	int _clipEndX, _clipEndY;

	public (int StartX, int StartY, int EndX, int EndY) GetSelectionRange()
		=> (_clipStartX, _clipStartY, _clipEndX, _clipEndY);

	public bool HasSelection => (_clipStartX != _clipEndX) || (_clipStartY != _clipEndY);

	public bool HasMultilineSelection => (_clipStartY != _clipEndY);
	public bool HasMultilineClipboardContent => clipboard.HasMultilineContent;

	public void StartSelection(int x, int y)
	{
		_clipStartX = x;
		_clipStartY = y;

		_clipEndX = x;
		_clipEndY = y;
	}

	public void ExtendSelection(int x, int y)
	{
		if (_clipStartX < 0)
			StartSelection(x, y);
		else
		{
			_clipEndX = x;
			_clipEndY = y;
		}
	}

	public void CancelSelection()
	{
		_clipStartX = _clipStartY = -1;
		_clipEndX = _clipEndY = -1;
	}

	public void ClearClipboard()
	{
		clipboard.Clear();
	}

	public void Cut() => CutCopy(retain: false, stash: true);
	public void Copy() => CutCopy(retain: true, stash: true);
	public void Delete() => CutCopy(retain: false, stash: false);

	void CutCopy(bool retain, bool stash)
	{
		if (!_owner.IsEditable && !retain)
		{
			CancelSelection();
			return;
		}

		if (!retain && stash)
			ClearClipboard();

		int effectiveStartX = Math.Min(_clipStartX, _clipEndX);
		int effectiveEndX = Math.Max(_clipStartX, _clipEndX);

		int effectiveStartY = Math.Min(_clipStartY, _clipEndY);
		int effectiveEndY = Math.Max(_clipStartY, _clipEndY);

		if ((effectiveStartX == 0) && (effectiveEndX == 0))
			effectiveEndY--;

		if (_clipStartY != _clipEndY)
		{
			if (stash)
				clipboard.ContentMultiLine = new List<CodeLine>();

			var contentBuffer = clipboard.ContentMultiLine;

			if (!stash)
				contentBuffer = null;

			var buffer = new StringWriter();
			var bufferBuilder = buffer.GetStringBuilder();

			int lineCount = 1 + effectiveEndY - effectiveStartY;

			for (int i = 0; i < lineCount; i++)
			{
				bufferBuilder.Length = 0;

				_owner.RenderLine(effectiveStartY, buffer);

				contentBuffer?.Add(
					CodeLine.CreateUnparsed(bufferBuilder.ToString()));

				if (retain)
					effectiveStartY++;
				else
					_owner.DeleteLine(effectiveStartY);
			}

			if (!retain)
			{
				_owner.CursorX = 0;
				_owner.CursorY = effectiveStartY;
			}
		}
		else if (effectiveStartX != effectiveEndX)
		{
			if (effectiveStartY != _owner.CursorY)
				throw new Exception("Internal error: Single-line selection is not on current line");

			var buffer = _owner.EditCurrentLine();

			int startX = Math.Min(effectiveStartX, effectiveEndX);
			int charCount = Math.Abs(effectiveEndX - effectiveStartX);

			if (startX < 0)
				startX = 0;
			if (startX > buffer.Length)
				startX = buffer.Length;

			int realChars = charCount;

			if (startX + realChars > buffer.Length)
				realChars = buffer.Length - startX;

			int virtualChars = charCount - realChars;

			if (charCount != 0)
			{
				if (stash)
				{
					if (virtualChars == 0)
						clipboard.ContentSingleLine = buffer.ToString(startX, charCount);
					else
					{
						var extended = new StringBuilder(charCount);

						extended.Append(buffer, startX, realChars);

						while (extended.Length < charCount)
							extended.Append(' ');

						clipboard.ContentSingleLine = extended.ToString();
					}
				}

				if (!retain)
				{
					buffer.Remove(startX, realChars);
					_owner.CursorX = startX;
					_owner.CurrentLineEdited = true;

					CancelSelection();
				}
			}
		}
	}

	public void Paste()
	{
		if (clipboard.ContentMultiLine != null)
		{
			try
			{
				_owner.CommitCurrentLine();
			}
			catch { } // No syntax checking here.

			for (int i = 0; i < clipboard.ContentMultiLine.Count; i++)
				_owner.InsertLine(_owner.CursorY + i, clipboard.ContentMultiLine[i]);
		}
		else
		{
			var buffer = _owner.EditCurrentLine();

			while (buffer.Length < _owner.CursorX)
				buffer.Append(' ');

			buffer.Insert(_owner.CursorX, clipboard.ContentSingleLine);

			_owner.CurrentLineEdited = true;
		}
	}

	public string GetSelectedText(bool multiline)
	{
		int effectiveStartX = Math.Min(_clipStartX, _clipEndX);
		int effectiveEndX = Math.Max(_clipStartX, _clipEndX);

		int effectiveStartY = Math.Min(_clipStartY, _clipEndY);
		int effectiveEndY = Math.Max(_clipStartY, _clipEndY);

		if ((effectiveStartX == 0) && (effectiveEndX == 0))
			effectiveEndY--;

		if (_clipStartY != _clipEndY)
		{
			if (!multiline)
				return "";

			int lineCount = 1 + effectiveEndY - effectiveStartY;

			var writer = new StringWriter();

			for (int i = 0; i < lineCount; i++)
			{
				_owner.RenderLine(effectiveStartY, writer);
				writer.WriteLine();

				effectiveStartY++;
			}

			return writer.ToString();
		}
		else if (effectiveStartX != effectiveEndX)
		{
			if (effectiveStartY != _owner.CursorY)
				throw new Exception("Internal error: Single-line selection is not on current line");

			var writer = new StringWriter();

			_owner.RenderLine(_owner.CursorY, writer);

			var selection = writer.GetStringBuilder();

			int startX = Math.Min(effectiveStartX, effectiveEndX);
			int charCount = Math.Abs(effectiveEndX - effectiveStartX);

			if (startX < 0)
				startX = 0;
			if (startX > selection.Length)
				startX = selection.Length;

			int realChars = charCount;

			if (startX + realChars > selection.Length)
				realChars = selection.Length - startX;

			int virtualChars = charCount - realChars;

			if (startX + charCount < selection.Length)
			{
				selection.Remove(
					startX + charCount,
					selection.Length - startX - charCount);
			}

			selection.Remove(
				0,
				startX);

			return selection.ToString();
		}
		else
			return "";

	}
}
