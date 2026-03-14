using QBX.ExecutionEngine.Execution.Events;

namespace QBX.ExecutionEngine.Compiled.Statements;

public interface IOnEventStatementConfigurator
{
	void SetEventType(EventType eventType);
	Evaluable? SetSourceExpression(Evaluable? sourceExpression);

	Executable AsExecutable();
}
