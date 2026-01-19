using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;
using QBX.Numbers;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class AssignmentStatement(CodeModel.Statements.Statement? source) : Executable(source)
{
	public Evaluable? TargetExpression;
	public Evaluable? ValueExpression;

	public override void Execute(ExecutionContext context, StackFrame stackFrame)
	{
		var targetVariable = TargetExpression!.Evaluate(context, stackFrame);
		var valueVariable = ValueExpression!.Evaluate(context, stackFrame);

		try
		{
			targetVariable.SetData(valueVariable.GetData());
		}
		catch (RuntimeException error)
		{
			throw error.AddContext(TargetExpression.SourceExpression?.Token);
		}
	}
}
