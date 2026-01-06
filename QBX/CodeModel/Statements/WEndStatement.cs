using System.IO;

namespace QBX.CodeModel.Statements;

public class WEndStatement : Statement
{
	public override StatementType Type => StatementType.WEnd;

	protected override void RenderImplementation(TextWriter writer)
	{
		writer.Write("WEND");
	}
}
