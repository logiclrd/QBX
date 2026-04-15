using QBX.ExecutionEngine.Execution;
using QBX.Firmware;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class ResetGraphicsWindowStatement(CodeModel.Statements.WindowStatement source) : GraphicsWindowStatement(source)
{
	public override void Execute(ExecutionContext context, StackFrame stackFrame)
	{
		if (context.VisualLibrary is not GraphicsLibrary visual)
			throw RuntimeException.IllegalFunctionCall(Source);

		visual.CoordinateSystem.ResetWindow();
	}
}
