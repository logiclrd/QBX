using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;

namespace QBX.ExecutionEngine.Compiled.Expressions;

public class IdentifierExpression(int variableIndex, DataType type) : Expression
{
	public override DataType Type => type;

	public override Variable Evaluate(ExecutionContext context, StackFrame stackFrame)
	{
		return stackFrame.Variables[variableIndex];
	}

	public override LiteralValue EvaluateConstant()
	{
		throw CompilerException.ValueIsNotConstant(SourceExpression?.Token);
	}
}
