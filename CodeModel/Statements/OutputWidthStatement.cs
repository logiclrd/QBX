using QBX.CodeModel.Expressions;

namespace QBX.CodeModel.Statements;

public abstract class OutputWidthStatement : Statement
{
	public Expression? WidthExpression { get; set; }

	protected void VerifyWidthExpression()
	{
		if (WidthExpression == null)
			throw new Exception("Internal error: " + Type + "Statement with no WidthExpression");
	}
}
