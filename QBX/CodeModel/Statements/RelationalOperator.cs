using QBX.CodeModel.Expressions;

namespace QBX.CodeModel.Statements;

public enum RelationalOperator
{
	Equals = Operator.Equals,
	NotEquals = Operator.NotEquals,
	LessThan = Operator.LessThan,
	LessThanOrEquals = Operator.LessThanOrEquals,
	GreaterThan = Operator.GreaterThan,
	GreaterThanOrEquals = Operator.GreaterThanOrEquals,
}
