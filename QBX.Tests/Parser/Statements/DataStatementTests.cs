using QBX.CodeModel.Statements;
using QBX.LexicalAnalysis;
using QBX.Parser;

namespace QBX.Tests.Parser.Statements;

public class DataStatementTests
{
	[TestCase("DATA", new TokenType[0])]
	[TestCase("DATA 1, 2, 3", new TokenType[] { TokenType.Number, TokenType.Number, TokenType.Number })]
	[TestCase("DATA \"hello\", \"donkey\", 42", new TokenType[] { TokenType.String, TokenType.String, TokenType.Number })]
	public void ShouldParse(string definition, TokenType[] dataTokens)
	{
		// Arrange
		var tokens = new Lexer(definition).ToList();

		tokens.RemoveAll(token => token.Type == TokenType.Whitespace);

		bool inType = false;

		var sut = new BasicParser();

		// Act
		var result = sut.ParseStatement(tokens, ref inType);

		// Assert
		result.Should().BeOfType<DataStatement>();

		var dataResult = (DataStatement)result;

		dataResult.DataItems.Should().HaveCount(dataTokens.Length);

		for (int i = 0; i < dataTokens.Length; i++)
			dataResult.DataItems[i].Type.Should().Be(dataTokens[i]);
	}
}
/*
using QBX.LexicalAnalysis;

namespace QBX.Tests.Parser.Statements;

public class DataStatement
{
	public override StatementType Type => StatementType.Data;

	public List<Token> DataItems { get; set; }

	public DataStatement(List<Token> dataItems)
	{
		DataItems = dataItems;
	}

	public override void Render(TextWriter writer)
	{
		writer.Write("DATA ");

		for (int i=0; i<DataItems.Count; i++)
		{
			if (i > 0)
				writer.Write(',');

			writer.Write(DataItems[i].Value);
		}
	}
}

*/
