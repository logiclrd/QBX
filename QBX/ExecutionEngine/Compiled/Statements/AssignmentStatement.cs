using QBX.ExecutionEngine.Execution;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class AssignmentStatement : IExecutable
{
	public IEvaluable? TargetExpression;
	public IEvaluable? ValueExpression;

	public void Execute(ExecutionContext context, bool stepInto)
	{
		var targetVariable = TargetExpression!.Evaluate(context);
		var valueVariable = ValueExpression!.Evaluate(context);

		targetVariable.SetData(valueVariable.GetData());
	}
}
