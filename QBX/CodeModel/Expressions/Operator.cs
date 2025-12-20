namespace QBX.CodeModel.Expressions;

public enum Operator
{
	[Precedence(2)] Negate, // unary

	[Precedence(6)] Add,
	[Precedence(6)] Subtract,
	[Precedence(3)] Multiply,
	[Precedence(3)] Divide,
	[Precedence(1)] Exponentiate,
	[Precedence(4)] IntegerDivide,
	[Precedence(5)] Modulo,

	[Precedence(7)] Equals,
	[Precedence(7)] NotEquals,
	[Precedence(7)] LessThan,
	[Precedence(7)] LessThanOrEquals,
	[Precedence(7)] GreaterThan,
	[Precedence(7)] GreaterThanOrEquals,

	[Precedence(8)] Not,
	[Precedence(9)] And,
	[Precedence(10)] Or,
	[Precedence(11)] ExclusiveOr,
	[Precedence(12)] Equivalent,
	[Precedence(13)] Implies,
}
