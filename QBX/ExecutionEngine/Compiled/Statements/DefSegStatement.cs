using QBX.ExecutionEngine.Execution;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class DefSegStatement(CodeModel.Statements.Statement source) : Executable(source)
{
	public Evaluable? SegmentExpression;

	public override void Execute(ExecutionContext context, StackFrame stackFrame)
	{
		if (SegmentExpression == null)
			context.RuntimeState.SegmentBase = context.RuntimeState.GetDataSegmentBase();
		else
		{
			int segment = SegmentExpression.EvaluateAndCoerceToInt(context, stackFrame);

			if (segment < 0)
				segment += 0x10000;

			if ((segment < 0) || (segment >= 0x10000))
				throw RuntimeException.Overflow(Source);

			context.RuntimeState.SegmentBase = segment * 0x10;
		}
	}
}
