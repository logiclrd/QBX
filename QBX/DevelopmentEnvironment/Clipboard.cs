using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using QBX.CodeModel;

namespace QBX.DevelopmentEnvironment;

public class Clipboard(Viewport owner)
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
	string? _clipboardContentSingleLine;
	List<CodeLine>? _clipboardContentMultiLine;

	public (int StartX, int StartY, int EndX, int EndY) GetSelectionRange()
		=> (_clipStartX, _clipStartY, _clipEndX, _clipEndY);

	public bool HasSelection => (_clipStartX != _clipEndX) || (_clipStartY != _clipEndY);

	public void StartSelection(int x, int y)
	{
		_clipStartX = x;
		_clipStartY = y;

		_clipEndX = x;
		_clipEndY = y;
	}

	public void ExtendSelection(int x, int y)
	{
		_clipEndX = x;
		_clipEndY = y;
	}

	public void CancelSelection()
	{
		_clipStartX = _clipStartY = -1;
		_clipEndX = _clipEndY = -1;
	}

	public void Clear()
	{
		_clipboardContentMultiLine = null;
		_clipboardContentSingleLine = null;
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
			Clear();

		int effectiveStartX = Math.Min(_clipStartX, _clipEndX);
		int effectiveEndX = Math.Max(_clipStartX, _clipEndX);

		int effectiveStartY = Math.Min(_clipStartY, _clipEndY);
		int effectiveEndY = Math.Max(_clipStartY, _clipEndY);

		if ((effectiveStartX == 0) && (effectiveEndX == 0))
			effectiveEndY--;

		if (_clipStartY != _clipEndY)
		{
			_clipboardContentMultiLine = stash ? new List<CodeLine>() : null;

			int lineCount = 1 + effectiveEndY - effectiveStartY;

			for (int i = 0; i < lineCount; i++)
			{
				_clipboardContentMultiLine?.Add(_owner.GetCodeLineAt(effectiveStartY));

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
						_clipboardContentSingleLine = buffer.ToString(startX, charCount);
					else
					{
						var extended = new StringBuilder(charCount);

						extended.Append(buffer, startX, realChars);

						while (extended.Length < charCount)
							extended.Append(' ');

						_clipboardContentSingleLine = extended.ToString();
					}
				}

				if (!retain)
				{
					buffer.Remove(startX, realChars);
					_owner.CursorX = startX;
					_owner.CurrentLineChanged = true;

					CancelSelection();
				}
			}
		}
	}

	public void Paste()
	{
		if (_clipboardContentMultiLine != null)
		{
			_owner.CommitCurrentLine();

			for (int i = 0; i < _clipboardContentMultiLine.Count; i++)
				_owner.InsertLine(_owner.CursorY + i, _clipboardContentMultiLine[i]);
		}
		else
		{
			var buffer = _owner.EditCurrentLine();

			buffer.Insert(_owner.CursorX, _clipboardContentSingleLine);

			_owner.CurrentLineChanged = true;
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
				_owner.GetCodeLineAt(effectiveStartY).Render(writer);
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
