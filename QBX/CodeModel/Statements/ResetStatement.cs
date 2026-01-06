using System.IO;

namespace QBX.CodeModel.Statements;

public class ResetStatement : Statement
{
	public override StatementType Type => StatementType.Reset;

	protected override void RenderImplementation(TextWriter writer)
	{
		writer.Write("RESET");
	}
}
