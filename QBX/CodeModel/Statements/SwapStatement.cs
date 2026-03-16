using System;
using System.IO;

using QBX.CodeModel.Expressions;

namespace QBX.CodeModel.Statements;

public class SwapStatement : Statement
{
	public override StatementType Type => StatementType.Swap;

	public Expression? Variable1Expression { get; set; }
	public Expression? Variable2Expression { get; set; }

	protected override void RenderImplementation(TextWriter writer)
	{
		if (Variable1Expression == null)
			throw new Exception("Internal error: SwapStatement with no Variable1Expression");
		if (Variable2Expression == null)
			throw new Exception("Internal error: SwapStatement with no Variable2Expression");

		writer.Write("SWAP ");
		Variable1Expression.Render(writer);
		writer.Write(", ");
		Variable2Expression.Render(writer);
	}
}
