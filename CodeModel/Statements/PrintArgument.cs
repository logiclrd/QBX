using QBX.CodeModel;
using QBX.CodeModel.Expressions;

namespace QBX.CodeModel.Statements;

public class PrintArgument : IRenderableCode
{
	public Expression? Expression { get; set; }
	public PrintCursorAction CursorAction { get; set; }

	public void Render(TextWriter writer)
	{
		Expression?.Render(writer);

		switch (CursorAction)
		{
			case PrintCursorAction.None: writer.Write(';'); break;
			case PrintCursorAction.NextZone: writer.Write(','); break;
		}
	}
}
