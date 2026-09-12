using QBX.ExecutionEngine.Execution;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class ChainStatement(CodeModel.Statements.ChainStatement source) : ReplaceRunningProgramStatement(source)
{
	public override bool CanExecuteDirectWithoutEmbedding => true;

	protected override void ConfigureContext(ExecutionContext context)
	{
		// Preserve common data blocks
		context.SetChainExecution();
	}
}
