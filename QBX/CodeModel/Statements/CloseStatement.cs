using System.Collections.Generic;
using System.IO;

namespace QBX.CodeModel.Statements;

public class CloseStatement : Statement
{
	public override StatementType Type => StatementType.Close;

	public List<CloseArgument> Arguments { get; } = new List<CloseArgument>();

	protected override void RenderImplementation(TextWriter writer)
	{
		writer.Write("CLOSE ");

		for (int i = 0; i < Arguments.Count; i++)
		{
			if (i > 0)
				writer.Write(", ");

			Arguments[i].Render(writer);
		}
	}
}
