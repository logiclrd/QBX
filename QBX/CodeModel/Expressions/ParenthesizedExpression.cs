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

	public override void Render(TextWriter writer)
	{
		writer.Write('(');
		Child.Render(writer);
		writer.Write(')');
	}
}
