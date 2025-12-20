using QBX.Parser;

namespace QBX.CodeModel.Statements;

public class GoSubStatement : GoToStatement
{
	public override StatementType Type => StatementType.Loop;

	protected override void RenderStatementName(TextWriter writer)
	{
		writer.Write("GOSUB");
	}
}
