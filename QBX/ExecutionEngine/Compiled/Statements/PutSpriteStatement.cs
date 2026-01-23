using System;

using QBX.ExecutionEngine.Compiled.Expressions;
using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;
using QBX.Firmware;
using QBX.Numbers;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class PutSpriteStatement(CodeModel.Statements.Statement source) : Executable(source)
{
	public bool Step;

	public Evaluable? XExpression;
	public Evaluable? YExpression;

	public Evaluable? SourceExpression;

	public PutSpriteAction ActionVerb;

	public override void Execute(ExecutionContext context, StackFrame stackFrame)
	{
		if (context.VisualLibrary is not GraphicsLibrary visual)
			throw RuntimeException.IllegalFunctionCall(Source);

		if (XExpression == null)
			throw new Exception("PutSpriteExpression with no XExpression");
		if (YExpression == null)
			throw new Exception("PutSpriteExpression with no YExpression");
		if (SourceExpression == null)
			throw new Exception("PutSpriteExpression with no SourceExpression");

		var actionVerb =
			ActionVerb switch
			{
				PutSpriteAction.PixelSet => Firmware.PutSpriteAction.PixelSet,
				PutSpriteAction.PixelSetInverted => Firmware.PutSpriteAction.PixelSet,
				PutSpriteAction.And => Firmware.PutSpriteAction.And,
				PutSpriteAction.Or => Firmware.PutSpriteAction.Or,
				PutSpriteAction.ExclusiveOr => Firmware.PutSpriteAction.ExclusiveOr,

				_ => throw new Exception("Unrecognized ActionVerb value " + ActionVerb)
			};

		var xValue = XExpression.Evaluate(context, stackFrame);
		var yValue = YExpression.Evaluate(context, stackFrame);

		float x = NumberConverter.ToSingle(xValue);
		float y = NumberConverter.ToSingle(yValue);

		if (Step)
		{
			x += visual.LastPoint.X;
			y += visual.LastPoint.Y;
		}

		Execution.Array array;
		int arrayOffset = 0;

		if (SourceExpression is ArrayElementExpression arrayElement)
		{
			arrayElement.EvaluateInParts(context, stackFrame, out array, out var subscripts);

			arrayOffset = array.Subscripts.GetElementIndex(subscripts, arrayElement.SubscriptExpressions);
		}
		else
		{
			var target = SourceExpression.Evaluate(context, stackFrame);

			if (!target.DataType.IsArray)
				throw RuntimeException.TypeMismatch(SourceExpression?.Source);

			array = ((ArrayVariable)target).Array;
		}

		array.EnsurePacked();

		var sourceBytes = array.PackedData.Slice(arrayOffset);

		visual.PutSprite(sourceBytes, actionVerb, x, y);
	}
}
