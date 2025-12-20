using QBX.CodeModel.Statements;

namespace QBX.Parser;

public class EmptyStatement : Statement
{
	public override StatementType Type => StatementType.Empty;

	public override void Render(TextWriter writer)
	{
	}
}
