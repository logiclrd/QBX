using System.IO;

using QBX.LexicalAnalysis;
using QBX.Parser;

namespace QBX.CodeModel.Statements;

public class TypeStatement : Statement
{
	public override StatementType Type => StatementType.Type;

	public Identifier Name { get; set; } = Identifier.Empty;

	public Token? NameToken { get; set; }

	protected override void RenderImplementation(TextWriter writer)
	{
		writer.Write("TYPE ");
		writer.Write(Name);
	}
}
