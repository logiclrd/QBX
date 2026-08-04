using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using QBX.CodeModel;
using QBX.DevelopmentEnvironment.Help;

namespace QBX.DevelopmentEnvironment;

public partial class Program
{
	Dictionary<int, BookMark> _bookMarks = new();

	public BookMark CaptureBookMark()
	{
		var bookMark = new BookMark();

		if (FocusedViewport == HelpViewport)
		{
			bookMark.TargetType = BookMarkTargetType.HelpTopic;
			bookMark.HelpTopic = HelpViewport.HelpTopic;
		}
		else if (FocusedViewport == ImmediateViewport)
			bookMark.TargetType = BookMarkTargetType.ImmediateViewport;
		else
		{
			bookMark.TargetType = BookMarkTargetType.TextEditor;
			bookMark.Unit = FocusedViewport.EditableUnit;
			bookMark.Element = FocusedViewport.EditableElement;
		}

		bookMark.CursorX = FocusedViewport.CursorX;
		bookMark.CursorY = FocusedViewport.CursorY;

		return bookMark;
	}

	public void CaptureBookMark(int id)
	{
		SetBookMark(id, CaptureBookMark());
	}

	public void SetBookMark(int id, BookMark bookMark)
	{
		_bookMarks[id] = bookMark;
	}

	public void NavigateToBookMark(int id)
	{
		if (!_bookMarks.TryGetValue(id, out var bookMark))
			Machine.DOS.Beep();
		else if (!NavigateToBookMark(bookMark))
			_bookMarks.Remove(id);
	}

	public bool NavigateToBookMark(BookMark bookMark)
	{
		if (Dialogs.Any())
			return true;

		try
		{
			FocusedViewport.CommitCurrentLine();
		}
		catch { }

		switch (bookMark.TargetType)
		{
			case BookMarkTargetType.TextEditor:
				if ((bookMark.Unit == null)
				 || !LoadedFiles.Contains(bookMark.Unit)
				 || !(bookMark.Element is IEditableElement element)
				 || !bookMark.Unit.Elements.Contains(element))
				{
					Machine.DOS.Beep();
					return false;
				}

				NavigateTo(element, bookMark.CursorY, bookMark.CursorX);

				return true;
			case BookMarkTargetType.HelpTopic:
				if (!(bookMark.HelpTopic is HelpDatabaseTopic helpTopic))
				{
					Machine.DOS.Beep();
					return false;
				}

				ShowHelpTopic(helpTopic);

				if (HelpViewport != null)
				{
					HelpViewport.ScrollCursorIntoView(
						bookMark.CursorX,
						bookMark.CursorY,
						newScrollX: 0,
						newScrollY: bookMark.CursorY - 4,
						ViewportPositioningPriority.Cursor,
						viewportWidth: 78,
						terminateToCommitEdit: null);
				}

				return true;
			case BookMarkTargetType.ImmediateViewport:
				FocusedViewport = ImmediateViewport;

				ImmediateViewport.ScrollCursorIntoView(
					bookMark.CursorX,
					bookMark.CursorY,
					newScrollX: 0,
					newScrollY: bookMark.CursorY - 4,
					ViewportPositioningPriority.Cursor,
					viewportWidth: 78,
					terminateToCommitEdit: null);

				return true;
		}

		return false;
	}
}
