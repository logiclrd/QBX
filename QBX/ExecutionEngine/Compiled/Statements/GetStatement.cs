namespace QBX.ExecutionEngine.Compiled.Statements;

public abstract class GetStatement(CodeModel.Statements.Statement? source) : Executable(source)
{
	public Evaluable? FileNumberExpression;
	public Evaluable? RecordNumberExpression;
}
