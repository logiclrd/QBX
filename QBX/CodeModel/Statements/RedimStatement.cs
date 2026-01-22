using System.IO;

namespace QBX.CodeModel.Statements;

public class RedimStatement : DimStatement
{
	public override StatementType Type => StatementType.Redim;

	public bool Preserve { get; set; }

	public override bool AlwaysDeclareArrays => false;
	public override bool DeclareScalars => false;

	protected override string StatementName => "REDIM";

	protected override void RenderPreserveFlag(TextWriter writer)
	{
		if (Preserve)
			writer.Write(" PRESERVE");
	}
}
