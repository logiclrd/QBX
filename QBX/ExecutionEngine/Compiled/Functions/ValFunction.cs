using System;
using System.Text.RegularExpressions;
using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;

namespace QBX.ExecutionEngine.Compiled.Functions;

public class ValFunction : Function
{
	public Evaluable? Argument;

	protected override void SetArgument(int index, Evaluable value)
	{
		if (!value.Type.IsString)
			throw CompilerException.TypeMismatch(value.SourceExpression?.Token);

		Argument = value;
	}

	public override void CollapseConstantSubexpressions()
	{
		CollapseConstantExpression(ref Argument);
	}

	public override DataType Type => DataType.Double;

	public override Variable Evaluate(ExecutionContext context, StackFrame stackFrame)
	{
		if (Argument == null)
			throw new Exception("ValFunction with no Argument");

		var type = Argument.Type;

		StringValue argumentValue;

		if (type.IsString)
			argumentValue = ((StringVariable)Argument.Evaluate(context, stackFrame)).Value;
		else
			throw new Exception("Internal error");

		var parsedValue = PermissiveParse(argumentValue.ToString());

		return new DoubleVariable(parsedValue);
	}

	static readonly Regex PermissiveParseRegex = new Regex(
		@"^[ \t\r\n]*(?<ParseInput>(\d+\.\d*|\.\d*|\d+)([dDeE][+-]?\d{1,3})?)",
		RegexOptions.Singleline);

	internal static double PermissiveParse(string str)
	{
		if (str.Length > 128)
			str = str.Substring(0, 128);

		var match = PermissiveParseRegex.Match(str);

		if (match.Success)
		{
			var parseInput = match.Captures[0].ValueSpan;

			int doubleExponentCharacterIndex = parseInput.IndexOf('d');

			if (doubleExponentCharacterIndex < 0)
				doubleExponentCharacterIndex = parseInput.IndexOf('D');

			if (doubleExponentCharacterIndex >= 0)
			{
				var clone = new char[parseInput.Length];

				parseInput.CopyTo(clone);
				parseInput = clone;

				clone[doubleExponentCharacterIndex] = 'E';
			}

			while ((parseInput.Length > 0) && (parseInput[0] == '0'))
				parseInput = parseInput.Slice(1);

			if (parseInput.Length == 0)
				return 0;

			if (double.TryParse(parseInput, out var parsedValue))
				return parsedValue;
		}

		return 0;
	}
}
