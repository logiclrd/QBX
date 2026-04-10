using System;
using System.Reflection.PortableExecutable;
using System.Runtime.InteropServices;

using QBX.ExecutionEngine.Execution;
using QBX.OperatingSystem.FileStructures;
using QBX.OperatingSystem.Memory;
using QBX.Parser;

using OSOpenMode = QBX.OperatingSystem.FileStructures.OpenMode;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class BLoadStatement(CodeModel.Statements.Statement source) : BlockIOStatement(source)
{
	public override void Execute(ExecutionContext context, StackFrame stackFrame)
	{
		ExecuteCommon(
			context,
			stackFrame,
			prepare: null,
			FileMode.Open,
			OSOpenMode.Access_ReadOnly | OSOpenMode.Share_DenyWrite,
			(fileHandle, offset) =>
			{
				var headerBytes = new byte[7];

				int remaining = headerBytes.Length;

				while (remaining > 0)
				{
					int numRead = context.Machine.DOS.Read(fileHandle, headerBytes.AsSpan().Slice(headerBytes.Length - remaining));

					if (numRead <= 0)
						throw RuntimeException.DeviceIOError(Source);

					remaining -= numRead;
				}

				if (headerBytes[0] != BSaveStatement.Signature)
					throw RuntimeException.BadFileMode(Source);

				var headerFields = MemoryMarshal.Cast<byte, ushort>(headerBytes.AsSpan().Slice(1));

				ushort segment =
					(OffsetExpression != null)
					? unchecked((ushort)(context.RuntimeState.SegmentBase >> 4))
					: headerFields[0];

				if (OffsetExpression == null)
					offset = headerFields[1];

				var address = new SegmentedAddress(segment, (ushort)offset);

				int linearAddress = address.ToLinearAddress();

				remaining = headerFields[2];

				while (remaining > 0)
				{
					int numRead = context.Machine.DOS.Read(
						fileHandle,
						context.Machine.MemoryBus,
						linearAddress,
						remaining);

					if (numRead <= 0)
						throw RuntimeException.DeviceIOError(Source);

					linearAddress += numRead;
					remaining -= numRead;
				}
			});
	}
}
