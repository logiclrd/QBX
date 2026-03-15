using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;
using QBX.OperatingSystem;
using QBX.OperatingSystem.FileStructures;
using QBX.OperatingSystem.Memory;

namespace QBX.ExecutionEngine.Compiled.Functions;

public class DirFunction : Function
{
	public Evaluable? FileSpec;

	protected override int MinArgumentCount => 0;

	protected override void SetArgument(int index, Evaluable value)
	{
		if (!value.Type.IsString)
			throw CompilerException.TypeMismatch(value.Source);

		FileSpec = value;
	}

	public override void CollapseConstantSubexpressions()
	{
		CollapseConstantExpression(ref FileSpec);
	}

	public override DataType Type => DataType.String;

	static DOSFileInfo s_fileInfo = new DOSFileInfo();

	static int s_fileInfoBufferAddress;

	static void EnsureFileInfoBufferConfigured(DOS dos)
	{
		if (s_fileInfoBufferAddress == 0)
		{
			s_fileInfoBufferAddress = dos.MemoryManager.AllocateMemory(
				DOSFileInfo.Size,
				dos.CurrentPSPSegment);
		}

		var segmentedAddress = new SegmentedAddress(s_fileInfoBufferAddress);

		dos.DiskTransferAddressSegment = segmentedAddress.Segment;
		dos.DiskTransferAddressOffset = segmentedAddress.Offset;
	}

	public override Variable Evaluate(ExecutionContext context, StackFrame stackFrame)
	{
		using (context.Machine.DOS.SuppressExceptionsInScope())
		{
			EnsureFileInfoBufferConfigured(context.Machine.DOS);

			if (FileSpec != null)
			{
				var argumentValue = (StringVariable)FileSpec.Evaluate(context, stackFrame);

				context.Machine.DOS.FindFirst(
					argumentValue.ValueString,
					FileAttributes.Normal);
			}
			else
				context.Machine.DOS.FindNext();
		}

		if (context.Machine.DOS.LastError != DOSError.None)
			return new StringVariable();
		else
		{
			s_fileInfo.Deserialize(context.Machine.MemoryBus, s_fileInfoBufferAddress);

			return new StringVariable(s_fileInfo.FileName.TrimZ());
		}
	}
}
