using System;
using System.IO;

using QBX.CodeModel.Expressions;

namespace QBX.CodeModel.Statements;

public abstract class FileByteRangeStatement : Statement
{
	public Expression? FileNumberExpression { get; set; }
	public Expression? RangeStartExpression { get; set; }
	public Expression? RangeEndExpression { get; set; }

	protected abstract string StatementName { get; }

	protected override void RenderImplementation(TextWriter writer)
	{
		if (FileNumberExpression == null)
			throw new Exception($"Internal error: {Type}Statement with no FileNumberExpression");
		if (FileNumberExpression == null)
			throw new Exception($"Internal error: {Type}Statement with no FileNumberExpression");

		writer.Write(StatementName);
		writer.Write(" #");
		FileNumberExpression.Render(writer);

		if (RangeStartExpression != null)
		{
			writer.Write(", ");
			RangeStartExpression.Render(writer);

			if (RangeEndExpression != null)
			{
				writer.Write(" TO ");
				RangeEndExpression.Render(writer);
			}
		}
	}
}
