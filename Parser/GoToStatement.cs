using QBX.CodeModel.Statements;

namespace QBX.Parser;

public class GoToStatement : Statement
{
	public override StatementType Type => StatementType.GoTo;

	public decimal? TargetLineNumber;
	public string? TargetLabel;

	protected virtual void RenderStatementName(TextWriter writer)
		=> writer.Write("GOTO");

	public override void Render(TextWriter writer)
	{
		RenderStatementName(writer);
		writer.Write(' ');

		if ((TargetLineNumber != null) && (TargetLabel != null))
			throw new Exception("Internal error: GOTO or GOSUB with both line number and label");

		if (TargetLineNumber != null)
			writer.Write(TargetLineNumber);
		else if (TargetLabel != null)
			writer.Write(TargetLabel);
		else
			throw new Exception("Internal error: GOTO or GOSUB with neither line number nor label");
	}
}
