namespace QBX.ExecutionEngine.Compiled.Statements;

public class FieldMapping(Evaluable widthExpression, Evaluable stringVariableExpression)
{
	public Evaluable WidthExpression = widthExpression;
	public Evaluable StringVariableExpression = stringVariableExpression;
}
