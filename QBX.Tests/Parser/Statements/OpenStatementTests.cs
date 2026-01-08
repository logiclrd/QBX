using QBX.CodeModel.Expressions;
using QBX.CodeModel.Statements;
using QBX.LexicalAnalysis;
using QBX.Parser;

namespace QBX.Tests.Parser.Statements;

public class OpenStatementTests
{
	[TestCase(
		"OPEN \"file\" FOR INPUT AS #1",
		typeof(LiteralExpression),
		OpenMode.Input,
		AccessMode.Unspecified,
		LockMode.None,
		1,
		-1)]
	[TestCase(
		"OPEN \"file\" FOR INPUT ACCESS READ AS #filenumber%",
		typeof(LiteralExpression),
		OpenMode.Input,
		AccessMode.Read,
		LockMode.None,
		-1,
		-1)]
	[TestCase(
		"OPEN fileName$ FOR INPUT SHARED AS #1",
		typeof(IdentifierExpression),
		OpenMode.Input,
		AccessMode.Unspecified,
		LockMode.Shared,
		1,
		-1)]
	[TestCase(
		"OPEN baseName$ + \".txt\" FOR RANDOM LOCK WRITE AS #33",
		typeof(BinaryExpression),
		OpenMode.Random,
		AccessMode.Unspecified,
		LockMode.LockWrite,
		33,
		-1)]
	[TestCase(
		"OPEN \"file\" FOR OUTPUT ACCESS READ WRITE LOCK READ WRITE AS #1",
		typeof(LiteralExpression),
		OpenMode.Output,
		AccessMode.ReadWrite,
		LockMode.LockReadWrite,
		1,
		-1)]
	[TestCase(
		"OPEN \"file\" FOR BINARY ACCESS WRITE LOCK WRITE AS #7 LEN = 128",
		typeof(LiteralExpression),
		OpenMode.Binary,
		AccessMode.Write,
		LockMode.LockWrite,
		7,
		128)]
	[TestCase(
		"OPEN \"file\" FOR BINARY AS #1 LEN = 128",
		typeof(LiteralExpression),
		OpenMode.Binary,
		AccessMode.Unspecified,
		LockMode.None,
		1,
		128)]
	public void ShouldParse(string statement, Type expectedFileNameExpressionType, OpenMode expectedOpenMode, AccessMode expectedAccessMode, LockMode expectedLockMode, int expectedFileNumber, int expectedRecordLength)
	{
		// Arrange
		var tokens = new Lexer(statement).ToList();

		tokens.RemoveAll(token => token.Type == TokenType.Whitespace);

		var sut = new BasicParser();

		// Act
		var result = sut.ParseStatement(tokens, ignoreErrors: false);

		// Assert
		result.Should().BeOfType<OpenStatement>();

		var openResult = (OpenStatement)result;

		openResult.FileNameExpression.Should().BeOfType(expectedFileNameExpressionType);
		openResult.OpenMode.Should().Be(expectedOpenMode);
		openResult.AccessMode.Should().Be(expectedAccessMode);
		openResult.LockMode.Should().Be(expectedLockMode);

		if (expectedFileNumber > 0)
		{
			openResult.FileNumberExpression.Should().BeOfType<LiteralExpression>()
				.Which.Token!.Value.Should().Be(expectedFileNumber.ToString());
		}
		else
			openResult.FileNumberExpression.Should().NotBeOfType<LiteralExpression>();

		if (expectedRecordLength > 0)
		{
			openResult.RecordLengthExpression.Should().BeOfType<LiteralExpression>()
				.Which.Token!.Value.Should().Be(expectedRecordLength.ToString());
		}
		else
			openResult.RecordLengthExpression.Should().BeNull();
	}
}
