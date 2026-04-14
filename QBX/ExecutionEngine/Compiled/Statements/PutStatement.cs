namespace QBX.ExecutionEngine.Compiled.Statements;

public abstract class PutStatement(CodeModel.Statements.PutStatement source) : Executable(source)
{
	public Evaluable? FileNumberExpression;
	public Evaluable? RecordNumberExpression;
}
