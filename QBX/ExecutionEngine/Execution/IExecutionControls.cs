namespace QBX.ExecutionEngine.Execution;

public interface IExecutionControls
{
	void ContinueExecution();
	void StepOverNextRoutine();
	void ExecuteOneStatement();
	void Break();
	void Terminate();

	void WaitForInterruption();
}
