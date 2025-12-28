/*
 * TODO
 * 
namespace QBX.Tests.Parser.Statements;

public class LPrintStatement
{
	public override StatementType Type => StatementType.LPrint;

	public List<PrintArgument> Arguments { get; } = new List<PrintArgument>();

	public override void Render(TextWriter writer)
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

*/
