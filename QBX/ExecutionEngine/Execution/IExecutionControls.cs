using QBX.ExecutionEngine.Compiled;

namespace QBX.ExecutionEngine.Execution;

public interface IExecutionControls
{
	object Sync { get; }

	void ContinueExecution();
	void StepOverNextRoutine();
	void ExecuteOneStatement();
	void Break();
	void Terminate();

	void ExecuteDirect(Sequence sequence);

	void IgnoreBreakFromNextStatement();

	void WaitForInterruption();
	void WaitForStartUp();
}
