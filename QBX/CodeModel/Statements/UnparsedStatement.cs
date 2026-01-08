using System.IO;
using System.Linq;

using QBX.LexicalAnalysis;
using QBX.Parser;

namespace QBX.CodeModel.Statements;

public class UnparsedStatement : Statement
{
	public UnparsedStatement()
	{
	}

	public UnparsedStatement(string indentation, ListRange<Token> tokens)
	{
		Indentation = indentation;
		Text = string.Concat(tokens.Select(token => token.Value));
	}

	public override StatementType Type => StatementType.Unparsed;

	public string Text { get; set; } = "";

	protected override void RenderImplementation(TextWriter writer)
	{
		writer.Write(Text);
	}
}
