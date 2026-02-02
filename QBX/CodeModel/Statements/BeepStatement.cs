using System.IO;

namespace QBX.CodeModel.Statements;

public class BeepStatement : Statement
{
	public override StatementType Type => StatementType.Beep;

	protected override void RenderImplementation(TextWriter writer)
	{
		writer.Write("BEEP");
	}
}
