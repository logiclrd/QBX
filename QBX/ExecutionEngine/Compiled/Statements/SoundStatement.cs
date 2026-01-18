using System;

using QBX.ExecutionEngine.Execution;

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
			throw new Exception("SoundStatement with no Durationxpression");

		var frequencyValue = FrequencyExpression.Evaluate(context, stackFrame);
		var durationValue = DurationExpression.Evaluate(context, stackFrame);

		int frequency = frequencyValue.CoerceToInt();
		int duration = durationValue.CoerceToInt();

		context.PlayProcessor.PlaySound(frequency, duration);
	}
}
