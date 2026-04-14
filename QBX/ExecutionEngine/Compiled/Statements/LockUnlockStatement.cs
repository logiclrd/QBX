using System;

using QBX.ExecutionEngine.Execution;
using QBX.OperatingSystem;

namespace QBX.ExecutionEngine.Compiled.Statements;

public abstract class LockUnlockStatement(CodeModel.Statements.FileByteRangeStatement source) : Executable(source)
{
	public Evaluable? FileNumberExpression;
	public Evaluable? StartExpression;
	public Evaluable? EndExpression;

	protected abstract void LockUnlock(DOS dos, int fileHandle, uint start, uint length);

	public override void Execute(ExecutionContext context, StackFrame stackFrame)
	{
		if (FileNumberExpression == null)
			throw new Exception($"{GetType().Name} with no FileNumberExpression");

		int fileNumber = FileNumberExpression.EvaluateAndCoerceToInt(context, stackFrame);

		if (!context.Files.TryGetValue(fileNumber, out var openFile))
			throw RuntimeException.BadFileNameOrNumber(Source);

		bool lockEntireFile = false;

		if ((StartExpression == null) && (EndExpression == null))
			lockEntireFile = true;
		else if ((openFile.IOMode != OpenFileIOMode.Random) && (openFile.IOMode != OpenFileIOMode.Binary))
			lockEntireFile = true;

		uint start, length;

		if (lockEntireFile)
		{
			start = uint.MinValue;
			length = uint.MaxValue;
		}
		else
		{
			checked
			{
				if (StartExpression == null)
					start = 0;
				else
					start = (uint)StartExpression.EvaluateAndCoerceToInt(context, stackFrame);

				if (EndExpression == null)
					length = 1;
				else
					length = (uint)EndExpression.EvaluateAndCoerceToInt(context, stackFrame) - start + 1;

				if (openFile.IOMode == OpenFileIOMode.Random)
				{
					start *= (uint)openFile.RecordLength;
					length *= (uint)openFile.RecordLength;
				}
			}
		}

		LockUnlock(context.Machine.DOS, openFile.FileHandle, start, length);
	}
}
