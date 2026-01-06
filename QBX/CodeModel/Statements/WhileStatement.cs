using System;
using System.IO;

using QBX.CodeModel.Expressions;

namespace QBX.CodeModel.Statements;

public class WhileStatement : Statement
{
	public override StatementType Type => StatementType.While;

	public Expression? Condition { get; set; }

	protected override void RenderImplementation(TextWriter writer)
	{
		if (Condition == null)
			throw new Exception("Internal error: WhileStatement with no condition");

		writer.Write("WHILE ");
		Condition.Render(writer);
	}
}
