using System;
using System.Runtime.InteropServices;

using QBX.ExecutionEngine.Execution;
using QBX.OperatingSystem.FileStructures;
using QBX.OperatingSystem.Memory;

using OSOpenMode = QBX.OperatingSystem.FileStructures.OpenMode;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class BSaveStatement(CodeModel.Statements.Statement source) : BlockIOStatement(source)
{
	public const byte Signature = 0xFD;

	public Evaluable? LengthExpression;

	public override void Execute(ExecutionContext context, StackFrame stackFrame)
	{
		if (OffsetExpression == null)
			throw new Exception(GetType().Name + " with no OffsetExpression");
		if (LengthExpression == null)
			throw new Exception("BSaveStatement with no LengthExpression");

		int length = -1;

		ExecuteCommon(
			context,
			stackFrame,
			prepare:
				() =>
				{
					length = LengthExpression.EvaluateAndCoerceToInt(context, stackFrame);

					if ((length < short.MinValue) || (length > ushort.MaxValue)) // NB: top end is unsigned
						throw RuntimeException.Overflow(LengthExpression.Source);

					length &= 0xFFFF;
				},
			FileMode.Create,
			OSOpenMode.Access_WriteOnly | OSOpenMode.Share_DenyRead,
			(fileHandle, offset) =>
			{
				int segment = context.RuntimeState.SegmentBase >> 4;

				var headerBytes = new byte[7];

				headerBytes[0] = BSaveStatement.Signature;

				var headerFields = MemoryMarshal.Cast<byte, ushort>(headerBytes.AsSpan().Slice(1));

				headerFields[0] = unchecked((ushort)segment);
				headerFields[1] = unchecked((ushort)offset);
				headerFields[2] = unchecked((ushort)length);

				int remaining = headerBytes.Length;

				while (remaining > 0)
				{
					int numWritten = context.Machine.DOS.Write(fileHandle, headerBytes.AsSpan().Slice(headerBytes.Length - remaining), out _);

					if (numWritten <= 0)
						throw RuntimeException.DeviceIOError(Source);

					remaining -= numWritten;
				}

				var address = new SegmentedAddress(headerFields[0], headerFields[1]);

				int linearAddress = address.ToLinearAddress();

				int blockSize = headerFields[2];

				context.Machine.SystemMemory.UpdateDynamicData(
					linearAddress,
					linearAddress + blockSize);

				remaining = blockSize;

				while (remaining > 0)
				{
					int numWritten = context.Machine.DOS.Write(
						fileHandle,
						context.Machine.MemoryBus,
						linearAddress,
						remaining);

					if (numWritten <= 0)
						throw RuntimeException.DeviceIOError(Source);

					linearAddress += numWritten;
					remaining -= numWritten;
				}
			});
	}
}
