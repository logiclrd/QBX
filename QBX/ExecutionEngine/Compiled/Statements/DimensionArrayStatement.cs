using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class DimensionArrayStatement : IExecutable
{
	public int VariableIndex;
	public ArraySubscriptsExpressions Subscripts = new ArraySubscriptsExpressions();
	public bool IsRedimension;
	public bool PreserveData;

	public void Execute(ExecutionContext context, bool stepInto)
	{
		var subscripts = Subscripts.Evaluate(context);

		var variable = (ArrayVariable)context.CurrentFrame.Variables[VariableIndex];

		variable.InitializeArray(subscripts);
	}
}
