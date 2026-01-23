using System;

using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;
using QBX.Firmware;
using QBX.Numbers;

namespace QBX.ExecutionEngine.Compiled.Statements;

public abstract class PaintStatement(CodeModel.Statements.Statement source) : Executable(source)
{
	public bool Step;
	public Evaluable? XExpression;
	public Evaluable? YExpression;
	public Evaluable? PaintExpression;
	public Evaluable? BorderExpression;

	public static PaintStatement Construct(CodeModel.Statements.Statement source, bool step, Evaluable xExpression, Evaluable yExpression, Evaluable? paintExpression, Evaluable? borderExpression, Evaluable? backgroundExpression = null)
	{
		if (!xExpression.Type.IsNumeric)
			throw CompilerException.TypeMismatch(xExpression?.Source);
		if (!yExpression.Type.IsNumeric)
			throw CompilerException.TypeMismatch(yExpression?.Source);

		if ((backgroundExpression == null)
		 && ((paintExpression == null) || paintExpression.Type.IsNumeric))
		{
			// Paint with a simple numeric attribute.
			if ((paintExpression != null) && !paintExpression.Type.IsNumeric)
				throw CompilerException.TypeMismatch(paintExpression?.Source);
			if ((borderExpression != null) && !borderExpression.Type.IsNumeric)
				throw CompilerException.TypeMismatch(paintExpression?.Source);

			var translatedStatement = new PaintSolidFill(source);

			translatedStatement.Step = step;
			translatedStatement.XExpression = xExpression;
			translatedStatement.YExpression = yExpression;
			translatedStatement.PaintExpression = paintExpression;
			translatedStatement.BorderExpression = borderExpression;

			return translatedStatement;
		}
		else
		{
			// Paint with a pattern (string expression).
			if ((paintExpression != null) && !paintExpression.Type.IsString)
				throw CompilerException.TypeMismatch(paintExpression?.Source);
			if ((borderExpression != null) && !borderExpression.Type.IsNumeric)
				throw CompilerException.TypeMismatch(borderExpression?.Source);
			if ((backgroundExpression != null) && !backgroundExpression.Type.IsString)
				throw CompilerException.TypeMismatch(backgroundExpression?.Source);

			PaintPatternFillStatement translatedStatement;

			if (backgroundExpression == null)
				translatedStatement = new PaintPatternFillStatement(source);
			else
			{
				translatedStatement =
					new PaintPatternFillBackgroundStatement(source)
					{
						BackgroundExpression = backgroundExpression
					};
			}

			translatedStatement.Step = step;
			translatedStatement.XExpression = xExpression;
			translatedStatement.YExpression = yExpression;
			translatedStatement.PaintExpression = paintExpression;
			translatedStatement.BorderExpression = borderExpression;

			return translatedStatement;
		}
	}
}

public class PaintSolidFill(CodeModel.Statements.Statement source) : PaintStatement(source)
{
	public override void Execute(ExecutionContext context, StackFrame stackFrame)
	{
		if (XExpression == null)
			throw new Exception("PaintStatement with no XExpression");
		if (YExpression == null)
			throw new Exception("PaintStatement with no YExpression");

		if (context.VisualLibrary is not GraphicsLibrary visual)
			throw RuntimeException.IllegalFunctionCall(Source);

		var xValue = XExpression.Evaluate(context, stackFrame);
		var yValue = YExpression.Evaluate(context, stackFrame);

		float x = NumberConverter.ToSingle(xValue);
		float y = NumberConverter.ToSingle(yValue);

		if (Step)
		{
			x += visual.LastPoint.X;
			y += visual.LastPoint.Y;
		}

		int paint = visual.DrawingAttribute;

		if (PaintExpression != null)
			paint = PaintExpression.EvaluateAndCoerceToInt(context, stackFrame);

		int border = visual.DrawingAttribute;

		if (BorderExpression != null)
			border = BorderExpression.EvaluateAndCoerceToInt(context, stackFrame);

		visual.BorderFill(x, y, border, paint);
	}
}

public class PaintPatternFillStatement(CodeModel.Statements.Statement source) : PaintStatement(source)
{
	public sealed override void Execute(ExecutionContext context, StackFrame stackFrame)
	{
		if (XExpression == null)
			throw new Exception($"{GetType().Name} with no XExpression");
		if (YExpression == null)
			throw new Exception($"{GetType().Name} with no YExpression");
		if (PaintExpression == null)
			throw new Exception($"{GetType().Name} with no PaintExpression");

		if (context.VisualLibrary is not GraphicsLibrary visual)
			throw RuntimeException.IllegalFunctionCall(Source);

		var xValue = XExpression.Evaluate(context, stackFrame);
		var yValue = YExpression.Evaluate(context, stackFrame);

		float x = NumberConverter.ToSingle(xValue);
		float y = NumberConverter.ToSingle(yValue);

		if (Step)
		{
			x += visual.LastPoint.X;
			y += visual.LastPoint.Y;
		}

		var paintValue = (StringVariable)PaintExpression.Evaluate(context, stackFrame);

		var fill = paintValue.Value;

		int border = visual.DrawingAttribute;

		if (BorderExpression != null)
			border = BorderExpression.EvaluateAndCoerceToInt(context, stackFrame);

		ExecuteImplementation(visual, x, y, border, fill.ToByteArray(), context, stackFrame);
	}

	protected virtual void ExecuteImplementation(GraphicsLibrary visual, float x, float y, int border, byte[] fill, ExecutionContext context, StackFrame stackFrame)
	{
		visual.BorderFill(x, y, border, fill, backgroundPatternBytes: null);
	}
}

public class PaintPatternFillBackgroundStatement(CodeModel.Statements.Statement source) : PaintPatternFillStatement(source)
{
	public Evaluable? BackgroundExpression;

	protected override void ExecuteImplementation(GraphicsLibrary visual, float x, float y, int border, byte[] fill, ExecutionContext context, StackFrame stackFrame)
	{
		if (BackgroundExpression == null)
			throw new Exception("PaintPatternFillBackgroundStatement with no BackgroundExpression");

		var backgroundValue = (StringVariable)BackgroundExpression.Evaluate(context, stackFrame);

		var background = backgroundValue.Value.ToByteArray();

		bool previousIsMatch = false;

		// For reasons specific to QBASIC's PAINT algorithm, Illegal Function Call results
		// if more than two consecutive pattern bytes match the corresponding background
		// bytes.
		for (int i = 0, l = Math.Min(fill.Length, background.Length); i < l; i++)
		{
			bool isMatch = fill[i] == background[i];

			if (previousIsMatch && isMatch)
				throw RuntimeException.IllegalFunctionCall(Source);

			previousIsMatch = isMatch;
		}

		visual.BorderFill(x, y, border, fill, background);
	}
}
