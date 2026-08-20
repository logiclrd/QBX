using QBX.ExecutionEngine.Execution;

namespace QBX.ExecutionEngine.Compiled.Statements;

// A no-op that makes it possible to pause execution/have a
// breakpoint on the END SUB/FUNCTION line at the end of a
// code element.
public class EndOfRoutineStatement(CodeModel.Statements.EndScopeStatement source) : Executable(source)
{
	protected override void ExecuteImplementation(ExecutionContext context, StackFrame stackFrame)
	{
	}
}
