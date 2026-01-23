using System;

using QBX.ExecutionEngine.Compiled.Expressions;
using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;
using QBX.Firmware;
using QBX.Numbers;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class GetSpriteStatement(CodeModel.Statements.Statement source) : Executable(source)
{
	public bool FromStep;
	public Evaluable? FromXExpression;
	public Evaluable? FromYExpression;

	public bool ToStep;
	public Evaluable? ToXExpression;
	public Evaluable? ToYExpression;

	public Evaluable? TargetExpression;

	public override void Execute(ExecutionContext context, StackFrame stackFrame)
	{
		if (context.VisualLibrary is not GraphicsLibrary visual)
			throw RuntimeException.IllegalFunctionCall(Source);

		if (FromXExpression == null)
			throw new Exception("GetSpriteExpression with no FromXExpression");
		if (FromYExpression == null)
			throw new Exception("GetSpriteExpression with no FromYExpression");
		if (ToXExpression == null)
			throw new Exception("GetSpriteExpression with no ToXExpression");
		if (ToYExpression == null)
			throw new Exception("GetSpriteExpression with no ToYExpression");
		if (TargetExpression == null)
			throw new Exception("GetSpriteExpression with no TargetExpression");

		var fromXValue = FromXExpression.Evaluate(context, stackFrame);
		var fromYValue = FromYExpression.Evaluate(context, stackFrame);

		float fromX = NumberConverter.ToSingle(fromXValue);
		float fromY = NumberConverter.ToSingle(fromYValue);

		if (FromStep)
		{
			fromX += visual.LastPoint.X;
			fromY += visual.LastPoint.Y;
		}

		var toXValue = ToXExpression.Evaluate(context, stackFrame);
		var toYValue = ToYExpression.Evaluate(context, stackFrame);

		float toX = NumberConverter.ToSingle(toXValue);
		float toY = NumberConverter.ToSingle(toYValue);

		if (ToStep)
		{
			toX += fromX;
			toY += fromY;
		}

		Execution.Array array;
		int arrayOffset = 0;

		if (TargetExpression is ArrayElementExpression arrayElement)
		{
			arrayElement.EvaluateInParts(context, stackFrame, out array, out var subscripts);

			arrayOffset = array.Subscripts.GetElementIndex(subscripts, arrayElement.SubscriptExpressions);
		}
		else
		{
			var target = TargetExpression.Evaluate(context, stackFrame);

			if (!target.DataType.IsArray)
				throw RuntimeException.TypeMismatch(TargetExpression?.Source);

			array = ((ArrayVariable)target).Array;
		}

		array.EnsurePacked();

		var targetBytes = array.PackedData.Slice(arrayOffset);

		visual.GetSprite(fromX, fromY, toX, toY, targetBytes);
	}
}
