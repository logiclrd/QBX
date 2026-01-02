using QBX.CodeModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace QBX.DevelopmentEnvironment;

public class Clipboard(Viewport owner)
{
	Viewport _owner = owner;

	int _clipStartX, _clipStartY;
	int _clipEndX, _clipEndY;
	string? _clipboardContentSingleLine;
	List<CodeLine>? _clipboardContentMultiLine;

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

	public void Cut() => CutCopy(retain: false);
	public void Copy() => CutCopy(retain: true);

	void CutCopy(bool retain)
	{
		if (!_owner.IsEditable)
		{
			CancelSelection();
			return;
		}

		if (!retain)
			Clear();

		if (_clipStartY != _clipEndY)
		{
			_clipboardContentMultiLine = new List<CodeLine>();

			int lineCount = 1 + Math.Abs(_clipEndY - _clipStartX);
			int startY = Math.Min(_clipStartY, _clipEndY);

			for (int i = 0; i < lineCount; i++)
			{
				_clipboardContentMultiLine.Add(_owner.GetCodeLineAt(startY));

				if (retain)
					startY++;
				else
					_owner.DeleteLine(startY);
			}

			_owner.CursorX = 0;
		}
		else if (_clipStartX != _clipEndX)
		{
			if (_clipStartY != _owner.CursorY)
				throw new Exception("Internal error: Single-line selection is not on current line");

			var buffer = _owner.EditCurrentLine();

			int startX = Math.Min(_clipStartX, _clipEndX);
			int charCount = 1 + Math.Abs(_clipEndX - _clipStartX);

			if (startX < 0)
				startX = 0;
			if (startX > buffer.Length)
				startX = buffer.Length;
			if (startX + charCount > buffer.Length)
				charCount = buffer.Length - startX;

			if (charCount != 0)
			{
				_clipboardContentSingleLine = buffer.ToString(startX, charCount);

				if (!retain)
				{
					buffer.Remove(startX, charCount);
					_owner.CursorX = startX;
				}
			}

			CancelSelection();
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
		}
	}
}
