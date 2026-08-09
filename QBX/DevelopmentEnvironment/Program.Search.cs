using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;

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

		// Commit any changes immediately. The current element might be visible in both viewports.
		CommitViewportsAndSwallowError();

		if ((initialFindWhat?.Length == 1)
		 && !IsWordCharacter(initialFindWhat[0]))
			initialFindWhat = null;

		var searchScopeMode =
			FocusedViewport == HelpViewport
			? SearchScopeMode.HelpFile
			: SearchScopeMode.TextEditor;

		var dialog = new FindDialog(searchScopeMode, Machine, Configuration);

		if (_lastSearchParameters != null)
		{
			dialog.FindWhat = new StringValue(_lastSearchParameters.FindWhatString);
			dialog.MatchUpperLowercase = _lastSearchParameters.MatchUpperLowercase;
			dialog.WholeWord = _lastSearchParameters.WholeWord;
			dialog.SearchScope = _lastSearchParameters.SearchScope;
		}

		if (initialFindWhat != null)
			dialog.FindWhat = new StringValue(initialFindWhat);

		dialog.Find +=
			(searchParameters) =>
			{
				var origin = GetOriginFromCurrentCursorLocation();

				if (origin != null)
					Find(searchParameters, origin);
			};

		ShowDialog(dialog);
	}

	public void ShowChangeDialog()
		=> ShowChangeDialog(GetTokenUnderCursor());

	public void ShowChangeDialog(string? initialFindWhat = null, string? initialChangeTo = null)
	{
		if (FocusedViewport == ImmediateViewport)
			return;

		// Commit any changes immediately. The current element might be visible in both viewports.
		CommitViewportsAndSwallowError();

		if ((initialFindWhat?.Length == 1)
		 && !IsWordCharacter(initialFindWhat[0]))
			initialFindWhat = null;

		var searchScopeMode =
			FocusedViewport == HelpViewport
			? SearchScopeMode.HelpFile
			: SearchScopeMode.TextEditor;

		var dialog = new ChangeDialog(searchScopeMode, Machine, Configuration);

		if (_lastSearchParameters != null)
		{
			dialog.FindWhat = new StringValue(initialFindWhat ?? _lastSearchParameters.FindWhatString);
			dialog.ChangeTo = new StringValue(initialChangeTo ?? _lastSearchParameters.ChangeToString);
			dialog.MatchUpperLowercase = _lastSearchParameters.MatchUpperLowercase;
			dialog.WholeWord = _lastSearchParameters.WholeWord;
			dialog.SearchScope = _lastSearchParameters.SearchScope;
		}

		dialog.Change +=
			(searchParameters) =>
			{
				var origin = GetOriginFromCurrentCursorLocation();

				if (origin != null)
					Change(searchParameters, origin, showPrompt: true);
			};

		dialog.ChangeAll +=
			(searchParameters) =>
			{
				var origin = GetOriginFromCurrentCursorLocation();

				if (origin != null)
					Change(searchParameters, origin, showPrompt: false);
			};

		ShowDialog(dialog);
	}

	SearchParameters? _lastSearchParameters;
	SearchPosition? _lastFindResult;

	[MemberNotNullWhen(true, nameof(_lastFindResult))]
	public bool ForgetLastFindResult()
	{
		_lastFindResult = null;
		return false;
	}

	[MemberNotNullWhen(true, nameof(_lastFindResult))]
	public bool HaveLastFindResult()
		=> _lastFindResult != null;

	void ClampSearchPosition(SearchPositionIndex index)
	{
		if (index.HelpTopic != null)
		{
			if (!HelpSystem.ContainsTopic(index.HelpTopic))
				index.HelpTopic = HelpSystem.GetFirstTopic();

			if (index.HelpTopic == null)
				index.LineIndex = 0;
			else if (index.LineIndex >= index.HelpTopic.Lines.Count)
				index.LineIndex = index.HelpTopic.Lines.Count - 1;
		}
		else
		{
			index.LoadedFileIndex = index.LoadedFileIndex.Clamp(0, LoadedFiles.Count - 1);

			var unit = LoadedFiles[index.LoadedFileIndex];

			index.ElementIndex = index.ElementIndex.Clamp(0, unit.Elements.Count - 1);

			var element = unit.Elements[index.ElementIndex];

			index.LineIndex = index.LineIndex.Clamp(0, element.Lines.Count - 1);

			var buffer = new StringWriter();

			if (PrimaryViewport.EditableElement == element)
				PrimaryViewport.RenderLine(index.LineIndex, EditableLineState.Uncommitted, buffer);
			else if (SplitViewport?.EditableElement == element)
				SplitViewport?.RenderLine(index.LineIndex, EditableLineState.Uncommitted, buffer);
			else
				element.Lines[index.LineIndex].Render(buffer);

			index.CharacterOffset = index.CharacterOffset.Clamp(0, buffer.GetStringBuilder().Length - 1);
		}
	}

	SearchPositionIndex? GetOriginFromCurrentCursorLocation()
	{
		var origin = new SearchPositionIndex();

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
		origin.CharacterOffset = FocusedViewport.CursorX;

		return origin;
	}

	public void RepeatFind()
	{
		if (FocusedViewport == ImmediateViewport)
			return;

		if (_lastSearchParameters != null)
		{
			SearchPositionIndex? origin = null;

			if (HaveLastFindResult())
			{
				bool lastFindInHelp = (_lastFindResult.HelpTopic != null);
				bool thisFindInHelp = (FocusedViewport == HelpViewport);

				if (lastFindInHelp != thisFindInHelp)
					ForgetLastFindResult();
				else
				{
					// If we're sitting right on the last search result, start searching after it.
					// If we're not, then start searching from where the cursor is now.
					if ((_lastFindResult.Element == FocusedViewport.EditableElement)
					 && (_lastFindResult.LineIndex == FocusedViewport.CursorY)
					 && (_lastFindResult.CharacterOffset == FocusedViewport.CursorX))
					{
						origin = RebuildIndex(_lastFindResult, thisFindInHelp ? SearchScopeMode.HelpFile : SearchScopeMode.TextEditor);
						origin?.AdvanceCharacterOffset(_lastSearchParameters.FindWhatString.Length);
					}
				}
			}

			if (origin == null)
			{
				ForgetLastFindResult();
				origin = GetOriginFromCurrentCursorLocation();
			}

			if (origin != null)
				Find(_lastSearchParameters, origin);
		}
	}

	class SearchState(SearchPositionIndex index, SearchPosition position) : SearchParameters
	{
		public SearchPositionIndex Origin = index.Clone();

		public SearchPositionIndex Index = index;
		public SearchPosition Position = position;
		public bool IsWrapped;

		public void AdvanceCharacterOffset(int count = 1)
		{
			Index.AdvanceCharacterOffset(count);
			Position.AdvanceCharacterOffset(count);
		}
	}

	SearchPositionIndex? RebuildIndex(SearchPosition position, SearchScopeMode scopeMode)
	{
		var origin = new SearchPositionIndex();

		if (scopeMode == SearchScopeMode.HelpFile)
		{
			origin.HelpTopic = position.HelpTopic ?? HelpViewport?.HelpTopic ?? HelpSystem.GetFirstTopic();
			origin.LineIndex = position.LineIndex;
		}
		else
		{
			if ((position.Unit == null)
			 || (position.Element == null))
				return null;

			origin.LoadedFileIndex = LoadedFiles.IndexOf(position.Unit);

			if (origin.LoadedFileIndex < 0)
				return null;

			origin.ElementIndex = position.Unit.Elements.IndexOf(position.Element);

			if (origin.ElementIndex < 0)
				return null;

			origin.LineIndex = position.Element.Lines.IndexOf(position.Line);

			if (origin.LineIndex < 0)
				origin.LineIndex = position.LineIndex;
		}

		origin.CharacterOffset = position.CharacterOffset;

		return origin;
	}

	SearchState InitializeSearchState(SearchPositionIndex index)
	{
		var position = new SearchPosition();

		if (index.HelpTopic != null)
			position.HelpTopic = index.HelpTopic;
		else
		{
			if ((index.LoadedFileIndex >= 0) && (index.LoadedFileIndex < LoadedFiles.Count))
			{
				position.Unit = LoadedFiles[index.LoadedFileIndex];

				if ((index.ElementIndex >= 0) && (index.ElementIndex < position.Unit.Elements.Count))
				{
					position.Element = position.Unit.Elements[index.ElementIndex];

					if ((index.LineIndex >= 0) && (index.LineIndex < position.Element.Lines.Count))
						position.Line = position.Element.Lines[index.LineIndex];
				}
			}
		}

		position.LineIndex = index.LineIndex;
		position.CharacterOffset = index.CharacterOffset;

		return new SearchState(index.Clone(), position);
	}

	void AdvanceSearchState(SearchState state)
	{
		state.Index.LineIndex++;
		state.Position.LineIndex++;

		state.Index.CharacterOffset = 0;
		state.Position.CharacterOffset = 0;

		if (state.Position.HelpTopic != null)
		{
			if (state.Index.LineIndex >= state.Position.HelpTopic.Lines.Count)
			{
				state.Index.LineIndex = 0;
				state.Position.LineIndex = 0;

				switch (state.SearchScope)
				{
					case SearchScope.ActiveWindow:
						state.IsWrapped = true;
						break;
					case SearchScope.HelpFile:
						var thisTopic = state.Position.HelpTopic;

						var database = thisTopic.Database;

						var nextTopic = database.GetNextTopic(thisTopic);

						if (nextTopic?.TopicIndex < thisTopic.TopicIndex)
							state.IsWrapped = true;

						state.Position.HelpTopic = nextTopic;

						break;
				}
			}
		}
		else
		{
			if ((state.Position.Element == null)
			 || (state.Position.Unit == null))
				throw new Exception("Internal error");

			if (state.Index.LineIndex >= state.Position.Element.Lines.Count)
			{
				state.Index.LineIndex = 0;
				state.Position.LineIndex = 0;

				if (state.SearchScope == SearchScope.ActiveWindow)
					state.IsWrapped = true;
				else
				{
					state.Index.ElementIndex++;
					state.Position.Element = null;

					if (state.Index.ElementIndex >= state.Position.Unit.Elements.Count)
					{
						state.Index.ElementIndex = 0;

						if (state.SearchScope == SearchScope.CurrentModule)
							state.IsWrapped = true;
						else
						{
							state.Position.Unit = null;

							state.Index.LoadedFileIndex++;

							if (state.Index.LoadedFileIndex >= LoadedFiles.Count)
							{
								state.Index.LoadedFileIndex = 0;
								state.IsWrapped = true;
							}

							if (state.Index.LoadedFileIndex < LoadedFiles.Count)
								state.Position.Unit = LoadedFiles[state.Index.LoadedFileIndex];
						}
					}
				}

				if ((state.Position.Unit != null)
				 && (state.Index.ElementIndex < state.Position.Unit.Elements.Count))
					state.Position.Element = state.Position.Unit.Elements[state.Index.ElementIndex];
			}

			if ((state.Position.Element != null)
			 && (state.Index.LineIndex < state.Position.Element.Lines.Count))
				state.Position.Line = state.Position.Element.Lines[state.Index.LineIndex];
		}
	}

	void RenderSearchPositionLine(SearchPosition position, TextWriter writer)
	{
		if (position.HelpTopic != null)
		{
			if (position.LineIndex < position.HelpTopic.Lines.Count)
				position.HelpTopic.Lines[position.LineIndex].RenderPlainText(writer);
		}
		else if (PrimaryViewport.EditableElement == position.Element)
			PrimaryViewport.RenderLine(position.LineIndex, EditableLineState.Uncommitted, writer);
		else if (SplitViewport?.EditableElement == position.Element)
			SplitViewport?.RenderLine(position.LineIndex, EditableLineState.Uncommitted, writer);
		else if (position.Line != null)
			position.Line.Render(writer);
	}

	enum SearchAction
	{
		Continue,
		Exit,
	}

	void PerformSearch(SearchParameters searchParameters, SearchPositionIndex origin, Func<SearchPosition, SearchState, SearchAction> onMatch)
	{
		_lastSearchParameters = searchParameters;

		UpdateSearchMenu();

		ClampSearchPosition(origin);

		var searchState = InitializeSearchState(origin);

		searchParameters.CopyTo(searchState);

		searchState.InitializeComparer();

		ContinueSearch(searchState, onMatch);
	}

	void ContinueSearch(SearchState searchState, Func<SearchPosition, SearchState, SearchAction> onMatch)
	{
		if (searchState.FindWhatString.Length == 0)
			return; // just in case

		var writer = new CharacterArrayWriter();

		ForgetLastFindResult();

		do
		{
			writer.Clear();

			RenderSearchPositionLine(searchState.Position, writer);

			var lineSpan = writer.GetBuffer();

			bool findWhatStartIsWord = IsWordCharacter(searchState.FindWhatString[0]);
			bool findWhatEndIsWord = IsWordCharacter(searchState.FindWhatString[searchState.FindWhatString.Length - 1]);

			if (searchState.Position.CharacterOffset > 0)
			{
				if (searchState.WholeWord)
				{
					// Skip forward until we find a word boundary. With the current architecture,
					// we can't peek backwards, so if we do a search and find a match at
					// matchIndex == 0, then unless we set up this invariant, we're in a pickle;
					// we don't know whether the start of the match is a word boundary.

					while (searchState.Position.CharacterOffset < lineSpan.Length)
					{
						bool precedingCharacterIsWord = IsWordCharacter(lineSpan[searchState.Position.CharacterOffset - 1]);

						bool startIsCandidate = (precedingCharacterIsWord != findWhatStartIsWord);

						if (startIsCandidate)
							break;

						searchState.AdvanceCharacterOffset();
					}
				}

				lineSpan = lineSpan.Slice(searchState.Position.CharacterOffset);
			}

			int matchIndex = -1;

			while (lineSpan.Length >= searchState.FindWhatString.Length)
			{
				matchIndex = lineSpan.IndexOf(searchState.FindWhatString, searchState.Comparer);

				if (!searchState.WholeWord)
					break;

				if (matchIndex < 0)
					break;

				if (matchIndex > 0) // we have already ruled out index 0
				{
					// Whole Word is enabled and there are characters before and after the current match.
					// Make sure the start and end of the current match are word boundaries.

					bool haveNextCharacter = (matchIndex + searchState.FindWhatString.Length < lineSpan.Length);

					bool precedingCharacterIsWord = IsWordCharacter(lineSpan[matchIndex - 1]);
					bool nextCharacterIsWord =
						haveNextCharacter
						? IsWordCharacter(lineSpan[matchIndex + searchState.FindWhatString.Length])
						: !findWhatEndIsWord; // short circuit to success at end of string

					// Only keep this match if the start & end of the match are a word boundaries.
					if ((precedingCharacterIsWord != findWhatStartIsWord)
					 && (nextCharacterIsWord != findWhatEndIsWord))
						break;
				}

				// There's an algorithm for figuring out how many characters we can skip
				// (more than one in a go), but it requires a bunch of set-up. :-P
				lineSpan = lineSpan.Slice(matchIndex + 1);
				searchState.AdvanceCharacterOffset(matchIndex + 1);

				matchIndex = -1; // discard this match
			}

			if (matchIndex < 0)
				AdvanceSearchState(searchState); // next line
			else
			{
				searchState.Position.CharacterOffset += matchIndex;
				searchState.Index.CharacterOffset += matchIndex;

				_lastFindResult = searchState.Position.Clone();
				_lastFindResult.Length = searchState.FindWhatString.Length;

				var action = onMatch(_lastFindResult, searchState);

				if (action == SearchAction.Exit)
					break;
			}
		}
		while ((searchState.IsWrapped == false) || (searchState.Index < searchState.Origin));

		if (_lastFindResult == null)
			ShowDialog(new MatchNotFoundDialog(Machine, Configuration));
	}

	public void Find(SearchParameters searchParameters, SearchPositionIndex origin)
	{
		SearchAction OnMatch(SearchPosition result, SearchState state)
		{
			NavigateToSearchResult(result);

			return SearchAction.Exit;
		}

		PerformSearch(searchParameters, origin, OnMatch);
	}

	public void Change(SearchParameters searchParameters, SearchPositionIndex origin, bool showPrompt)
	{
		SearchAction OnMatch(SearchPosition result, SearchState searchState)
		{
			if (showPrompt)
			{
				NavigateToSearchResult(result);

				ShowChangeMatchDialog(
					result,
					searchState,
					findNext:
						() =>
						{
							searchState.AdvanceCharacterOffset(searchState.FindWhatString.Length);
							ContinueSearch(searchState, OnMatch);
						});

				return SearchAction.Exit;
			}
			else
			{
				ApplyChange(result, searchState);

				return SearchAction.Continue;
			}
		}

		PerformSearch(searchParameters, origin, OnMatch);

		if (_lastFindResult == null)
			ShowDialog(new MatchNotFoundDialog(Machine, Configuration));
	}

	void ApplyChange(SearchPosition result, SearchState state)
	{
		if ((result == null)
		 || (result.Element is not IEditableElement element)
		 || (result.Line is not IEditableLine line))
			return;

		StringBuilder buffer;

		try
		{
			PrimaryViewport.CommitCurrentLine();
		}
		catch { }

		try
		{
			SplitViewport?.CommitCurrentLine();
		}
		catch { }

		if (FocusedViewport.EditableElement == element)
		{
			if (result.LineIndex == FocusedViewport.CursorY)
			{
				buffer = FocusedViewport.EditCurrentLine();

				FocusedViewport.CurrentLineEdited = true;
			}
			else
			{
				buffer = new StringBuilder();

				FocusedViewport.RenderLine(result.LineIndex, EditableLineState.Uncommitted, new StringWriter(buffer));
			}
		}
		else
		{
			buffer = new StringBuilder();

			line.Render(new StringWriter(buffer), includeCRLF: false);
		}

		buffer.Remove(result.CharacterOffset, result.Length);
		buffer.Insert(result.CharacterOffset, state.ChangeToString);

		if (state.Origin.IsSameLineAs(state.Index)
		 && (state.Origin.CharacterOffset > state.Index.CharacterOffset + state.ChangeToString.Length))
		{
			// We've just bumped the origin by editing the start of the line it's on.
			state.Origin.AdvanceCharacterOffset(state.ChangeToString.Length - state.FindWhatString.Length);
		}

		// The regular loop will advance by FindWhatString.Length. If the
		// current occurrence is skipped then the FindWhat string is what's
		// there. But if we came down this path, we've changed it to
		// ChangeToString. We ultimately want to advance by ChangeToString.Length
		// but we need to cancel out the main loop's FindWhatString.Length
		// advancement.
		state.AdvanceCharacterOffset(state.ChangeToString.Length - state.FindWhatString.Length);

		if (FocusedViewport.EditableElement == element)
		{
			FocusedViewport.SelectionManager.CancelSelection();

			// Commit any changes immediately. The current element might be visible in both viewports.
			try
			{
				FocusedViewport.CommitCurrentLine();
			}
			catch { }

			if (FocusedViewport.CursorY != result.LineIndex)
				element.ReplaceLine(result.LineIndex, element.ConstructLine(buffer));
		}
	}

	void ShowChangeMatchDialog(SearchPosition result, SearchState state, Action findNext)
	{
		var dialog = new ChangeMatchDialog(Machine, Configuration);

		dialog.PerformChange += () => ApplyChange(result, state);
		dialog.FindNext += findNext;

		ShowDialog(dialog);
	}

	public void NavigateToSearchResult(SearchPosition result)
	{
		void ScrollSearchResultIntoView(Viewport viewport)
		{
			viewport.ScrollCursorIntoView(
				newCursorX: result.CharacterOffset,
				newCursorY: result.LineIndex,
				newScrollX: viewport.ScrollX,
				newScrollY: viewport.ScrollY,
				ViewportPositioningPriority.Cursor,
				viewportWidth: TextLibrary.Width - 2,
				terminateToCommitEdit: null);

			viewport.SelectionManager.StartSelection(
				viewport.CursorX, viewport.CursorY);
			viewport.SelectionManager.ExtendSelection(
				viewport.CursorX + result.Length, viewport.CursorY);
		}

		if (result.HelpTopic != null)
		{
			ShowHelpTopic(result.HelpTopic);

			if (HelpViewport != null)
			{
				HelpViewport.CancelEdit();
				ScrollSearchResultIntoView(HelpViewport);
			}
		}
		else if (result.Element != null)
		{
			try
			{
				FocusedViewport?.CommitCurrentLine();
			}
			catch { }

			NavigateTo(
				result.Element,
				result.LineIndex,
				result.CharacterOffset);

			if (FocusedViewport != null)
				ScrollSearchResultIntoView(FocusedViewport);
		}
	}
}
