/*
namespace QBX.Tests.Parser.Statements;

public abstract class TargetLineStatement
{
	public decimal? TargetLineNumber;
	public string? TargetLabel;

	protected abstract string StatementName { get; }

	public override void Render(TextWriter writer)
	{
		writer.Write(StatementName);
		writer.Write(' ');

		if ((TargetLineNumber != null) && (TargetLabel != null))
			throw new Exception($"Internal error: {StatementName} with both line number and label");

		if (TargetLineNumber != null)
			writer.Write(TargetLineNumber);
		else if (TargetLabel != null)
			writer.Write(TargetLabel);
		else
			throw new Exception($"Internal error: {StatementName} with neither line number nor label");
	}
}

*/
