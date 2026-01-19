using System.IO;

using QBX.LexicalAnalysis;

namespace QBX.CodeModel.Expressions;

public class ParenthesizedExpression : Expression
{
	public Expression Child { get; set; }

	public ParenthesizedExpression(Token openParenthesisToken, Expression child)
	{
		Token = openParenthesisToken;
		Child = child;
	}

	public override Expression ClaimTokens(CodeModel.Statements.Statement owner)
	{
		Child.ClaimTokens(owner);

		return base.ClaimTokens(owner);
	}

	public override void Render(TextWriter writer)
	{
		writer.Write('(');
		Child.Render(writer);
		writer.Write(')');
	}
}
