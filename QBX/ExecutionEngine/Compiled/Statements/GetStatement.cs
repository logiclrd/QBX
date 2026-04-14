namespace QBX.ExecutionEngine.Compiled.Statements;

public abstract class GetStatement(CodeModel.Statements.GetStatement source) : Executable(source)
{
	public Evaluable? FileNumberExpression;
	public Evaluable? RecordNumberExpression;
}
