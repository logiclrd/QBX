using System.Collections.Generic;
using System.IO;

namespace QBX.CodeModel.Statements;

public class ConstStatement : Statement
{
	public override StatementType Type => StatementType.Const;

	public List<ConstDeclaration> Declarations { get; set; }

	public ConstStatement(List<ConstDeclaration> declarations)
	{
		Declarations = declarations;
	}

	protected override void RenderImplementation(TextWriter writer)
	{
		writer.Write("CONST ");

		for (int i = 0; i < Declarations.Count; i++)
		{
			if (i > 0)
				writer.Write(", ");

			Declarations[i].Render(writer);
		}
	}
}
