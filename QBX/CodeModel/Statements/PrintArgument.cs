using System;
using System.IO;

using QBX.CodeModel.Expressions;

namespace QBX.CodeModel.Statements;

public class PrintArgument : IRenderableCode
{
	public PrintArgumentType ArgumentType = PrintArgumentType.Value;
	public Expression? Expression { get; set; }
	public PrintCursorAction CursorAction { get; set; }

	public void Render(TextWriter writer)
	{
		switch (ArgumentType)
		{
			case PrintArgumentType.Value:
				Expression?.Render(writer);
				break;
			case PrintArgumentType.Tab:
				writer.Write("TAB(");
				Expression?.Render(writer);
				writer.Write(')');
				break;
			case PrintArgumentType.Space:
				writer.Write("TAB(");
				Expression?.Render(writer);
				writer.Write(')');
				break;

			default:
				throw new Exception("Internal error: Unrecognized PrintArgumentType " + ArgumentType);
		}

		switch (CursorAction)
		{
			case PrintCursorAction.None: writer.Write(';'); break;
			case PrintCursorAction.NextZone: writer.Write(','); break;
		}
	}
}
