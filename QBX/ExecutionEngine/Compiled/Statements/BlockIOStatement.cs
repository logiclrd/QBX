using System;

using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;
using QBX.OperatingSystem;
using QBX.OperatingSystem.FileStructures;

using OSOpenMode = QBX.OperatingSystem.FileStructures.OpenMode;

namespace QBX.ExecutionEngine.Compiled.Statements;

public abstract class BlockIOStatement(CodeModel.Statements.BlockIOStatement source) : Executable(source)
{
	public Evaluable? FileNameExpression;
	public Evaluable? OffsetExpression;

	protected delegate void TransferDataFunctor(int fileHandle, int offset);

	protected void ExecuteCommon(
		ExecutionContext context,
		StackFrame stackFrame,
		Action? prepare,
		FileMode fileFileMode,
		OSOpenMode fileOpenMode,
		TransferDataFunctor transferData)
	{
		if (FileNameExpression == null)
			throw new Exception(GetType().Name + " with no FileNameExpression");

		var fileName = (StringVariable)FileNameExpression.Evaluate(context, stackFrame);

		int offset = -1;

		if (OffsetExpression != null)
		{
			offset = OffsetExpression.EvaluateAndCoerceToInt(context, stackFrame);

			if ((offset < short.MinValue) || (offset > ushort.MaxValue)) // NB: top end is unsigned
				throw RuntimeException.Overflow(OffsetExpression.Source);

			offset &= 0xFFFF;
		}

		prepare?.Invoke();

		int fileHandle = -1;

		try
		{
			DOSError lastError = DOSError.None;

			fileHandle = context.Machine.DOS.OpenFile(
				fileName.Value.ToString(),
				fileFileMode,
				fileOpenMode | OSOpenMode.Share_Compatibility);

			lastError = context.Machine.DOS.LastError;

			if (lastError != DOSError.None)
				throw RuntimeException.ForDOSError(lastError, Source);

			transferData(fileHandle, offset);
		}
		catch (DOSException ex)
		{
			throw RuntimeException.ForDOSError(ex.ToDOSError(), Source);
		}
		finally
		{
			try
			{
				context.Machine.DOS.CloseFile(fileHandle);
			}
			catch { }
		}
	}
}
