using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class DimensionArrayStatement(CodeModel.Statements.Statement? source) : Statement(source)
{
	public int VariableIndex;
	public ArraySubscriptsExpressions Subscripts = new ArraySubscriptsExpressions();
	public bool IsRedimension;
	public bool PreserveData;

	public override void Execute(ExecutionContext context, StackFrame stackFrame)
	{
		var subscripts = Subscripts.Evaluate(context, stackFrame);

		var variable = (ArrayVariable)stackFrame.Variables[VariableIndex];

		variable.InitializeArray(subscripts);
	}
}
