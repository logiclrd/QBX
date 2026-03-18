using QBX.CodeModel.Statements;
using QBX.ExecutionEngine;
using QBX.LexicalAnalysis;
using QBX.Parser;

using static QBX.Tests.Utility.IdentifierHelpers;

namespace QBX.Tests.Parser.Statements;

public class CommonStatementTests
{
	[TestCase("COMMON SHARED arena() AS arenaType",
		CommonBlock.DefaultBlockNameValue,
		"arena",
		true,
		"arenaType",
		null,
		false)]
	[TestCase("COMMON SHARED /BlockName/ arena(1 TO 50, 1 TO 80) AS arenaType",
		"BlockName",
		"arena",
		true,
		"arenaType",
		null,
		false)]
	[TestCase("COMMON curLevel, colorTable()",
		CommonBlock.DefaultBlockNameValue,
		"curLevel",
		false,
		null,
		"colorTable",
		true)]
	[TestCase("COMMON /CustomBlockName/ curLevel, colorTable()",
		"CustomBlockName",
		"curLevel",
		false,
		null,
		"colorTable",
		true)]
	public void ShouldParse(string declaration, string expectedBlockName, string variable1Name, bool variable1ShouldBeArray, string? variable1Type, string? variable2Name, bool variable2ShouldBeArray)
	{
		// Arrange
		var tokens = new Lexer(declaration).ToList();

		tokens.RemoveAll(token => token.Type == TokenType.Whitespace);

		var identifierRepository = new IdentifierRepository();

		var sut = new BasicParser(identifierRepository);

		// Act
		var result = sut.ParseStatement(tokens, ignoreErrors: false);

		// Assert
		result.Should().BeOfType<CommonStatement>();

		var commonResult = (CommonStatement)result;

		commonResult.BlockName.Should().Be(ID(expectedBlockName));

		int expectedCount = (variable2Name == null) ? 1 : 2;

		commonResult.Declarations.Should().HaveCount(expectedCount);

		commonResult.Declarations[0].Name.Should().Be(ID(variable1Name));

		if (variable1ShouldBeArray == false)
			commonResult.Declarations[0].Subscripts.Should().BeNull();
		else
		{
			commonResult.Declarations[0].Subscripts.Should().NotBeNull();
			commonResult.Declarations[0].Subscripts!.Subscripts.Should().HaveCount(0);
		}

		if (variable1Type != null)
			commonResult.Declarations[0].UserType.Should().Be(ID(variable1Type));

		if (variable2Name != null)
		{
			commonResult.Declarations[1].Name.Should().Be(ID(variable2Name));

			if (variable2ShouldBeArray == false)
				commonResult.Declarations[1].Subscripts.Should().BeNull();
			else
			{
				commonResult.Declarations[1].Subscripts.Should().NotBeNull();
				commonResult.Declarations[1].Subscripts!.Subscripts.Should().HaveCount(0);
			}
		}
	}
}
