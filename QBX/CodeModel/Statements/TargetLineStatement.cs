using System;
using System.IO;

namespace QBX.CodeModel.Statements;

public abstract class TargetLineStatement : Statement
{
	public string? TargetLineNumber;
	public string? TargetLabel;

	public virtual bool CanBeParameterless => false;

	protected abstract string StatementName { get; }

	protected override void RenderImplementation(TextWriter writer)
	{
		writer.Write(StatementName);
		writer.Write(' ');

		if ((TargetLineNumber != null) && (TargetLabel != null))
			throw new Exception($"Internal error: {StatementName} with both line number and label");

		if (TargetLineNumber != null)
			writer.Write(TargetLineNumber);
		else if (TargetLabel != null)
			writer.Write(TargetLabel);
		else if (!CanBeParameterless)
			throw new Exception($"Internal error: {StatementName} with neither line number nor label");
	}
}
