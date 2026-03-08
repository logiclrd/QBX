using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;

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
			if (targetVariable is StringVariable stringVariable)
				context.UnlinkFieldVariable(stringVariable);

			targetVariable.SetData(valueVariable.GetData());
		}
		catch (RuntimeException error)
		{
			throw error.AddContext(TargetExpression.Source);
		}
	}
}
