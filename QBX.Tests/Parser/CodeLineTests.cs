using QBX.CodeModel;
using QBX.CodeModel.Statements;
using QBX.LexicalAnalysis;
using QBX.Parser;

namespace QBX.Tests.Parser;

public class CodeLineTests
{
	[Test]
	public void LineNumber()
	{
		// Arrange
		string input =
@"10 PRINT ""Hello, world""
20 GOTO 10

PRINT ""Look ma, no line number""";

		var lexer = new Lexer(input);

		var tokens = lexer.ToList();

		var sut = new BasicParser();

		// Act
		var lines = sut.ParseCodeLines(tokens).ToList();

		// Assert
		lines.Should().HaveCount(4);
		lines[0].LineNumber.Should().Be("10");
		lines[1].LineNumber.Should().Be("20");
		lines[2].LineNumber.Should().BeNull();
		lines[3].LineNumber.Should().BeNull();
	}

	[Test]
	public void Label()
	{
		// Arrange
		string input =
@"top:
PRINT ""Hello, world""
GOTO top

    retry: PRINT ""Trying again""";

		var lexer = new Lexer(input);

		var tokens = lexer.ToList();

		var sut = new BasicParser();

		// Act
		var lines = sut.ParseCodeLines(tokens).ToList();

		// Assert
		lines.Should().HaveCount(5);
		lines[0].Label.Should().NotBeNull();
		lines[0].Label!.Name.Should().Be("top");
		lines[0].Label!.Indentation.Should().Be("");
		lines[0].Statements.Should().HaveCount(1);
		lines[0].Statements[0].Should().BeOfType<EmptyStatement>();
		lines[1].Label.Should().BeNull();
		lines[2].Label.Should().BeNull();
		lines[3].Label.Should().BeNull();
		lines[4].Label.Should().NotBeNull();
		lines[4].Label!.Name.Should().Be("retry");
		lines[4].Label!.Indentation.Should().Be("    ");
		lines[4].Statements.Should().HaveCount(1);
		lines[4].Statements[0].Should().BeOfType<PrintStatement>();
	}

	[TestCase("     GOTO 10", "     ")]
	[TestCase("10   GOTO 10", "   ")]
	[TestCase("\t\tNEXT", "\t\t")]
	[TestCase("      PRINT     \"this\"   ;  \"is\";   a%     ; trick(question)", "      ")]
	[TestCase("befuddle:      PRINT     \"this\"   ;  \"is\";   a%     ; trick(question)", "      ")]
	[TestCase("befuddle:      ' tricked you", "")]
	public void Indentation(string input, string expectedIndentation)
	{
		// Arrange
		var lexer = new Lexer(input);

		var tokens = lexer.ToList();

		var sut = new BasicParser();

		// Act
		var lines = sut.ParseCodeLines(tokens).ToList();

		// Assert
		lines.Should().HaveCount(1);

		lines[0].Statements[0].Indentation.Should().Be(expectedIndentation);
	}

	[TestCase("NEXT", new StatementType[] { StatementType.Next })]
	[TestCase("END SELECT: PRINT 2", new StatementType[] { StatementType.EndSelect, StatementType.Print })]
	[TestCase("x = 3: y% = 7: IF y% > x * aspect THEN collision = -1", new StatementType[] { StatementType.Assignment, StatementType.Assignment, StatementType.If })]
	public void Statements(string input, StatementType[] expectedStatementTypes)
	{
		// Arrange
		var lexer = new Lexer(input);

		var tokens = lexer.ToList();

		var sut = new BasicParser();

		// Act
		var lines = sut.ParseCodeLines(tokens).ToList();

		// Assert
		lines.Should().HaveCount(1);

		lines[0].Statements.Should().HaveCount(expectedStatementTypes.Length);

		for (int i = 0; i < expectedStatementTypes.Length; i++)
			lines[0].Statements[i].Type.Should().Be(expectedStatementTypes[i]);
	}

	[Test]
	public void EndOfLineComment()
	{
		// Arrange
		string comment = "                     ' Turn off CapLock, NumLock and ScrollLock";
		string input = "DEF SEG = 0" + comment;

		var lexer = new Lexer(input);

		var tokens = lexer.ToList();

		var sut = new BasicParser();

		// Act
		var lines = sut.ParseCodeLines(tokens).ToList();

		// Assert
		lines.Should().HaveCount(1);

		lines[0].EndOfLineComment.Should().Be(comment);
	}
}
