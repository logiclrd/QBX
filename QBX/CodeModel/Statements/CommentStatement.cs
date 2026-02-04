using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using QBX.ExecutionEngine;

namespace QBX.CodeModel.Statements;

public class CommentStatement(CommentStatementType type, string comment) : Statement
{
	public override StatementType Type => StatementType.Comment;

	public CommentStatementType CommentStatementType { get; set; } = type;
	public string Comment { get; set; } = comment;

	static readonly Dictionary<string, string> Metacommands =
		new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
		{
			{ "$STATIC", "$STATIC" },
			{ "$DYNAMIC", "$DYNAMIC" },
			{ "$INCLUDE", "$INCLUDE" },
		};

	public static ReadOnlySpan<char> FormatCommentText(ReadOnlySpan<char> commentText)
	{
		bool IsSpace(char ch) => (ch == ' ') || (ch == '\t');

		var span = commentText;

		int directiveCharacter = span.IndexOf('$');

		if (directiveCharacter < 0)
			return span;

		for (int i = 0; i < directiveCharacter; i++)
			if (!IsSpace(span[i]))
				return span;

		StringBuilder? buffer = null;
		int matchPrefixLength = directiveCharacter;

		span = span.Slice(directiveCharacter);

		int directiveStart = span.IndexOf('$');
		bool precedingIsDirective = false;

		while (directiveStart >= 0)
		{
			int directiveEnd = directiveStart + 1;

			while ((directiveEnd < span.Length) && char.IsAsciiLetterOrDigit(span[directiveEnd]))
				directiveEnd++;

			if (buffer == null)
				matchPrefixLength += directiveStart;
			else
				buffer.Append(span.Slice(0, directiveStart));

			var directive = span.Slice(directiveStart, directiveEnd - directiveStart).ToString();

			span = span.Slice(directiveEnd);

			char precedingCharacter =
				buffer != null
				? buffer[buffer.Length - 1]
				: matchPrefixLength > 0
					? commentText[matchPrefixLength - 1]
					: '\0';

			if (Metacommands.TryGetValue(directive, out var canonicalDirective))
			{
				if (precedingIsDirective)
				{
					if (directiveStart == 0)
					{
						buffer ??= new StringBuilder().Append(commentText.Slice(0, matchPrefixLength));
						buffer.Append(' ');
					}
				}

				if ((buffer == null) && directive.Equals(canonicalDirective, StringComparison.Ordinal))
					matchPrefixLength += canonicalDirective.Length;
				else
				{
					buffer ??= new StringBuilder().Append(commentText.Slice(0, matchPrefixLength));
					buffer.Append(canonicalDirective);
				}

				if (canonicalDirective != "$INCLUDE")
					precedingIsDirective = true;
				else
				{
					// Elide spaces between $INCLUDE and :
					while ((span.Length > 0) && IsSpace(span[0]))
					{
						buffer ??= new StringBuilder().Append(commentText.Slice(0, matchPrefixLength));
						span = span.Slice(1);
					}

					if ((span.Length == 0) || (span[0] != ':'))
						throw CompilerException.MetacommandError();

					if (buffer == null)
						matchPrefixLength++;
					else
						buffer.Append(':');

					span = span.Slice(1);

					// Ensure one space between : and filename
					if ((buffer == null) && (span.Length > 0) && IsSpace(span[0]))
					{
						matchPrefixLength++;
						span = span.Slice(1);
					}
					else
					{
						buffer ??= new StringBuilder().Append(commentText.Slice(0, matchPrefixLength));
						buffer.Append(' ');
					}

					while ((span.Length > 0) && IsSpace(span[0]))
					{
						span = span.Slice(1);
						buffer ??= new StringBuilder().Append(commentText.Slice(0, matchPrefixLength));
					}

					if ((span.Length == 0) || (span[0] != '\''))
						throw CompilerException.MetacommandError();

					if (buffer == null)
						matchPrefixLength++;
					else
						buffer.Append('\'');

					span = span.Slice(1);

					int endQuote = span.IndexOf('\'');

					if (endQuote < 0)
						throw CompilerException.MetacommandError();

					// As long as we have the quoted string, we're good. Everything
					// after the end quote is ignored.
					break;
				}
			}
			else
			{
				// Nearly fooled me, but that's not a directive!
				if (buffer == null)
					matchPrefixLength += directive.Length;
				else
					buffer.Append(directive);

				precedingIsDirective = false;
			}

			directiveStart = span.IndexOf('$');
		}

		if (buffer == null)
			return commentText; // Yey, no changes!
		else
			return buffer.Append(span).ToString();
	}

	protected override void RenderImplementation(TextWriter writer)
	{
		if (CommentStatementType == CommentStatementType.REM)
		{
			writer.Write("REM");

			if (Comment.Length > 0 && !string.IsNullOrWhiteSpace(Comment))
				writer.Write(' ');
		}
		else
			writer.Write('\'');

		var commentSpan = Comment.AsSpan();

		var reformattedCommentSpan = FormatCommentText(commentSpan);

		if (reformattedCommentSpan != commentSpan)
			Comment = reformattedCommentSpan.ToString();

		writer.Write(Comment);
	}
}
