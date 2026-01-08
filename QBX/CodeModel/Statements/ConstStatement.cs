using System.Collections.Generic;
using System.IO;

namespace QBX.CodeModel.Statements;

public class ConstStatement : Statement
{
	public override StatementType Type => StatementType.Const;

	public List<ConstDefinition> Definitions { get; set; }

	public ConstStatement(List<ConstDefinition> definitions)
	{
		Definitions = definitions;
	}

	protected override void RenderImplementation(TextWriter writer)
	{
		writer.Write("CONST ");

		for (int i = 0; i < Definitions.Count; i++)
		{
			if (i > 0)
				writer.Write(", ");

			Definitions[i].Render(writer);
		}
	}
}
