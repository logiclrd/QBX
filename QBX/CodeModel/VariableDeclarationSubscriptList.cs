using System;
using System.Collections.Generic;
using System.IO;

namespace QBX.CodeModel;

public class VariableDeclarationSubscriptList : List<VariableDeclarationSubscript>, IRenderableCode
{
	public void Render(TextWriter writer)
	{
		writer.Write("(");

		for (int i = 0; i < Count; i++)
		{
			if (i > 0)
				writer.Write(", ");

			this[i].Render(writer);
		}

		writer.Write(")");
	}
}
