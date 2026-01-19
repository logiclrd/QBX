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

		int frequency = FrequencyExpression.EvaluateAndCoerceToInt(context, stackFrame);
		int duration = DurationExpression.EvaluateAndCoerceToInt(context, stackFrame);

		context.PlayProcessor.PlaySound(frequency, duration);
	}
}
