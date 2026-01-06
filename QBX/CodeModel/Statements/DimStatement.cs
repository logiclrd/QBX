using System.Collections.Generic;
using System.IO;

namespace QBX.CodeModel.Statements;

public class DimStatement : Statement
{
	public override StatementType Type => StatementType.Dim;

	public bool Shared { get; set; }
	public List<VariableDeclaration> Declarations { get; } = new List<VariableDeclaration>();

	protected override void RenderImplementation(TextWriter writer)
	{
		writer.Write("DIM ");

		if (Shared)
			writer.Write("SHARED ");

		for (int i = 0; i < Declarations.Count; i++)
		{
			if (i > 0)
				writer.Write(", ");

			Declarations[i].Render(writer);
		}
	}
}
