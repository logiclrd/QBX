using QBX.CodeModel.Statements;
using QBX.LexicalAnalysis;
using QBX.Parser;

namespace QBX.Tests.Parser.Statements;

public class AssignmentStatementTests
{
	[TestCase("foo")]
	[TestCase("foo%")]
	[TestCase("foo&")]
	[TestCase("foo!")]
	[TestCase("foo#")]
	[TestCase("foo$")]
	[TestCase("foo@")]
	public void ShouldParse(string targetVariableName)
	{
		// Arrange
		var text = $"{targetVariableName} = 0";

		var tokens = new Lexer(text).ToList();

		bool inType = false;

		var sut = new BasicParser();

		// Act
		var result = sut.ParseStatement(tokens, colonAfter: false, ref inType);

		// Assert
		result.Should().BeOfType<AssignmentStatement>();

		var assignmentResult = (AssignmentStatement)result;

		assignmentResult.Variable.Should().Be(targetVariableName);
	}
}
