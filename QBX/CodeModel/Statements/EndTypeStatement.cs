using System.IO;

namespace QBX.CodeModel.Statements;

public class EndTypeStatement : Statement
{
	public override StatementType Type => StatementType.EndType;

	public override bool IsLegalInDirectMode => false;

	protected override void RenderImplementation(TextWriter writer)
	{
		writer.Write("END TYPE");
	}
}
