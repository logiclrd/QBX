using System;
using System.Collections.Generic;
using System.IO;

using QBX.ExecutionEngine;

namespace QBX.CodeModel.Statements;

public class DataStatement : Statement
{
	public override StatementType Type => StatementType.Data;

	public string? RawString { get; set; }

	public DataStatement(string dataString)
	{
		RawString = dataString;
	}

	public IEnumerable<string> ParseDataItems()
	{
		var dataMemory = RawString.AsMemory();

		// TODO: make sure NumberParser parses empty string to 0 (not error)

		while (dataMemory.Length > 0)
		{
			var dataSpan = dataMemory.Span;

			while ((dataSpan.Length > 0) && char.IsWhiteSpace(dataSpan[0]))
			{
				dataMemory = dataMemory.Slice(1);
				dataSpan = dataMemory.Span;
			}

			if (dataSpan[0] == '"')
			{
				int closeQuote = dataSpan.Slice(1).IndexOf('"') + 1;

				if (closeQuote < 0)
				{
					yield return new string(dataSpan.Slice(1));
					break;
				}

				int nextField = closeQuote + 1;

				while ((nextField < dataSpan.Length) && char.IsWhiteSpace(dataSpan[nextField]))
					nextField++;

				if ((nextField < dataSpan.Length) && (dataSpan[nextField] != ','))
					throw RuntimeException.SyntaxError(default);

				nextField++;

				yield return new string(dataSpan.Slice(1, closeQuote - 1));

				dataMemory = dataMemory.Slice(nextField);
			}
			else
			{
				int comma = dataSpan.IndexOf(',');

				var token = (comma >= 0)
					? dataSpan.Slice(0, comma)
					: dataSpan;

				while ((token.Length > 0) && char.IsWhiteSpace(token[token.Length - 1]))
					token = token.Slice(0, token.Length - 1);

				yield return new string(token);

				dataMemory = dataMemory.Slice((comma >= 0) ? comma + 1 : dataMemory.Length);
			}
		}
	}

	protected override void RenderImplementation(TextWriter writer)
	{
		writer.Write("DATA");

		writer.Write(RawString);
	}
}
