using System.IO;

using QBX.CodeModel.Expressions;

namespace QBX.CodeModel.Statements;

public class SoftKeyConfigStatement : Statement
{
	public override StatementType Type => StatementType.SoftKeyConfig;

	public Expression? KeyExpression;
	public Expression? MacroExpression;

	protected override void RenderImplementation(TextWriter writer)
	{
		if (KeyExpression == null)
			throw new System.Exception("KeyConfigStatement with no KeyExpression");
		if (MacroExpression == null)
			throw new System.Exception("KeyConfigStatement with no MacroExpression");

		writer.Write("KEY ");
		KeyExpression.Render(writer);
		writer.Write(", ");
		MacroExpression.Render(writer);
	}
}
