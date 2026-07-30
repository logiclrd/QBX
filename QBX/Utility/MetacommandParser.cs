using System;
using System.Collections.Generic;

namespace QBX.Utility;

public class MetacommandParser
{
	public static IEnumerable<string> ParseDirectives(string commentText)
	{
		var commentMemory = commentText.AsMemory();
		var commentSpan = commentMemory.Span;

		bool IsSpace(char ch) => (ch == ' ') || (ch == '\t');

		while ((commentSpan.Length > 0) && IsSpace(commentSpan[0]))
		{
			commentSpan = commentSpan.Slice(1);
			commentMemory = commentMemory.Slice(1);
		}

		// Metacommand comments must start with $.
		if ((commentSpan.Length == 0) || (commentSpan[0] != '$'))
			yield break;

		int directiveIndex = commentSpan.IndexOf('$');

		while (directiveIndex >= 0)
		{
			commentSpan = commentSpan.Slice(directiveIndex);
			commentMemory = commentMemory.Slice(directiveIndex);

			int directiveEnd = 1;

			while ((directiveEnd < commentSpan.Length) && char.IsAsciiLetterOrDigit(commentSpan[directiveEnd]))
				directiveEnd++;

			var directive = commentSpan.Slice(0, directiveEnd);

			commentSpan = commentSpan.Slice(directiveEnd);
			commentMemory = commentMemory.Slice(directiveEnd);

			yield return new string(directive);

			// Reload the span following the coroutine transition.
			commentSpan = commentMemory.Span;

			directiveIndex = commentSpan.IndexOf('$');
		}
	}
}
