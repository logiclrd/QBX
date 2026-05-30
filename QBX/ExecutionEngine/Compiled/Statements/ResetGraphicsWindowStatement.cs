using QBX.ExecutionEngine.Execution;
using QBX.Firmware;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class ResetGraphicsWindowStatement(CodeModel.Statements.WindowStatement source) : GraphicsWindowStatement(source)
{
	public override void Execute(ExecutionContext context, StackFrame stackFrame)
	{
		if (context.VisualLibrary is not GraphicsLibrary visual)
			throw RuntimeException.IllegalFunctionCall(Source);

		var lastPointScreen = visual.CoordinateSystem.TranslateWindowToScreen(
			visual.LastPoint.X,
			visual.LastPoint.Y);

		visual.CoordinateSystem.ResetWindow();

		visual.LastPoint = visual.CoordinateSystem.TranslateScreenToWindow(
			lastPointScreen.X,
			lastPointScreen.Y);
	}
}
