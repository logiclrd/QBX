using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;

namespace QBX.ExecutionEngine.Compiled.Expressions;

public class IdentifierExpression(int variableIndex, DataType type) : IEvaluable
{
	public CodeModel.Statements.Statement? SourceStatement { get; set; }
	public CodeModel.Expressions.Expression? SourceExpression { get; set; }

	public DataType Type => type;

	public Variable Evaluate(ExecutionContext context)
	{
		return context.CurrentFrame.Variables[variableIndex];
	}

	public LiteralValue EvaluateConstant()
	{
		throw CompilerException.ValueIsNotConstant(SourceExpression?.Token);
	}
}
