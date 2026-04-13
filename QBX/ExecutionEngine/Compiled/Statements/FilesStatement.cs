using System;

using QBX.ExecutionEngine.Execution.Variables;
using QBX.OperatingSystem;
using QBX.OperatingSystem.FileStructures;
using QBX.OperatingSystem.Memory;

using ExecutionContext = QBX.ExecutionEngine.Execution.ExecutionContext;
using StackFrame = QBX.ExecutionEngine.Execution.StackFrame;

namespace QBX.ExecutionEngine.Compiled.Statements;

public partial class FilesStatement(CodeModel.Statements.Statement source) : Executable(source)
{
	public Evaluable? PatternExpression;

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

	public override void Execute(ExecutionContext context, StackFrame stackFrame)
	{
		string pattern = "*.*";

		if (PatternExpression != null)
		{
			var patternResult = (StringVariable)PatternExpression.Evaluate(context, stackFrame);

			pattern = patternResult.ValueString;
		}

		using (context.Machine.DOS.SuppressExceptionsInScope())
		{
			int driveIdentifier = context.Machine.DOS.GetDefaultDrive();

			if ((driveIdentifier < 1) || (driveIdentifier > 26))
				throw RuntimeException.PathNotFound(Source);

			string driveLetter = ((char)(driveIdentifier + 'A')).ToString();
			string currentDirectoryUnrooted = context.Machine.DOS.GetCurrentDirectoryUnrooted(driveIdentifier + 1);

			string currentDirectory = $"{driveLetter}:\\{currentDirectoryUnrooted}";

			context.VisualLibrary.WriteText(currentDirectory);
			context.VisualLibrary.NewLine();

			EnsureFileInfoBufferConfigured(context.Machine.DOS);

			Span<byte> entry = stackalloc byte[18];

			if (context.Machine.DOS.FindFirst(pattern, FileAttributes.Directory))
			{
				do
				{
					s_fileInfo.Deserialize(context.Machine.MemoryBus, s_fileInfoBufferAddress);

					entry.Fill((byte)' ');

					var matchedFileNameSpan = s_fileInfo.FileName.AsSpan();

					Span<byte> fileName, extension;

					int dot = matchedFileNameSpan.IndexOf((byte)'.');

					if (dot < 0)
					{
						fileName = matchedFileNameSpan;
						extension = Span<byte>.Empty;
					}
					else
					{
						fileName = matchedFileNameSpan.Slice(0, dot);
						extension = matchedFileNameSpan.Slice(dot + 1);
					}

					if (fileName.Length > 8)
						fileName = fileName.Slice(0, 8);
					if (extension.Length > 3)
						extension = extension.Slice(0, 3);

					fileName.CopyTo(entry);
					entry[8] = (byte)'.';
					extension.CopyTo(entry.Slice(9));

					if ((s_fileInfo.Attributes & FileAttributes.Directory) != 0)
					{
						entry[12] = (byte)'<';
						entry[13] = (byte)'D';
						entry[14] = (byte)'I';
						entry[15] = (byte)'R';
						entry[16] = (byte)'>';
					}

					if (context.VisualLibrary.CursorX + entry.Length >= context.VisualLibrary.CharacterWidth)
						context.VisualLibrary.NewLine();

					context.VisualLibrary.WriteText(entry);

					if (context.Machine.DOS.LastError != DOSError.None)
						throw RuntimeException.ForDOSError(context.Machine.DOS.LastError, Source);
				}
				while (context.Machine.DOS.FindNext());

				context.VisualLibrary.NewLine();

				if (!context.Machine.DOS.TryGetDriveParameterBlock(driveIdentifier, out var dpbAddress))
					throw RuntimeException.DeviceUnavailable(Source);

				ref DriveParameterBlock dpb = ref DriveParameterBlock.CreateReference(context.Machine.SystemMemory, dpbAddress.ToLinearAddress());

				long freeSpace = dpb.FreeClusterCount * 65536L;

				string summaryLine = freeSpace.ToString().PadLeft(10) + " Bytes free";

				context.VisualLibrary.WriteText(summaryLine);
				context.VisualLibrary.NewLine();
			}
		}
	}
}

