using QBX.CodeModel.Statements;

namespace QBX.Parser;

public class ElseIfStatement : IfStatement
{
	public override StatementType Type => StatementType.ElseIf;

	protected override void RenderStatementName(TextWriter writer)
	{
		writer.Write("ELSEIF");
	}
}
