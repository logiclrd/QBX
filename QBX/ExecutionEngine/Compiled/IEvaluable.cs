using QBX.ExecutionEngine.Execution.Variables;

namespace QBX.ExecutionEngine.Compiled;

public interface IEvaluable
{
	CodeModel.Statements.Statement? SourceStatement { get; }
	CodeModel.Expressions.Expression? SourceExpression { get; }

	DataType Type { get; }

	Variable Evaluate();
}
