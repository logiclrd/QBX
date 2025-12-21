using QBX.LexicalAnalysis;

namespace QBX.CodeModel.Expressions;

public class UnaryExpression : Expression
{
	public Operator Operator { get; set; }
	public Expression Child { get; set; }

	public UnaryExpression(Token operatorToken, Expression child)
	{
		Token = operatorToken;
		Child = child;

		Operator =
			operatorToken.Type switch
			{
				TokenType.Minus => Operator.Negate,
				TokenType.NOT => Operator.Not,

				_ => throw new Exception("Internal error: unrecognized unary expression operator token: " + operatorToken)
			};
	}

	public override void Render(TextWriter writer)
	{
		switch (Operator)
		{
			case Operator.Negate: writer.Write('-'); break;
			case Operator.Not: writer.Write("NOT "); break;

			default: throw new Exception("Internal error: Unspecified unary expression operator");
		}

		Child.Render(writer);
	}
}
