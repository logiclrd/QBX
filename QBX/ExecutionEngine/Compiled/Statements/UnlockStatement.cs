using QBX.OperatingSystem;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class UnlockStatement(CodeModel.Statements.UnlockStatement source) : LockUnlockStatement(source)
{
	protected override void LockUnlock(DOS dos, int fileHandle, uint start, uint length)
	{
		dos.UnlockFile(fileHandle, start, length);
	}
}
