using System;

using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;
using QBX.Firmware.Fonts;

namespace QBX.ExecutionEngine.Compiled.Functions;

public class LCaseFunction : Function
{
	public Evaluable? ArgumentExpression;

	protected override void SetArgument(int index, Evaluable value)
	{
		if (!value.Type.IsString)
			throw CompilerException.TypeMismatch(value.SourceExpression?.Token);

		ArgumentExpression = value;
	}

	public override void CollapseConstantSubexpressions()
	{
		ArgumentExpression?.CollapseConstantSubexpressions();
	}

	public override DataType Type => DataType.String;

	static CP437Encoding s_cp437 = new CP437Encoding(ControlCharacterInterpretation.Semantic);

	public override Variable Evaluate(ExecutionContext context, StackFrame stackFrame)
	{
		if (ArgumentExpression == null)
			throw new Exception("LCaseFunction with no ArgumentExpression");

		var stringVariable = (StringVariable)ArgumentExpression.Evaluate(context, stackFrame);

		var stringValue = stringVariable.ValueSpan;

		StringVariable? translated = null;
		StringValue? translatedValue = null;

		for (int i = 0, l = stringValue.Length; i < l; i++)
		{
			if (s_cp437.IsAsciiLetterUpper(stringValue[i]))
			{
				if (translatedValue == null)
				{
					translatedValue = new StringValue(stringValue);
					translated = new StringVariable(translatedValue, adopt: true);
				}

				translatedValue[i] = s_cp437.ToLower(stringValue[i]);
			}
		}

		return translated ?? stringVariable;
	}
}
