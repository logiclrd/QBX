using System;
using System.IO;

using QBX.CodeModel.Expressions;

namespace QBX.CodeModel.Statements;

public abstract class FileBlockOperationStatement : Statement
{
	public abstract string StatementName { get; }

	public Expression? FileNumberExpression { get; set; }
	public Expression? RecordNumberExpression { get; set; }
	public Expression? TargetExpression { get; set; }

	protected override void RenderImplementation(TextWriter writer)
	{
		if (FileNumberExpression == null)
			throw new Exception($"Internal error: {Type}Statement with no FileNumberExpression");

		writer.Write(StatementName);
		writer.Write(" #");
		FileNumberExpression.Render(writer);

		if ((RecordNumberExpression != null) || (TargetExpression != null))
		{
			writer.Write(", ");
			RecordNumberExpression?.Render(writer);

			if (TargetExpression != null)
			{
				writer.Write(", ");
				TargetExpression.Render(writer);
			}
		}
	}
}
