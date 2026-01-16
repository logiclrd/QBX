using QBX.ExecutionEngine.Execution;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class SoftKeyListStatement(CodeModel.Statements.Statement source) : Executable(source)
{
	public override void Execute(ExecutionContext context, StackFrame stackFrame)
	{
		var visual = context.VisualLibrary;

		for (int i = 1; i <= 12; i++)
		{
			visual.WriteText("F");
			visual.WriteText(i.ToString());

			if (i < 10)
				visual.WriteText("  ");
			else
				visual.WriteText(' ');

			if (context.RuntimeState.SoftKeyMacros[i - 1] is StringValue macro)
				visual.WriteText(macro.AsSpan());

			visual.NewLine();
		}
	}
}
