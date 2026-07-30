using System.IO;

namespace QBX.CodeModel.Statements;

public class ElseStatement : Statement
{
	public override StatementType Type => StatementType.Else;

	public override bool IsLegalInDirectMode => false;

	protected override void RenderImplementation(TextWriter writer)
	{
		writer.Write("ELSE");
	}
}
