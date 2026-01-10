namespace QBX.ExecutionEngine.Compiled.Statements;

public class PrintArgument
{
	public Evaluable? Expression;
	public PrintArgumentType ArgumentType = PrintArgumentType.Value;
	public PrintCursorAction CursorAction = PrintCursorAction.NextLine;
}
