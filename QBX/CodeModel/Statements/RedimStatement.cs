using System;
using System.Collections.Generic;
using System.IO;

namespace QBX.CodeModel.Statements;

public class RedimStatement : DimStatement
{
	public override StatementType Type => StatementType.Redim;

	public bool Preserve { get; set; }

	protected override string StatementName => "REDIM";

	protected override void RenderPreserveFlag(TextWriter writer)
	{
		if (Preserve)
			writer.Write(" PRESERVE");
	}
}
