using System;

using QBX.ExecutionEngine.Execution;
using QBX.OperatingSystem.FileStructures;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class PutFromFieldsStatement(CodeModel.Statements.PutStatement source) : PutStatement(source)
{
	[ThreadStatic]
	static byte[]? s_buffer;

	public override void Execute(ExecutionContext context, StackFrame stackFrame)
	{
		if (FileNumberExpression is null)
			throw new Exception("PutFromFieldsStatement with no FileNumberExpression");

		int fileNumber = FileNumberExpression.EvaluateAndCoerceToInt(context, stackFrame);

		if (!context.Files.TryGetValue(fileNumber, out var openFile))
			throw RuntimeException.BadFileNameOrNumber(Source);

		if (openFile.IOMode == OpenFileIOMode.Binary)
			throw RuntimeException.VariableRequired(Source);
		else if (openFile.IOMode != OpenFileIOMode.Random)
			throw RuntimeException.BadFileMode(Source);

		if (RecordNumberExpression != null)
		{
			int recordNumber = RecordNumberExpression.EvaluateAndCoerceToInt(context, stackFrame);

			if (recordNumber < 1)
				throw RuntimeException.BadRecordNumber(Source);

			openFile.CurrentRecordNumber = recordNumber;
		}

		context.Machine.DOS.SeekFile(
			openFile.FileHandle,
			(uint)openFile.CurrentRecordNumber * (uint)openFile.RecordLength,
			MoveMethod.FromBeginning);

		if ((s_buffer == null) || (s_buffer.Length < openFile.RecordLength))
			s_buffer = new byte[openFile.RecordLength];

		var bufferSpan = s_buffer.AsSpan().Slice(0, openFile.RecordLength);

		openFile.RecordOffset = 0;
		openFile.ReadFromFields(bufferSpan);
		openFile.RecordOffset = 0;

		int numWritten = context.Machine.DOS.Write(
			openFile.FileHandle,
			s_buffer.AsSpan().Slice(0, openFile.RecordLength),
			out _);

		if (numWritten < bufferSpan.Length)
			throw RuntimeException.DeviceIOError(Source);

		openFile.CurrentRecordNumber++;
	}
}
