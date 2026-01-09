using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;

namespace QBX.ExecutionEngine.Compiled.Expressions;

public abstract class Expression : IEvaluable
{
	public CodeModel.Statements.Statement? SourceStatement { get; set; }
	public CodeModel.Expressions.Expression? SourceExpression { get; set; }

	public abstract DataType Type { get; }

	public abstract Variable Evaluate(ExecutionContext context, StackFrame stackFrame);

	public virtual LiteralValue EvaluateConstant()
		=> throw CompilerException.ValueIsNotConstant(SourceExpression?.Token);
}
