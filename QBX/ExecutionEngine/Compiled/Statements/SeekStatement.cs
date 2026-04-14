using System;

using QBX.ExecutionEngine.Execution;
using QBX.OperatingSystem;
using QBX.OperatingSystem.FileStructures;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class SeekStatement(CodeModel.Statements.SeekStatement source) : Executable(source)
{
	public Evaluable? FileNumberExpression;
	public Evaluable? PositionExpression;

	public override void Execute(ExecutionContext context, StackFrame stackFrame)
	{
		if (FileNumberExpression == null)
			throw new Exception($"SeekStatement with no FileNumberExpression");
		if (PositionExpression == null)
			throw new Exception($"SeekStatement with no PositionExpression");

		int fileNumber = FileNumberExpression.EvaluateAndCoerceToInt(context, stackFrame);

		if (!context.Files.TryGetValue(fileNumber, out var openFile))
			throw RuntimeException.BadFileNameOrNumber(Source);

		int newRecordNumber = PositionExpression.EvaluateAndCoerceToInt(context, stackFrame);

		// BASIC is 1-based
		newRecordNumber--;

		if (newRecordNumber < 0)
			throw RuntimeException.BadRecordNumber(PositionExpression.Source?.Token);

		openFile.CurrentRecordNumber = newRecordNumber;
		openFile.DataParser = null;

		int byteOffset = newRecordNumber;

		if (openFile.IOMode == OpenFileIOMode.Random)
			byteOffset += openFile.RecordLength;

		try
		{
			context.Machine.DOS.SeekFile(openFile.FileHandle, byteOffset, MoveMethod.FromBeginning);
		}
		catch (DOSException ex)
		{
			throw RuntimeException.ForDOSError(ex.ToDOSError(), Source);
		}
	}
}
