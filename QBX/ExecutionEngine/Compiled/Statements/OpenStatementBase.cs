using System;
using System.Collections.Generic;

using QBX.ExecutionEngine.Execution;
using QBX.OperatingSystem;
using QBX.OperatingSystem.FileStructures;

using OSOpenMode = QBX.OperatingSystem.FileStructures.OpenMode;

namespace QBX.ExecutionEngine.Compiled.Statements;

public abstract class OpenStatementBase(CodeModel.Statements.OpenStatementBase source) : Executable(source)
{
	protected static readonly IEnumerable<OSOpenMode> Attempt_Read =
		[OSOpenMode.Access_ReadOnly];
	protected static readonly IEnumerable<OSOpenMode> Attempt_Write =
		[OSOpenMode.Access_WriteOnly];
	protected static readonly IEnumerable<OSOpenMode> Attempt_ReadWrite =
		[OSOpenMode.Access_ReadWrite];
	protected static readonly IEnumerable<OSOpenMode> Attempt_ReadWrite_Write =
		[OSOpenMode.Access_ReadWrite, OSOpenMode.Access_WriteOnly];
	protected static readonly IEnumerable<OSOpenMode> Attempt_ReadWrite_Write_Read =
		[OSOpenMode.Access_ReadWrite, OSOpenMode.Access_WriteOnly, OSOpenMode.Access_ReadOnly];

	protected void PerformOpen(string fileName, OpenMode openMode, FileMode fileMode, OSOpenMode shareMode, IEnumerable<OSOpenMode> attemptAccessModes, int? recordLength, int fileNumber, ExecutionContext context)
	{
		try
		{
			DOSError lastError = DOSError.None;

			var openFile = new OpenFile();

			foreach (var accessMode in attemptAccessModes)
			{
				try
				{
					openFile.FileHandle = context.Machine.DOS.OpenFile(
						fileName,
						fileMode,
						accessMode | shareMode);

					lastError = context.Machine.DOS.LastError;

					if (lastError == DOSError.None)
						break;
				}
				catch (DOSException ex)
				{
					lastError = ex.ToDOSError();
				}
			}

			if (lastError != DOSError.None)
				throw RuntimeException.ForDOSError(lastError, Source);

			if (openMode == OpenMode.Append)
				context.Machine.DOS.SeekFile(openFile.FileHandle, 0, MoveMethod.FromEnd);

			openFile.IOMode =
				openMode switch
				{
					OpenMode.Random => OpenFileIOMode.Random,
					OpenMode.Binary => OpenFileIOMode.Binary,
					OpenMode.Input => OpenFileIOMode.Input,
					OpenMode.Output or OpenMode.Append => OpenFileIOMode.Output,

					_ => throw new Exception("Unrecognized OpenMode value " + openMode)
				};

			openFile.OpenedForAppend = (openMode == OpenMode.Append);

			if (recordLength != null)
			{
				if (openFile.IOMode == OpenFileIOMode.Random)
					openFile.RecordLength = recordLength.Value;
				else
				{
					openFile.BufferSize = recordLength.Value;

					context.Machine.DOS.SetFileBufferSize(openFile.FileHandle, openFile.BufferSize);
				}
			}

			if (openMode == OpenMode.Random)
				openFile.ConfigureFields(System.Array.Empty<FileRecordField>(), context);

			context.Files[fileNumber] = openFile;
		}
		catch (DOSException ex)
		{
			throw RuntimeException.ForDOSError(ex.ToDOSError(), Source);
		}
	}
}
