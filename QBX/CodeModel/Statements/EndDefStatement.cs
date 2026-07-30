using System.IO;

namespace QBX.CodeModel.Statements;

public class EndDefStatement : Statement
{
	public override StatementType Type => StatementType.EndDef;

	public override bool IsLegalInDirectMode => false;

	protected override void RenderImplementation(TextWriter writer)
	{
		writer.Write("END DEF");
	}
}
