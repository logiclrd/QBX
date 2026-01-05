namespace QBX.ExecutionEngine.Compiled.Statements;

public class ScreenStatement : IExecutable
{
	public IEvaluable? ModeExpression;
	public IEvaluable? ColourSwitchExpression;
	public IEvaluable? ActivePageExpression;
	public IEvaluable? VisiblePageExpression;

	public void Execute(Execution.ExecutionContext context, bool stepInto)
	{
		throw new NotImplementedException();
	}
}
