using QBX.OperatingSystem;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class LockStatement(CodeModel.Statements.LockStatement source) : LockUnlockStatement(source)
{
	protected override void LockUnlock(DOS dos, int fileHandle, uint start, uint length)
	{
		dos.LockFile(fileHandle, start, length);
	}
}
