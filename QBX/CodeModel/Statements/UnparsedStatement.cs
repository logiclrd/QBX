using System;
using System.Collections.Generic;
using System.Text;

namespace QBX.CodeModel.Statements;

public class UnparsedStatement : Statement
{
	public override StatementType Type => StatementType.Unparsed;

	public string Text { get; set; } = "";

	protected override void RenderImplementation(TextWriter writer)
	{
		writer.Write(Text);
	}
}
