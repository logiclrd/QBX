using QBX.CodeModel.Statements;
using QBX.LexicalAnalysis;
using QBX.Parser;

namespace QBX.Tests.Parser.Statements;

public class CommentStatementTests
{
	[TestCase("' This is a comment", " This is a comment")]
	[TestCase("REM This is a comment", "This is a comment")]
	public void ShouldParse(string text, string comment)
	{
		// Arrange
		var tokens = new Lexer(text).ToList();

		tokens.RemoveAll(token => token.Type == TokenType.Whitespace);

		bool inType = false;

		var sut = new BasicParser();

		// Act
		var result = sut.ParseStatement(tokens, colonAfter: false, ref inType);

		// Assert
		result.Should().BeOfType<CommentStatement>();

		var commentResult = (CommentStatement)result;

		commentResult.Comment.Should().Be(comment);
	}
}

/*
namespace QBX.Tests.Parser.Statements;

public class CommentStatement(CommentStatementType type, string comment)
{
	public override StatementType Type => StatementType.Comment;

	public CommentStatementType CommentStatementType { get; set; } = type;
	public string Comment { get; set; } = comment;

	public override void Render(TextWriter writer)
	{
		if (CommentStatementType == CommentStatementType.REM)
		{
			writer.Write("REM");

			if (Comment.Length > 0 && !char.IsWhiteSpace(Comment[0]))
				writer.Write(' ');
		}
		else
			writer.Write('\'');

		writer.Write(Comment);
	}
}

*/
