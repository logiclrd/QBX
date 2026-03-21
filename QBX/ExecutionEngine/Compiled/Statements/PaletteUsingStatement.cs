using System;

using QBX.ExecutionEngine.Compiled.Expressions;
using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;
using QBX.Firmware;

using Array = QBX.ExecutionEngine.Execution.Array;

namespace QBX.ExecutionEngine.Compiled.Statements;

public abstract class PaletteUsingStatement(CodeModel.Statements.PaletteStatement source) : Executable(source)
{
	protected abstract void GetArrayAndIndex(ExecutionContext context, StackFrame stackFrame, out Array array, out int index);

	public override void Execute(ExecutionContext context, StackFrame stackFrame)
	{
		GetArrayAndIndex(context, stackFrame, out var array, out var index);

		int numAttributes =
			context.VisualLibrary is GraphicsLibrary graphicsLibrary
			? graphicsLibrary.MaximumAttribute + 1
			: 16;

		int maxColour = context.RuntimeState.MaximumColour;

		int[] translated = new int[numAttributes];

		for (int attribute = 0; attribute < numAttributes; attribute++)
		{
			if (array.Elements[index + attribute] is Variable value)
			{
				int colour = value.CoerceToInt(context: null);

				if ((maxColour > 0) && (colour > maxColour))
					throw RuntimeException.IllegalFunctionCall(Source);

				translated[attribute] = colour;
			}
		}

		for (int attribute = 0; attribute < numAttributes; attribute++)
		{
			int colour = translated[attribute];

			if (colour == -1)
				continue;

			PaletteStatement.AlterPalette(attribute, colour, context.RuntimeState.PaletteMode, context.Machine);
		}
	}
}

public class PaletteUsingArrayStatement(CodeModel.Statements.PaletteStatement source) : PaletteUsingStatement(source)
{
	public int ArrayVariableIndex;

	protected override void GetArrayAndIndex(ExecutionContext context, StackFrame stackFrame, out Array array, out int index)
	{
		if ((ArrayVariableIndex < 0) || (ArrayVariableIndex >= stackFrame.Variables.Length))
			throw new Exception("Internal error: invalid ArrayVariableIndex");

		if (stackFrame.Variables[ArrayVariableIndex] is not ArrayVariable arrayVariable)
			throw new Exception("Internal error: variable at ArrayVariableIndex is not an array");

		array = arrayVariable.Array;
		index = 0;
	}
}

public class PaletteUsingArrayElementStatement(CodeModel.Statements.PaletteStatement source) : PaletteUsingStatement(source)
{
	public ArrayElementExpression? ArrayElementExpression;

	protected override void GetArrayAndIndex(ExecutionContext context, StackFrame stackFrame, out Array array, out int index)
	{
		if (ArrayElementExpression == null)
			throw new Exception("PaletteUsingArrayElementStatement with no ArrayElementExpression");

		var arrayVariable = ArrayElementExpression.ArrayExpression.Evaluate(context, stackFrame) as ArrayVariable;

		if (arrayVariable == null)
			throw new Exception("ArrayExpression did not evaluate to an ArrayVariable");

		var subscripts = new Variable[ArrayElementExpression.SubscriptExpressions.Count];

		for (int i = 0; i < subscripts.Length; i++)
			subscripts[i] = ArrayElementExpression.SubscriptExpressions[i].Evaluate(context, stackFrame);

		array = arrayVariable.Array;
		index = array.Subscripts.GetElementIndex(subscripts, ArrayElementExpression.SubscriptExpressions);
	}
}
