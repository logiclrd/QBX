using System;

using QBX.ExecutionEngine.Execution;
using QBX.OperatingSystem.FileStructures;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class GetToFieldsStatement(CodeModel.Statements.GetStatement source) : GetStatement(source)
{
	[ThreadStatic]
	static byte[]? s_buffer;

	public override void Execute(ExecutionContext context, StackFrame stackFrame)
	{
		if (FileNumberExpression is null)
			throw new Exception("GetToFieldsStatement with no FileNumberExpression");

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

		int numRead = context.Machine.DOS.Read(
			openFile.FileHandle,
			s_buffer.AsSpan().Slice(0, openFile.RecordLength));

		openFile.RecordOffset = 0;

		if (numRead == 0)
			throw RuntimeException.InputPastEndOfFile(Source);

		openFile.WriteToFields(s_buffer.AsSpan().Slice(0, numRead));
		openFile.RecordOffset = 0;

		openFile.CurrentRecordNumber++;
	}
}
