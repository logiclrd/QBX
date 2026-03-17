using System;

using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;
using QBX.Firmware.Fonts;

namespace QBX.ExecutionEngine.Compiled.Functions;

public class UCaseFunction : Function
{
	public Evaluable? ArgumentExpression;

	protected override void SetArgument(int index, Evaluable value)
	{
		if (!value.Type.IsString)
			throw CompilerException.TypeMismatch(value.Source);

		ArgumentExpression = value;
	}

	public override void CollapseConstantSubexpressions()
	{
		ArgumentExpression?.CollapseConstantSubexpressions();
	}

	public override DataType Type => DataType.String;

	public override Variable Evaluate(ExecutionContext context, StackFrame stackFrame)
	{
		if (ArgumentExpression == null)
			throw new Exception("UCaseFunction with no ArgumentExpression");

		var stringVariable = (StringVariable)ArgumentExpression.Evaluate(context, stackFrame);

		var stringValue = stringVariable.ValueSpan;

		StringVariable? translated = null;
		StringValue? translatedValue = null;

		for (int i = 0, l = stringValue.Length; i < l; i++)
		{
			if (CP437Encoding.IsAsciiLetterLower(stringValue[i]))
			{
				if (translatedValue == null)
				{
					translatedValue = new StringValue(stringValue);
					translated = new StringVariable(translatedValue, adopt: true);
				}

				translatedValue[i] = CP437Encoding.ToUpper(stringValue[i]);
			}
		}

		return translated ?? stringVariable;
	}
}
