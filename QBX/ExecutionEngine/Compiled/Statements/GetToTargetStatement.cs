using System;
using System.Buffers.Binary;

using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;
using QBX.OperatingSystem.FileStructures;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class GetToTargetStatement(CodeModel.Statements.GetStatement source) : GetStatement(source)
{
	public Evaluable? TargetExpression;

	[ThreadStatic]
	static byte[]? s_buffer;

	static Span<byte> EnsureBuffer(int size)
	{
		if ((s_buffer == null) || (s_buffer.Length < size))
			s_buffer = new byte[size * 2];

		return s_buffer.AsSpan().Slice(0, size);
	}

	public override void Execute(ExecutionContext context, StackFrame stackFrame)
	{
		if (FileNumberExpression is null)
			throw new Exception("GetToTargetStatement with no FileNumberExpression");
		if (TargetExpression is null)
			throw new Exception("GetToTargetStatement with no TargetExpression");

		int fileNumber = FileNumberExpression.EvaluateAndCoerceToInt(context, stackFrame);

		if (!context.Files.TryGetValue(fileNumber, out var openFile))
			throw RuntimeException.BadFileNameOrNumber(Source);

		if ((openFile.IOMode != OpenFileIOMode.Random)
		 && (openFile.IOMode != OpenFileIOMode.Binary))
			throw RuntimeException.BadFileMode(Source);

		var target = TargetExpression.Evaluate(context, stackFrame);

		if (RecordNumberExpression != null)
		{
			int recordNumber = RecordNumberExpression.EvaluateAndCoerceToInt(context, stackFrame) - 1;

			if (recordNumber < 1)
				throw RuntimeException.BadRecordNumber(Source);

			if (openFile.IOMode == OpenFileIOMode.Random)
				openFile.CurrentRecordNumber = recordNumber;
			else
				context.Machine.DOS.SeekFile(openFile.FileHandle, recordNumber, MoveMethod.FromBeginning);
		}

		if (openFile.IOMode == OpenFileIOMode.Random)
		{
			context.Machine.DOS.SeekFile(
				openFile.FileHandle,
				(uint)openFile.CurrentRecordNumber * (uint)openFile.RecordLength,
				MoveMethod.FromBeginning);
		}

		var stringTarget = target as StringVariable;

		bool useStringLengthPrefix =
			(openFile.IOMode == OpenFileIOMode.Random) &&
			(stringTarget != null) &&
			!stringTarget.Value.IsFixedLength;

		int alreadyRead = 0;
		int readSize;
		int numRead;

		if (!useStringLengthPrefix)
			readSize = stringTarget?.Value.Length ?? target.DataType.ByteSize;
		else
		{
			Span<byte> lengthPrefixBytes = stackalloc byte[2];

			try
			{
				numRead = context.Machine.DOS.Read(
					openFile.FileHandle,
					lengthPrefixBytes);

				if (numRead < lengthPrefixBytes.Length)
					throw RuntimeException.InputPastEndOfFile(Source);
			}
			catch (Exception e) when (e is not RuntimeException)
			{
				throw RuntimeException.DeviceIOError(Source);
			}

			alreadyRead = 2;
			readSize = BinaryPrimitives.ReadInt16LittleEndian(lengthPrefixBytes);
		}

		if ((openFile.IOMode == OpenFileIOMode.Random)
		 && (alreadyRead + readSize > openFile.RecordLength))
			throw RuntimeException.BadRecordLength(Source);

		if (stringTarget != null)
			stringTarget.Value.Length = readSize;

		var bufferSpan =
			(stringTarget != null)
			? stringTarget.ValueSpan
			: EnsureBuffer(readSize);

		numRead = context.Machine.DOS.Read(
			openFile.FileHandle,
			bufferSpan);

		if (numRead < readSize)
			throw RuntimeException.InputPastEndOfFile(Source);

		if (stringTarget == null)
			target.Deserialize(bufferSpan);

		if (openFile.IOMode == OpenFileIOMode.Random)
			openFile.CurrentRecordNumber++;
	}
}
