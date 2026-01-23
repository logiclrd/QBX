using System;
using System.Collections.Generic;
using System.Linq;

using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;

namespace QBX.ExecutionEngine.Compiled.Functions;

public class InStrFunction : Function
{
	public Evaluable? StartExpression;
	public Evaluable? StringExpression;
	public Evaluable? SearchForExpression;

	protected override int MinArgumentCount => 2;
	protected override int MaxArgumentCount => 3;

	public override void SetArguments(IEnumerable<Evaluable> arguments)
	{
		var argList = arguments.ToList();

		switch (argList.Count)
		{
			case 2:
			{
				SetArgument(1, argList[0]);
				SetArgument(2, argList[1]);
				break;
			}
			case 3:
			{
				SetArgument(0, argList[0]);
				SetArgument(1, argList[1]);
				SetArgument(2, argList[2]);
				break;
			}

			default: throw CompilerException.ArgumentCountMismatch(Source?.Token);
		}
	}

	protected override void SetArgument(int index, Evaluable value)
	{
		switch (index)
		{
			case 0:
				if (!value.Type.IsNumeric)
					throw CompilerException.TypeMismatch(value.Source);

				StartExpression = value;
				break;
			case 1:
				if (!value.Type.IsString)
					throw CompilerException.TypeMismatch(value.Source);

				StringExpression = value;
				break;
			case 2:
				if (!value.Type.IsString)
					throw CompilerException.TypeMismatch(value.Source);

				SearchForExpression = value;
				break;
		}
	}

	public override void CollapseConstantSubexpressions()
	{
		CollapseConstantExpression(ref StartExpression);
		CollapseConstantExpression(ref StringExpression);
		CollapseConstantExpression(ref SearchForExpression);
	}

	public override DataType Type => DataType.Integer;

	public override Variable Evaluate(ExecutionContext context, StackFrame stackFrame)
	{
		if (StringExpression == null)
			throw new Exception("InStrFunction with no StringExpression");
		if (SearchForExpression == null)
			throw new Exception("InStrFunction with no SearchForExpression");

		int start = 1;

		if (StartExpression != null)
			start = StartExpression.EvaluateAndCoerceToInt(context, stackFrame);

		var stringVariable = (StringVariable)StringExpression.Evaluate(context, stackFrame);
		var searchForVariable = (StringVariable)SearchForExpression.Evaluate(context, stackFrame);

		var stringValue = stringVariable.Value;
		var searchForValue = searchForVariable.Value;

		try
		{
			return new IntegerVariable((short)(stringValue.IndexOf(searchForValue, start - 1) + 1));
		}
		catch (OverflowException)
		{
			throw RuntimeException.Overflow(Source?.Token);
		}
	}
}
