using System;
using System.IO;

namespace QBX.CodeModel.Statements;

public class ResumeStatement : Statement
{
	public override StatementType Type => StatementType.Resume;

	public bool NextStatement { get; set; }
	public string? TargetLineNumber;
	public string? TargetLabel;

	public bool SameStatement =>
		(TargetLineNumber == null)
		? ((TargetLabel == null) && !NextStatement)
		: (int.TryParse(TargetLineNumber, out var parsedLineNumber) && (parsedLineNumber == 0));

	protected override void RenderImplementation(TextWriter writer)
	{
		writer.Write("RESUME");

		if (NextStatement)
		{
			if ((TargetLineNumber != null) || (TargetLabel != null))
				throw new Exception("Internal error: ResumeStatement with both NextStatement and a target line");

			writer.Write(" NEXT");
		}
		else
		{
			if ((TargetLineNumber != null) && (TargetLabel != null))
				throw new Exception($"Internal error: ResumeStatement with both line number and label");

			if (TargetLineNumber != null)
			{
				writer.Write(' ');
				writer.Write(TargetLineNumber);
			}
			else
			{
				writer.Write(' ');
				writer.Write(TargetLabel);
			}
		}
	}
}
