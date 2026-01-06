using System.IO;

namespace QBX.CodeModel.Statements;

public class EmptyStatement : Statement
{
	public override StatementType Type => StatementType.Empty;

	protected override void RenderImplementation(TextWriter writer)
	{
	}
}
