using System;
using System.IO;

using QBX.CodeModel.Expressions;

namespace QBX.CodeModel.Statements;

public class GetSpriteStatement : Statement
{
	public override StatementType Type => StatementType.GetSprite;

	public bool FromStep { get; set; }
	public Expression? FromXExpression { get; set; }
	public Expression? FromYExpression { get; set; }

	public bool ToStep { get; set; }
	public Expression? ToXExpression { get; set; }
	public Expression? ToYExpression { get; set; }

	public Expression? TargetExpression { get; set; }

	protected override void RenderImplementation(TextWriter writer)
	{
		writer.Write("GET ");

		if (FromStep)
			writer.Write("STEP");

		if ((FromXExpression == null) || (FromYExpression == null))
			throw new Exception("Internal error: GetSpriteStatement missing one or both of the From coordinate expressions");

		writer.Write('(');
		FromXExpression.Render(writer);
		writer.Write(", ");
		FromYExpression.Render(writer);
		writer.Write(")-");

		if (ToStep)
			writer.Write("STEP");

		if ((ToXExpression == null) || (ToYExpression == null))
			throw new Exception("Internal error: GetSpriteStatement missing one or both of the To coordinate expressions");

		writer.Write('(');
		ToXExpression.Render(writer);
		writer.Write(", ");
		ToYExpression.Render(writer);
		writer.Write("), ");

		if (TargetExpression == null)
			throw new Exception("Internal error: GetSpriteStatement missing the Target expression");

		TargetExpression.Render(writer);
	}
}
