using QBX.ExecutionEngine.Execution;
using QBX.Firmware;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class ResetGraphicsWindowStatement(CodeModel.Statements.Statement source) : GraphicsWindowStatement(source)
{
	public override void Execute(ExecutionContext context, StackFrame stackFrame)
	{
		if (context.VisualLibrary is not GraphicsLibrary visual)
			throw RuntimeException.IllegalFunctionCall(Source);

		int w = visual.Width;
		int h = visual.Height;

		visual.Window = new Window(0, 0, w, h, w, h);
	}
}
