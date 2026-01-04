using QBX.LexicalAnalysis;

namespace QBX.CodeModel.Statements;

public class TypeStatement : Statement
{
	public override StatementType Type => StatementType.Type;

	public string Name { get; set; } = "";

	public Token? NameToken { get; set; }

	protected override void RenderImplementation(TextWriter writer)
	{
		writer.Write("TYPE ");
		writer.Write(Name);
	}
}
