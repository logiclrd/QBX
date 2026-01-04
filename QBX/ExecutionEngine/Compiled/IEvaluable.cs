using QBX.ExecutionEngine.Execution;

namespace QBX.ExecutionEngine.Compiled;

public interface IEvaluable
{
	CodeModel.Statements.Statement? SourceStatement { get; }
	CodeModel.Expressions.Expression? SourceExpression { get; }

	Variable Evaluate();
}
