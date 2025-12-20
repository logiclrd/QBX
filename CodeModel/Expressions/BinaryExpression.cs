using QBX.LexicalAnalysis;

namespace QBX.CodeModel.Expressions;

public class BinaryExpression : Expression
{
	public Expression Left { get; set; }
	public Operator Operator { get; set; }
	public Expression Right { get; set; }

	public BinaryExpression(Expression left, Token operatorToken, Expression right)
	{
		Left = left;
		Token = operatorToken;
		Right = right;

		Operator =
			operatorToken.Type switch
			{
				TokenType.Plus => Operator.Add,
				TokenType.Minus => Operator.Subtract,
				TokenType.Asterisk => Operator.Multiply,
				TokenType.Slash => Operator.Divide,
				TokenType.Caret => Operator.Exponentiate,
				TokenType.Backslash => Operator.IntegerDivide,
				TokenType.MOD => Operator.Modulo,

				TokenType.Equals => Operator.Equals,
				TokenType.NotEquals => Operator.NotEquals,
				TokenType.LessThan => Operator.LessThan,
				TokenType.LessThanOrEquals => Operator.LessThanOrEquals,
				TokenType.GreaterThan => Operator.GreaterThan,
				TokenType.GreaterThanOrEquals => Operator.GreaterThanOrEquals,

				TokenType.AND => Operator.And,
				TokenType.OR => Operator.Or,
				TokenType.XOR => Operator.ExclusiveOr,
				TokenType.EQV => Operator.Equivalent,
				TokenType.IMP => Operator.Implies,

				_ => throw new Exception("Internal error: unrecognized binary expression operator token: " + operatorToken)
			};
	}

	public override void Render(TextWriter writer)
	{
		Left.Render(writer);
		writer.Write(' ');

		switch (Operator)
		{
			case Operator.Add: writer.Write('+'); break;
			case Operator.Subtract: writer.Write('-'); break;
			case Operator.Multiply: writer.Write('*'); break;
			case Operator.Divide: writer.Write('/'); break;
			case Operator.Exponentiate: writer.Write('^'); break;
			case Operator.IntegerDivide: writer.Write('\\'); break;
			case Operator.Modulo: writer.Write("MOD"); break;

			case Operator.Equals: writer.Write('='); break;
			case Operator.NotEquals: writer.Write("<>"); break;
			case Operator.LessThan: writer.Write('<'); break;
			case Operator.LessThanOrEquals: writer.Write("<="); break;
			case Operator.GreaterThan: writer.Write('>'); break;
			case Operator.GreaterThanOrEquals: writer.Write(">="); break;

			case Operator.And: writer.Write("AND"); break;
			case Operator.Or: writer.Write("OR"); break;
			case Operator.ExclusiveOr: writer.Write("XOR"); break;
			case Operator.Equivalent: writer.Write("EQV"); break;
			case Operator.Implies: writer.Write("IMP"); break;

			default: throw new Exception("Internal error: Unspecified binary expression operator");
		}

		writer.Write(' ');
		Right.Render(writer);
	}
}
