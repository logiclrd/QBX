using System;

using QBX.LexicalAnalysis;

namespace QBX.Parser;

[Serializable]
public class SyntaxErrorException : Exception
{
	public Token Token { get; }
	public string? HelpContextString { get; }

	public SyntaxErrorException(Token token, string? message) : base(message)
	{
		Token = token;
	}

	public SyntaxErrorException(Token token, string? message, string? helpContextString) : base(message)
	{
		Token = token;
		HelpContextString = helpContextString;
	}

	public static SyntaxErrorException IdentifierTooLong(Token identifierToken)
	{
		if (identifierToken.Value.Length > 40)
		{
			identifierToken = new Token(
				identifierToken.LineNumberBox,
				identifierToken.Column,
				identifierToken.Type,
				identifierToken.Value.Substring(0, 40),
				identifierToken.DataType);
		}

		return new SyntaxErrorException(identifierToken, "Identifier too long", "-114");
	}

	public override string ToString()
	{
		return Message + $" ({Token.Line + 1}:{Token.Column + 1})";
	}
}
