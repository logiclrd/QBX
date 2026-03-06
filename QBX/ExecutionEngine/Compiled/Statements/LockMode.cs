namespace QBX.ExecutionEngine.Compiled.Statements;

public enum LockMode
{
	None,

	Shared,
	LockRead,
	LockWrite,
	LockReadWrite,
}
