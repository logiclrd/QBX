using QBX.ExecutionEngine.Execution;
using QBX.Firmware;
using QBX.Utility;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class ResetGraphicsViewportStatement(CodeModel.Statements.GraphicsViewportStatement source) : GraphicsViewportStatement(source)
{
	public override void Execute(ExecutionContext context, StackFrame stackFrame)
	{
		if (context.VisualLibrary is not GraphicsLibrary visual)
			throw RuntimeException.IllegalFunctionCall(Source);

		visual.ResetClip();

		visual.CoordinateSystem.ResetViewport();

		visual.LastPoint = visual.CoordinateSystem.ViewportCentre;
	}
}
