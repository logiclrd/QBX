using QBX.CodeModel;
using QBX.CodeModel.Statements;
using QBX.LexicalAnalysis;
using QBX.Parser;

namespace QBX.Tests.Parser.Statements;

public class DeclareStatementTests
{
	[TestCase("DECLARE SUB SpacePause (text$)",
		TokenType.SUB,
		"SpacePause",
		new string[] { "text$" })]
	[TestCase("DECLARE SUB PrintScore (NumPlayers%, score1%, score2%, lives1%, lives2%)",
		TokenType.SUB,
		"PrintScore",
		new string[] { "NumPlayers%", "score1%", "score2%", "lives1%", "lives2%" })]
	[TestCase("DECLARE SUB Intro ()",
		TokenType.SUB,
		"Intro",
		new string[0])]
	[TestCase("DECLARE SUB GetInputs (NumPlayers, speed, diff$, monitor$)",
		TokenType.SUB,
		"GetInputs",
		new string[] { "NumPlayers", "speed", "diff$", "monitor$" })]
	[TestCase("DECLARE SUB DrawScreen ()",
		TokenType.SUB,
		"DrawScreen",
		new string[0])]
	[TestCase("DECLARE SUB PlayNibbles (NumPlayers, speed, diff$)",
		TokenType.SUB,
		"PlayNibbles",
		new string[] { "NumPlayers", "speed", "diff$" })]
	[TestCase("DECLARE SUB Set (row, col, acolor)",
		TokenType.SUB,
		"Set",
		new string[] { "row", "col", "acolor" })]
	[TestCase("DECLARE SUB Center (row, text$)",
		TokenType.SUB,
		"Center",
		new string[] { "row", "text$" })]
	[TestCase("DECLARE SUB DoIntro ()",
		TokenType.SUB,
		"DoIntro",
		new string[0])]
	[TestCase("DECLARE SUB Initialize ()",
		TokenType.SUB,
		"Initialize",
		new string[0])]
	[TestCase("DECLARE SUB SparklePause ()",
		TokenType.SUB,
		"SparklePause",
		new string[0])]
	[TestCase("DECLARE SUB Level (WhatToDO, sammy() AS snaketype)",
		TokenType.SUB,
		"Level",
		new string[] { "WhatToDO", "sammy() AS snaketype" })]
	[TestCase("DECLARE SUB InitColors ()",
		TokenType.SUB,
		"InitColors",
		new string[0])]
	[TestCase("DECLARE SUB EraseSnake (snake() AS ANY, snakeBod() AS ANY, snakeNum%)",
		TokenType.SUB,
		"EraseSnake",
		new string[] { "snake() AS ANY", "snakeBod() AS ANY", "snakeNum%" })]
	[TestCase("DECLARE FUNCTION StillWantsToPlay ()",
		TokenType.FUNCTION,
		"StillWantsToPlay",
		new string[0])]
	[TestCase("DECLARE FUNCTION PointIsThere (row, col, backColor)",
		TokenType.FUNCTION,
		"PointIsThere",
		new string[] { "row", "col", "backColor" })]
	public void ShouldParse(string declaration, TokenType declarationTypeTokenType, string name, string[] parameters)
	{
		// Arrange
		var tokens = new Lexer(declaration).ToList();

		tokens.RemoveAll(token => token.Type == TokenType.Whitespace);

		bool inType = false;

		var sut = new BasicParser();

		// Act
		var result = sut.ParseStatement(tokens, ref inType);

		// Assert
		result.Should().BeOfType<DeclareStatement>();

		var declareResult = (DeclareStatement)result;

		declareResult.DeclarationType.Type.Should().Be(declarationTypeTokenType);
		declareResult.Name.Should().Be(name);
		declareResult.Parameters.Should().NotBeNull();
		declareResult.Parameters!.Parameters.Should().HaveCount(parameters.Length);

		for (int i = 0; i < parameters.Length; i++)
		{
			string param = parameters[i];

			DataType? dataType = null;
			string? userType = null;
			bool anyType = false;
			bool isArray = false;

			int asIndex = param.IndexOf(" AS ");

			if (asIndex >= 0)
			{
				string typeName = param.Substring(asIndex + 4);

				if (typeName == "ANY")
					anyType = true;
				else if (Enum.TryParse<DataType>(typeName, out var expectedDataType))
					dataType = expectedDataType;
				else
					userType = typeName;

				param = param.Remove(asIndex);
			}

			if (param.EndsWith("()"))
			{
				isArray = true;
				param = param.Remove(param.Length - 2);
			}

			var declaredParameter = declareResult.Parameters.Parameters[i];

			declaredParameter.Name.Should().Be(param);
			declaredParameter.IsArray.Should().Be(isArray);
			declaredParameter.Type.Should().Be(dataType);
			declaredParameter.UserType.Should().Be(userType);
			declaredParameter.AnyType.Should().Be(anyType);
		}
	}
}
