using System;
using System.Linq;

using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;

namespace QBX.ExecutionEngine.Compiled.Functions;

public class DateFunction : Function
{
	public override DataType Type => DataType.String;

	protected override int MinArgumentCount => 0;
	protected override int MaxArgumentCount => 0;

	public override Variable Evaluate(ExecutionContext context, StackFrame stackFrame)
	{
		var date = context.Machine.SystemClock.Now;

		string formattedDate = date.ToString("MM-dd-yyyy");

		return new StringVariable(new StringValue(formattedDate));
	}

	public override bool IsAssignable => true;

	static readonly char[] Separators = ['-', '/'];

	public override void EvaluateAndAssignTo(ExecutionContext context, StackFrame stackFrame, Variable newValue)
	{
		if (newValue is not StringVariable stringVariable)
			throw RuntimeException.TypeMismatch(Source);

		string newDateString = stringVariable.ValueString;

		var newDateSpan = newDateString.AsSpan();

		// Valid strings are either 8 characters ("mm-dd-yy") or 10 characters ("mm-dd-yyyy").
		if ((newDateSpan.Length != 8) && (newDateSpan.Length != 10))
			throw RuntimeException.IllegalFunctionCall(Source);

		// Valid strings have "-" or "/" separators in exactly these positions. They can be mixed and matched.
		// All other characters in valid strings are digits.
		for (int i = 0; i < newDateSpan.Length; i++)
		{
			if ((i == 2) || (i == 5))
			{
				if (!Separators.Contains(newDateSpan[i]))
					throw RuntimeException.IllegalFunctionCall(Source);
			}
			else
			{
				if (!char.IsAsciiDigit(newDateSpan[i]))
					throw RuntimeException.IllegalFunctionCall(Source);
			}
		}

		int month = int.Parse(newDateSpan.Slice(0, 2));
		int day = int.Parse(newDateSpan.Slice(3, 2));
		int year = int.Parse(newDateSpan.Slice(6));

		// If only two digits are supplied for the year, then the 20th century is implied.
		if (newDateSpan.Length == 8)
			year += 1900;

		// Let DateTime check if the numbers make sense.
		DateTime newDate;

		try
		{
			newDate = new DateTime(year, month, day);
		}
		catch
		{
			throw RuntimeException.IllegalFunctionCall(Source);
		}

		// System clock works in combined dates & times.
		newDate += context.Machine.SystemClock.Now.TimeOfDay;

		context.Machine.SystemClock.SetCurrentTime(newDate);
	}
}
