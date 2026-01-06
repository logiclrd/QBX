using System.IO;

namespace QBX.CodeModel.Statements;

public class EndDefStatement : Statement
{
	public override StatementType Type => StatementType.EndDef;

	protected override void RenderImplementation(TextWriter writer)
	{
		writer.Write("END DEF");
	}
}
