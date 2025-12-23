using QBX.CodeModel.Expressions;

namespace QBX.CodeModel.Statements;

public class FieldDefinition(Expression fieldWidthExpression, Expression targetExpression) : IRenderableCode
{
	public Expression FieldWidthExpression { get; set; } = fieldWidthExpression;
	public Expression TargetExpression { get; set; } = targetExpression;

	public void Render(TextWriter writer)
	{
		FieldWidthExpression.Render(writer);
		writer.Write(" AS ");
		TargetExpression.Render(writer);
	}
}
