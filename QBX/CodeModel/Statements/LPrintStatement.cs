using System.Collections.Generic;
using System.IO;

namespace QBX.CodeModel.Statements;

public class LPrintStatement : Statement
{
	public override StatementType Type => StatementType.LPrint;

	public List<PrintArgument> Arguments { get; } = new List<PrintArgument>();

	protected override void RenderImplementation(TextWriter writer)
	{
		writer.Write("LPRINT ");

		for (int i = 0; i < Arguments.Count; i++)
		{
			Arguments[i].Expression?.Render(writer);

			switch (Arguments[i].CursorAction)
			{
				case PrintCursorAction.None: writer.Write(';'); break;
				case PrintCursorAction.NextZone: writer.Write(','); break;
			}
		}
	}
}
