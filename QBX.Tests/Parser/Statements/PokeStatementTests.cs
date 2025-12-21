using QBX.CodeModel.Statements;
using QBX.LexicalAnalysis;
using QBX.Parser;

namespace QBX.Tests.Parser.Statements;

public class PokeStatementTests
{
	[TestCase("POKE 1047, KeyFlags")]
	[TestCase("POKE a%, b% XOR c%")]
	public void ShouldParse(string statement)
	{
		// Arrange
		var tokens = new Lexer(statement).ToList();

		tokens.RemoveAll(token => token.Type == TokenType.Whitespace);

		bool inType = false;

		var sut = new BasicParser();

		// Act
		var result = sut.ParseStatement(tokens, ref inType);

		// Assert
		result.Should().BeOfType<PokeStatement>();

		var pokeResult = (PokeStatement)result;

		pokeResult.AddressExpression.Should().NotBeNull();
		pokeResult.ValueExpression.Should().NotBeNull();
	}
}

/*
using QBX.CodeModel.Expressions;

namespace QBX.Tests.Parser.Statements;

public class PokeStatement
{
	public override StatementType Type => StatementType.Poke;

	public Expression? AddressExpression { get; set; }
	public Expression? ValueExpression { get; set; }

	public override void Render(TextWriter writer)
	{
		if ((AddressExpression == null) || (ValueExpression == null))
			throw new Exception("Internal error: PokeStatement missing address or value expression");

		writer.Write("POKE ");
		AddressExpression.Render(writer);
		writer.Write(", ");
		ValueExpression.Render(writer);
	}
}

*/
