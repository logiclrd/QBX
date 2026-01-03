namespace QBX.ExecutionEngine.Compiled;

public interface IEvaluable<T>
{
	CodeModel.Statements.Statement? SourceStatement { get; }
	CodeModel.Expressions.Expression? SourceExpression { get; }

	T Evaluate();
}
