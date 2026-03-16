using System;
using System.Collections.Generic;
using System.IO;

using QBX.CodeModel.Expressions;

namespace QBX.CodeModel.Statements;

public class EraseStatement : Statement
{
	public override StatementType Type => StatementType.Erase;

	public List<Expression> ArrayExpressions { get; } = new List<Expression>();

	protected override void RenderImplementation(TextWriter writer)
	{
		if (ArrayExpressions.Count == 0)
			throw new Exception("Internal error: EraseStatement with no ArrayExpressions");

		writer.Write("ERASE ");

		for (int i = 0; i < ArrayExpressions.Count; i++)
		{
			if (i > 0)
				writer.Write(", ");

			ArrayExpressions[i].Render(writer);
		}
	}
}
