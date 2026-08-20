using QBX.ExecutionEngine.Execution;
using QBX.Firmware;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class ResetGraphicsViewportStatement(CodeModel.Statements.GraphicsViewportStatement source) : GraphicsViewportStatement(source)
{
	protected override void ExecuteImplementation(ExecutionContext context, StackFrame stackFrame)
	{
		if (context.VisualLibrary is not GraphicsLibrary visual)
			throw RuntimeException.IllegalFunctionCall(Source);

		visual.ResetClip();

		visual.CoordinateSystem.ResetViewport();

		visual.LastPoint = visual.CoordinateSystem.ViewportCentre;

		context.RuntimeState.HaveGraphicsViewport = false;
	}
}
