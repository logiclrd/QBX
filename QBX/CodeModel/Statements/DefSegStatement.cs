using System.IO;

using QBX.CodeModel.Expressions;

namespace QBX.CodeModel.Statements;

public class DefSegStatement : Statement
{
	public override StatementType Type => StatementType.DefSeg;

	public Expression? SegmentExpression { get; set; }

	protected override void RenderImplementation(TextWriter writer)
	{
		writer.Write("DEF SEG");

		if (SegmentExpression != null)
		{
			writer.Write(" = ");
			SegmentExpression.Render(writer);
		}
	}
}
