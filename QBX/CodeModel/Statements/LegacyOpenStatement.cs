using QBX.CodeModel.Expressions;

namespace QBX.CodeModel.Statements;

public class LegacyOpenStatement : Statement
{
	public override StatementType Type => StatementType.OpenLegacy;

	public Expression? ModeExpression { get; set; }
	public Expression? FileNumberExpression { get; set; }
	public Expression? FileNameExpression { get; set; }
	public Expression? RecordLengthExpression { get; set; }

	protected override void RenderImplementation(TextWriter writer)
	{
		if (ModeExpression == null)
			throw new Exception("Internal error: LegacyOpenStatement with no ModeExpression");
		if (FileNumberExpression == null)
			throw new Exception("Internal error: LegacyOpenStatement with no FileNumberExpression");
		if (FileNameExpression == null)
			throw new Exception("Internal error: LegacyOpenStatement with no FileNameExpression");

		writer.Write("OPEN ");
		ModeExpression.Render(writer);
		writer.Write(", #");
		FileNumberExpression.Render(writer);
		writer.Write(", ");
		FileNameExpression.Render(writer);

		if (RecordLengthExpression != null)
		{
			writer.Write(", ");
			RecordLengthExpression.Render(writer);
		}
	}
}
