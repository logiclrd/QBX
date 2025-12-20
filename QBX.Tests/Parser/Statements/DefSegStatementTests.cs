/*
using QBX.CodeModel.Expressions;

namespace QBX.Tests.Parser.Statements;

public class DefSegStatement
{
	public override StatementType Type => StatementType.DefSeg;

	public Expression? SegmentExpression { get; set; }

	public override void Render(TextWriter writer)
	{
		writer.Write("DEF SEG");

		if (SegmentExpression != null)
		{
			writer.Write(" = ");
			SegmentExpression.Render(writer);
		}
	}
}

*/
