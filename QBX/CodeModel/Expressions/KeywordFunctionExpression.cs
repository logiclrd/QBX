using System.IO;

using QBX.LexicalAnalysis;

namespace QBX.CodeModel.Expressions;

public class KeywordFunctionExpression : Expression
{
	public TokenType Function { get; set; }
	public ExpressionList? Arguments { get; set; }

	public KeywordFunctionExpression(Token nameToken, ExpressionList? arguments = null)
	{
		Token = nameToken;

		Function = nameToken.Type;
		Arguments = arguments;
	}

	public override void Render(TextWriter writer)
	{
		Token.RenderKeyword(Function, writer);
		if (Arguments != null)
		{
			writer.Write('(');
			Arguments.Render(writer);
			writer.Write(')');
		}
	}
}
