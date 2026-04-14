using System;

using QBX.ExecutionEngine.Execution;
using QBX.Firmware;
using QBX.Numbers;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class SetGraphicsWindowStatement(CodeModel.Statements.WindowStatement source) : GraphicsWindowStatement(source)
{
	public bool UseScreenCoordinates;

	public Evaluable? X1Expression;
	public Evaluable? Y1Expression;
	public Evaluable? X2Expression;
	public Evaluable? Y2Expression;

	public override void Execute(ExecutionContext context, StackFrame stackFrame)
	{
		if (context.VisualLibrary is not GraphicsLibrary visual)
			throw RuntimeException.IllegalFunctionCall(Source);

		if (X1Expression == null)
			throw new Exception("SetGraphicsWindowStatement with no X1Expression");
		if (Y1Expression == null)
			throw new Exception("SetGraphicsWindowStatement with no Y1Expression");
		if (X2Expression == null)
			throw new Exception("SetGraphicsWindowStatement with no X2Expression");
		if (Y2Expression == null)
			throw new Exception("SetGraphicsWindowStatement with no Y2Expression");

		var x1Value = X1Expression.Evaluate(context, stackFrame);
		var y1Value = Y1Expression.Evaluate(context, stackFrame);
		var x2Value = X2Expression.Evaluate(context, stackFrame);
		var y2Value = Y2Expression.Evaluate(context, stackFrame);

		float x1 = NumberConverter.ToSingle(x1Value);
		float y1 = NumberConverter.ToSingle(y1Value);
		float x2 = NumberConverter.ToSingle(x2Value);
		float y2 = NumberConverter.ToSingle(y2Value);

		if (!UseScreenCoordinates)
			(y1, y2) = (y2, y1);

		int w = visual.Width;
		int h = visual.Height;

		visual.Window = new Window(x1, y1, x2, y2, w, h);
	}
}
