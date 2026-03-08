using System;

using QBX.ExecutionEngine.Execution;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class AssignmentStatement(CodeModel.Statements.Statement? source) : Executable(source)
{
	public Evaluable? TargetExpression;
	public Evaluable? ValueExpression;

	public override void Execute(ExecutionContext context, StackFrame stackFrame)
	{
		if (TargetExpression == null)
			throw new Exception("AssignmentStatement with no TargetExpression");
		if (ValueExpression == null)
			throw new Exception("AssignmentStatement with no ValueExpression");

		var valueVariable = ValueExpression.Evaluate(context, stackFrame);

		try
		{
			TargetExpression.EvaluateAndAssignTo(context, stackFrame, valueVariable);
		}
		catch (RuntimeException error)
		{
			throw error.AddContext(TargetExpression.Source);
		}
	}
}
