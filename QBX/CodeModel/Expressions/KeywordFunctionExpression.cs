using System.IO;

using QBX.LexicalAnalysis;

namespace QBX.CodeModel.Expressions;

public class KeywordFunctionExpression : Expression
{
	public TokenType Function { get; set; }
	public ExpressionList? Arguments { get; set; }

	// For special cases such as:
	//   MID$(s$, start%, length%) = "replace substring"
	public override bool IsValidAssignmentTarget()
	{
		if (Token.TryGetKeywordFunctionAttribute(Function, out var attribute))
			return attribute.IsAssignable;

		return false;
	}

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
