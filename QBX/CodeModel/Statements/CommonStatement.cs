using System.IO;

using QBX.ExecutionEngine;

namespace QBX.CodeModel.Statements;

public class CommonStatement : DimStatement
{
	public override StatementType Type => StatementType.Common;

	public string BlockName { get; set; } = CommonBlock.DefaultBlockName;

	public override bool AlwaysDeclareArrays => false;
	public override bool DeclareScalars => false;

	public override void AddDeclaration(VariableDeclaration declaration)
	{
		declaration.Subscripts?.Clear();

		base.AddDeclaration(declaration);
	}

	protected override string StatementName => "COMMON";

	protected override void RenderBlockName(TextWriter writer)
	{
		if (BlockName != CommonBlock.DefaultBlockName)
		{
			writer.Write(" /");
			writer.Write(BlockName);
			writer.Write('/');
		}
	}
}
