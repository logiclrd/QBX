using QBX.LexicalAnalysis;

namespace QBX.Parser;

[Serializable]
public class SyntaxErrorException : Exception
{
	public Token Token { get; }

	public SyntaxErrorException(Token token, string? message) : base(message)
	{
		Token = token;
	}

	public override string ToString()
	{
		return Message + $" ({Token.Line}:{Token.Column})";
	}
}
