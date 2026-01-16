using System.IO;

namespace QBX.CodeModel.Statements;

public class SoftKeyListStatement : Statement
{
	public override StatementType Type => StatementType.SoftKeyList;

	protected override void RenderImplementation(TextWriter writer)
	{
		writer.Write("KEY LIST");
	}
}
