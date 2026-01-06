using System.IO;

namespace QBX.CodeModel.Statements;

public class EndTypeStatement : Statement
{
	public override StatementType Type => StatementType.EndType;

	protected override void RenderImplementation(TextWriter writer)
	{
		writer.Write("END TYPE");
	}
}
