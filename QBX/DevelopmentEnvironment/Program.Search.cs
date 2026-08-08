using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;

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

	StringValue? _lastFindWhat;
	StringValue? _lastChangeTo;
	SearchScope _lastSearchScope = SearchScope.CurrentModule;
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

			var line = element.Lines[index.LineIndex];

			var buffer = new StringWriter();

			line.Render(buffer);

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

		return origin;
	}

	public void RepeatFind()
	{
		if (FocusedViewport == ImmediateViewport)
			return;

		if (_lastFindWhat != null)
		{
			SearchPositionIndex? origin = null;

			if (HaveLastFindResult())
			{
				bool lastFindInHelp = (_lastFindResult.HelpTopic != null);
				bool thisFindInHelp = (FocusedViewport == HelpViewport);

				if (lastFindInHelp != thisFindInHelp)
					ForgetLastFindResult();
				else
					origin = RebuildIndex(_lastFindResult, thisFindInHelp ? SearchScopeMode.HelpFile : SearchScopeMode.TextEditor);
			}

			if (origin == null)
			{
				ForgetLastFindResult();
				origin = GetOriginFromCurrentCursorLocation();
			}

			if (origin != null)
				PerformFind(_lastFindWhat, _lastSearchScope, origin);
		}
	}

	class SearchState(SearchPositionIndex index, SearchPosition position)
	{
		public SearchPositionIndex Index = index;
		public SearchPosition Position = position;
		public bool IsWrapped;
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

		return new SearchState(index, position);
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
				state.Position.HelpTopic = HelpSystem.GetNextTopic(state.Position.HelpTopic);
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

				state.Index.ElementIndex++;
				state.Position.Element = null;

				if (state.Index.ElementIndex >= state.Position.Unit.Elements.Count)
				{
					state.Index.ElementIndex = 0;
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
		else if (position.Line != null)
			position.Line.Render(writer);
	}


	public void PerformFind(StringValue findWhat, SearchScope searchScope, SearchPositionIndex origin)
	{
		_lastFindWhat = findWhat;
		_lastSearchScope = searchScope;

		UpdateSearchMenu();

		ClampSearchPosition(origin);

		var searchState = InitializeSearchState(origin.Clone());

		string findWhatString = findWhat.ToString();

		var writer = new CharacterArrayWriter();

		do
		{
			writer.Clear();

			RenderSearchPositionLine(searchState.Position, writer);

			var lineSpan = writer.GetBuffer();

			if (searchState.Position.CharacterOffset > 0)
				lineSpan = lineSpan.Slice(searchState.Position.CharacterOffset);

			int matchIndex = lineSpan.IndexOf(findWhatString);

			if (matchIndex >= 0)
			{
				matchIndex += searchState.Position.CharacterOffset;

				var result = searchState.Position.Clone();

				NavigateToSearchResult(result);

				break;
			}

			AdvanceSearchState(searchState);
		}
		while ((searchState.IsWrapped == false) || (searchState.Index < origin));

		ShowDialog(new MatchNotFoundDialog(Machine, Configuration));
	}

	public void PerformChange(StringValue findWhat, StringValue changeTo, SearchScope searchScope, SearchPositionIndex origin, bool showPrompt)
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

	public void NavigateToSearchResult(SearchPosition result)
	{
		if (result.HelpTopic != null)
		{
		}
	}
}

