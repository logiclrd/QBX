using System;
using System.IO;

using QBX.CodeModel.Expressions;

namespace QBX.CodeModel.Statements;

public class WaitStatement : Statement
{
	public override StatementType Type => StatementType.Wait;

	public Expression? PortExpression;
	public Expression? AndExpression;
	public Expression? XOrExpression;

	protected override void RenderImplementation(TextWriter writer)
	{
		if (PortExpression == null)
			throw new Exception("WaitStatement with no PortExpression");
		if (AndExpression == null)
			throw new Exception("WaitStatement with no AndExpression");

		writer.Write("WAIT ");
		PortExpression.Render(writer);
		writer.Write(", ");
		AndExpression.Render(writer);

		if (XOrExpression != null)
		{
			writer.Write(", ");
			XOrExpression.Render(writer);
		}
	}
}
