using QBX.ExecutionEngine.Execution;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class AssignmentStatement(CodeModel.Statements.Statement? source) : Statement(source)
{
	public IEvaluable? TargetExpression;
	public IEvaluable? ValueExpression;

	public override void Execute(ExecutionContext context, StackFrame stackFrame)
	{
		var targetVariable = TargetExpression!.Evaluate(context, stackFrame);
		var valueVariable = ValueExpression!.Evaluate(context, stackFrame);

		targetVariable.SetData(valueVariable.GetData());
	}
}
