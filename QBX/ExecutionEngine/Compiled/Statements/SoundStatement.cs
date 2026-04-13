using System;

using QBX.ExecutionEngine.Execution;
using QBX.Numbers;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class SoundStatement(CodeModel.Statements.Statement source)
	: Executable(source)
{
	public Evaluable? FrequencyExpression;
	public Evaluable? DurationExpression;

	public override void Execute(ExecutionContext context, StackFrame stackFrame)
	{
		if (FrequencyExpression == null)
			throw new Exception("SoundStatement with no FrequencyExpression");
		if (DurationExpression == null)
			throw new Exception("SoundStatement with no DurationExpression");

		int frequency = FrequencyExpression.EvaluateAndCoerceToInt(context, stackFrame);

		var durationValue = DurationExpression.Evaluate(context, stackFrame);

		double duration = NumberConverter.ToDouble(durationValue);

		if (duration < 0)
			throw RuntimeException.IllegalFunctionCall(Source);

		const int TickConversionFactor = (int)(0.5 + // QuickBASIC uses an integer conversion
			(65536.0 / 1193181.0)                           // seconds per tick (system)
			/ (PlayProcessor.TickMicroseconds * 0.000001)); // seconds per tick (PLAY processor)

		int ticks = (int)Math.Round(duration * TickConversionFactor, MidpointRounding.ToEven);

		if (ticks > 65535)
			ticks = 65535;

		if (ticks > 0)
			context.PlayProcessor.PlaySound(frequency, ticks);
		else
			context.PlayProcessor.StopSound();
	}
}
