using QBX.ExecutionEngine.Compiled.Expressions;
using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;

namespace QBX.ExecutionEngine.Compiled;

public abstract class Evaluable
{
	public CodeModel.Statements.Statement? SourceStatement { get; set; }
	public CodeModel.Expressions.Expression? SourceExpression { get; set; }

	public abstract DataType Type { get; }

	public virtual void CollapseConstantSubexpressions()
	{
	}

	public abstract Variable Evaluate(ExecutionContext context, StackFrame stackFrame);

	public virtual bool IsConstant
		=> false;
	public virtual LiteralValue EvaluateConstant()
		=> throw CompilerException.ValueIsNotConstant(SourceExpression?.Token);
}
