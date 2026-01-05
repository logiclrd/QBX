using QBX.CodeModel.Expressions;
using QBX.CodeModel.Statements;
using QBX.LexicalAnalysis;
using QBX.Parser;

namespace QBX.Tests.Parser.Statements;

public class CallStatementTests
{
	[TestCase("CALL MySub", CallStatementType.Explicit, 0)]
	[TestCase("CALL MySub(33)", CallStatementType.Explicit, 1)]
	[TestCase("CALL MySub(33, \"argh\")", CallStatementType.Explicit, 2)]
	[TestCase("MySub", CallStatementType.Implicit, 0)]
	[TestCase("MySub 33", CallStatementType.Implicit, 1)]
	[TestCase("MySub 33, \"argh\"", CallStatementType.Implicit, 2)]
	public void ShouldParse(string statement, CallStatementType callStatementType, int numArguments)
	{
		// Arrange
		var tokens = new Lexer(statement).ToList();

		tokens.RemoveAll(token => token.Type == TokenType.Whitespace);

		var sut = new BasicParser();

		// Act
		var result = sut.ParseStatement(tokens);

		// Assert
		result.Should().BeOfType<CallStatement>();

		var callResult = (CallStatement)result;

		callResult.CallStatementType.Should().Be(callStatementType);
		callResult.TargetName = "MySub";

		if (numArguments == 0)
			callResult.Arguments.Should().BeNull();
		else
		{
			callResult.Arguments.Should().NotBeNull();
			callResult.Arguments!.Expressions.Should().HaveCount(numArguments);

			if (numArguments > 1)
			{
				var arg = callResult.Arguments!.Expressions[0];

				arg.Should().BeOfType<LiteralExpression>()
					.Which.Token!.Value.Should().Be("33");
			}

			if (numArguments > 2)
			{
				var arg = callResult.Arguments!.Expressions[0];

				arg.Should().BeOfType<LiteralExpression>()
					.Which.Token!.Value.Should().Be("\"argh\"");
			}
		}
	}
}

/*
using QBX.CodeModel.Expressions;

namespace QBX.Tests.Parser.Statements;

class CallStatement
{
	public override StatementType Type => StatementType.Call;

	public CallStatementType CallStatementType { get; set; }
	public string TargetName { get; set; }
	public ExpressionList? Arguments { get; set; }

	public CallStatement(CallStatementType type, string targetName, ExpressionList? arguments)
	{
		CallStatementType = type;
		TargetName = targetName;
		Arguments = arguments;
	}

	public override void Render(TextWriter writer)
	{
		switch (CallStatementType)
		{
			case CallStatementType.Explicit:
				writer.Write("CALL {0}", TargetName);

				if (Arguments != null && Arguments.Expressions.Any())
				{
					writer.Write('(');
					Arguments.Render(writer);
					writer.Write(')');
				}

				break;
			case CallStatementType.Implicit:
				writer.Write(TargetName);

				if (Arguments != null && Arguments.Expressions.Any())
				{
					writer.Write(' ');
					Arguments.Render(writer);
				}

				break;
		}
	}
}

*/
