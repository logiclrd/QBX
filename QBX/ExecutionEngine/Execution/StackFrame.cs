using QBX.CodeModel.Statements;
using QBX.ExecutionEngine.Execution.Variables;

namespace QBX.ExecutionEngine.Execution;

public class StackFrame(Variable[] variables)
{
	public Variable[] Variables = variables;
	public Statement? CurrentStatement;
}
