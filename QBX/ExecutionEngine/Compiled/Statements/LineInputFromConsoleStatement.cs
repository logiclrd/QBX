using QBX.ExecutionEngine.Execution;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class LineInputFromConsoleStatement(string? promptString, bool echoNewline, CodeModel.Statements.LineInputStatement source) : LineInputStatement(source)
{
	protected override void EmitPrompt(ExecutionContext context)
	{
		if (promptString != null)
			context.VisualLibrary.WriteText(promptString);
	}

	protected override StringValue ReadLine(ExecutionContext context)
	{
		return new StringValue(context.VisualLibrary.ReadLine(context.Machine.Keyboard, echoNewline));
	}
}
