using System;

using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;

namespace QBX.ExecutionEngine.Compiled.Functions;

public class SpaceFunction : Function
{
	public Evaluable? Argument;

	protected override void SetArgument(int index, Evaluable value)
	{
		if (!value.Type.IsNumeric)
			throw CompilerException.TypeMismatch(value.SourceExpression?.Token);

		Argument = value;
	}

	public override void CollapseConstantSubexpressions()
	{
		CollapseConstantExpression(ref Argument);
	}

	public override DataType Type => DataType.String;

	public override Variable Evaluate(ExecutionContext context, StackFrame stackFrame)
	{
		if (Argument == null)
			throw new Exception("ChrFunction with no Argument");

		var argumentValue = Argument.Evaluate(context, stackFrame);

		int numSpaces;

		try
		{
			numSpaces = (byte)argumentValue.CoerceToInt();
		}
		catch (OverflowException)
		{
			throw RuntimeException.Overflow(Argument.SourceExpression?.Token);
		}

		var stringValue = StringValue.CreateFixedLength(numSpaces);

		stringValue.AsSpan().Slice(0, numSpaces).Fill((byte)' ');

		return new StringVariable(stringValue);
	}
}
