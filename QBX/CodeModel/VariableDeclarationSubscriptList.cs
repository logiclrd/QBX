using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace QBX.CodeModel;

public class VariableDeclarationSubscriptList : IRenderableCode
{
	public List<VariableDeclarationSubscript> Subscripts { get; } = new List<VariableDeclarationSubscript>();

	public bool Any() => Subscripts.Any();

	public void Add(VariableDeclarationSubscript subscript)
	{
		Subscripts.Add(subscript);
	}

	public void Render(TextWriter writer)
	{
		writer.Write("(");

		for (int i = 0; i < Subscripts.Count; i++)
		{
			if (i > 0)
				writer.Write(", ");

			Subscripts[i].Render(writer);
		}

		writer.Write(")");
	}
}
