using System.Diagnostics.CodeAnalysis;

using QBX.DevelopmentEnvironment.Dialogs;
using QBX.DevelopmentEnvironment.Help;
using QBX.ExecutionEngine.Execution;
using QBX.Utility;

namespace QBX.DevelopmentEnvironment;

public partial class Program
{
	public void ShowFindDialog()
		=> ShowFindDialog(GetTokenUnderCursor());

	public void ShowFindDialog(string? initialFindWhat = null)
	{
		if (FocusedViewport == ImmediateViewport)
			return;

		var searchScopeMode =
			FocusedViewport == HelpViewport
			? SearchScopeMode.HelpFile
			: SearchScopeMode.TextEditor;

		var dialog = new FindDialog(searchScopeMode, Machine, Configuration);

		dialog.FindWhat =
			initialFindWhat != null
			? new StringValue(initialFindWhat)
			: (_lastFindWhat ?? new StringValue());

		dialog.Find +=
			(findWhat, searchScope) =>
			{
				var origin = GetOriginFromCurrentCursorLocation();

				if (origin != null)
					PerformFind(findWhat, searchScope, origin);
			};

		ShowDialog(dialog);
	}

	public void ShowChangeDialog()
		=> ShowChangeDialog(GetTokenUnderCursor());

	public void ShowChangeDialog(string? initialFindWhat = null, string? initialChangeTo = null)
	{
		if (FocusedViewport == ImmediateViewport)
			return;

		var searchScopeMode =
			FocusedViewport == HelpViewport
			? SearchScopeMode.HelpFile
			: SearchScopeMode.TextEditor;

		var dialog = new ChangeDialog(searchScopeMode, Machine, Configuration);

		dialog.FindWhat =
			initialFindWhat != null
			? new StringValue(initialFindWhat)
			: (_lastFindWhat ?? new StringValue());

		dialog.ChangeTo =
			initialChangeTo != null
			? new StringValue(initialChangeTo)
			: (_lastChangeTo ?? new StringValue());

		dialog.Change +=
			(findWhat, changeTo, searchScope) =>
			{
				var origin = GetOriginFromCurrentCursorLocation();

				if (origin != null)
					PerformChange(findWhat, changeTo, searchScope, origin, showPrompt: true);
			};

		dialog.ChangeAll +=
			(findWhat, changeTo, searchScope) =>
			{
				var origin = GetOriginFromCurrentCursorLocation();

				if (origin != null)
					PerformChange(findWhat, changeTo, searchScope, origin, showPrompt: false);
			};

		ShowDialog(dialog);
	}

	public class SearchResult
	{
		public IEditableUnit? Unit;
		public IEditableElement? Element;
		public IEditableLine? Line;

		public HelpDatabaseTopic? HelpTopic;

		public int LineIndex;
	}

	StringValue? _lastFindWhat;
	StringValue? _lastChangeTo;
	SearchScope _lastSearchScope = SearchScope.CurrentModule;
	SearchResult? _lastFindResult;

	[MemberNotNullWhen(true, nameof(_lastFindResult))]
	public bool ForgetLastFindResult()
	{
		_lastFindResult = null;
		return false;
	}

	[MemberNotNullWhen(true, nameof(_lastFindResult))]
	public bool HaveLastFindResult()
		=> _lastFindResult != null;

	SearchOrigin? GetOriginFromCurrentCursorLocation()
	{
		var origin = new SearchOrigin();

		if (FocusedViewport == HelpViewport)
			origin.HelpTopic = HelpViewport.HelpTopic;
		else
		{
			var focusedElement = FocusedViewport.EditableElement;

			if (focusedElement == null)
				return null;

			origin.LoadedFileIndex = LoadedFiles.IndexOf(focusedElement.Owner);
			origin.ElementIndex = focusedElement.Owner.Elements.IndexOf(focusedElement);
		}

		origin.LineIndex = FocusedViewport.CursorY;

		return origin;
	}

	public void RepeatFind()
	{
		if (FocusedViewport == ImmediateViewport)
			return;

		if (_lastFindWhat != null)
		{
			SearchOrigin? origin = null;

			if (HaveLastFindResult())
			{
				bool lastFindInHelp = (_lastFindResult.HelpTopic != null);
				bool thisFindInHelp = (FocusedViewport == HelpViewport);

				if (lastFindInHelp != thisFindInHelp)
					ForgetLastFindResult();
				else
				{
					origin = new SearchOrigin();

					if (thisFindInHelp)
						origin.HelpTopic = _lastFindResult.HelpTopic ?? HelpViewport?.HelpTopic ?? HelpSystem.GetFirstTopic();
					else
					{
						if ((_lastFindResult.Unit == null)
						 || (_lastFindResult.Element == null))
							ForgetLastFindResult();
						else
						{
							origin.LoadedFileIndex = LoadedFiles.IndexOf(_lastFindResult.Unit);

							if (origin.LoadedFileIndex < 0)
								ForgetLastFindResult();
							else
							{
								origin.ElementIndex = _lastFindResult.Unit.Elements.IndexOf(_lastFindResult.Element);

								if (origin.ElementIndex < 0)
									ForgetLastFindResult();
								else
								{
									origin.LineIndex = _lastFindResult.Element.Lines.IndexOf(_lastFindResult.Line);

									if (origin.LineIndex < 0)
										origin.LineIndex = _lastFindResult.LineIndex;
								}
							}
						}
					}

					if (HaveLastFindResult())
						origin.LineIndex = _lastFindResult.LineIndex;
				}
			}

			if (!HaveLastFindResult())
				origin = GetOriginFromCurrentCursorLocation();

			if (origin != null)
				PerformFind(_lastFindWhat, _lastSearchScope, origin);
		}
	}

	public void PerformFind(StringValue findWhat, SearchScope searchScope, SearchOrigin origin)
	{
		_lastFindWhat = findWhat;
		_lastSearchScope = searchScope;

		UpdateSearchMenu();

		// TODO
	}

	public void PerformChange(StringValue findWhat, StringValue changeTo, SearchScope searchScope, SearchOrigin origin, bool showPrompt)
	{
		_lastFindWhat = findWhat;
		_lastChangeTo = changeTo;
		_lastSearchScope = searchScope;

		UpdateSearchMenu();

		// TODO:
		// if (showPrompt)
		//   show change result dialog, repeat until finished or cancelled
		// else
		//   just do them all
	}

	public void NavigateToSearchResult(SearchResult result)
	{
		if (result.HelpTopic != null)
		{
		}
	}
}

