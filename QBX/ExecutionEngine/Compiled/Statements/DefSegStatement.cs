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
			var segmentValue = SegmentExpression.Evaluate(context, stackFrame);

			int segment = segmentValue.CoerceToInt();

			if (segment < 0)
				segment += 0x10000;

			if ((segment < 0) || (segment >= 0x10000))
				throw RuntimeException.Overflow(SegmentExpression.SourceStatement?.FirstToken);

			context.RuntimeState.SegmentBase = segment * 0x10;
		}
	}
}
