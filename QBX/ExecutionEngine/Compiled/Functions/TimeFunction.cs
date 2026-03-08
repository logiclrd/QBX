using System;
using System.Linq;

using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;

namespace QBX.ExecutionEngine.Compiled.Functions;

public class TimeFunction : Function
{
	public override DataType Type => DataType.String;

	protected override int MinArgumentCount => 0;
	protected override int MaxArgumentCount => 0;

	public override Variable Evaluate(ExecutionContext context, StackFrame stackFrame)
	{
		var date = context.Machine.SystemClock.Now;

		string formattedTime = date.ToString("HH:mm:ss");

		return new StringVariable(new StringValue(formattedTime));
	}

	public override bool IsAssignable => true;

	public override void EvaluateAndAssignTo(ExecutionContext context, StackFrame stackFrame, Variable newValue)
	{
		if (newValue is not StringVariable stringVariable)
			throw RuntimeException.TypeMismatch(Source);

		string newTimeString = stringVariable.ValueString;

		var newTimeSpan = newTimeString.AsSpan();

		// Valid strings are either 2 characters ("HH" implied :00:00),
		// 5 characters ("HH:mm" implied :00) or 8 characters ("HH:mm:ss").
		if ((newTimeSpan.Length != 2) && (newTimeSpan.Length != 5) && (newTimeSpan.Length != 8))
			throw RuntimeException.IllegalFunctionCall(Source);

		// Valid strings have ":" separators in exactly these positions.
		// All other characters in valid strings are digits.
		for (int i = 0; i < newTimeSpan.Length; i++)
		{
			if ((i == 2) || (i == 5))
			{
				if (newTimeSpan[i] != ':')
					throw RuntimeException.IllegalFunctionCall(Source);
			}
			else
			{
				if (!char.IsAsciiDigit(newTimeSpan[i]))
					throw RuntimeException.IllegalFunctionCall(Source);
			}
		}

		int hour = int.Parse(newTimeSpan.Slice(0, 2));
		int minute = (newTimeSpan.Length >= 3) ? int.Parse(newTimeSpan.Slice(3, 2)) : 0;
		int second = (newTimeSpan.Length >= 6) ? int.Parse(newTimeSpan.Slice(6)) : 0;

		// Let DateTime check if the numbers make sense.
		DateTime newTime;

		try
		{
			newTime = new DateTime(year: 2000, month: 1, day: 1, hour, minute, second);
		}
		catch
		{
			throw RuntimeException.IllegalFunctionCall(Source);
		}

		// System clock works in combined dates & times.
		newTime = context.Machine.SystemClock.Now.Date + newTime.TimeOfDay;

		context.Machine.SystemClock.SetCurrentTime(newTime);
	}
}
