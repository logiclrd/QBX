using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class DimensionArrayStatement(CodeModel.Statements.Statement? source) : Executable(source)
{
	public int VariableIndex;
	public ArraySubscriptsExpressions Subscripts = new ArraySubscriptsExpressions();
	public bool IsRedimension;
	public bool PreserveData;

	public override void Execute(ExecutionContext context, StackFrame stackFrame)
	{
		var subscripts = Subscripts.Evaluate(context, stackFrame);

		var variable = (ArrayVariable)stackFrame.Variables[VariableIndex];

		if (!IsRedimension && !variable.Array.IsUninitialized)
			throw RuntimeException.DuplicateDefinition(Source);

		try
		{
			if (PreserveData && !variable.Array.IsUninitialized)
				variable.Array.RedimensionPreservingData(subscripts);
			else
				variable.InitializeArray(subscripts);
		}
		catch (RuntimeException ex)
		{
			throw ex.AddContext(Source);
		}
	}
}
