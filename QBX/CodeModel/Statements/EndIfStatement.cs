using System.IO;

namespace QBX.CodeModel.Statements;

public class EndIfStatement : Statement
{
	public override StatementType Type => StatementType.EndIf;

	protected override void RenderImplementation(TextWriter writer)
	{
		writer.Write("END IF");
	}
}
