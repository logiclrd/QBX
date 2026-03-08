using System;
using System.Collections.Generic;
using System.Linq;

using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;
using QBX.OperatingSystem;
using QBX.OperatingSystem.FileStructures;

using OSOpenMode = QBX.OperatingSystem.FileStructures.OpenMode;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class OpenStatement(CodeModel.Statements.Statement source) : Executable(source)
{
	public OpenMode OpenMode;
	public AccessMode AccessMode;
	public LockMode LockMode;
	public Evaluable? FileNameExpression;
	public Evaluable? FileNumberExpression;
	public Evaluable? RecordLengthExpression;

	static readonly IEnumerable<OSOpenMode> Attempt_Read =
		[OSOpenMode.Access_ReadOnly];
	static readonly IEnumerable<OSOpenMode> Attempt_Write =
		[OSOpenMode.Access_WriteOnly];
	static readonly IEnumerable<OSOpenMode> Attempt_ReadWrite =
		[OSOpenMode.Access_ReadWrite];
	static readonly IEnumerable<OSOpenMode> Attempt_ReadWrite_Write =
		[OSOpenMode.Access_ReadWrite, OSOpenMode.Access_WriteOnly];
	static readonly IEnumerable<OSOpenMode> Attempt_ReadWrite_Write_Read =
		[OSOpenMode.Access_ReadWrite, OSOpenMode.Access_WriteOnly, OSOpenMode.Access_ReadOnly];

	public override void Execute(ExecutionContext context, StackFrame stackFrame)
	{
		if (FileNameExpression == null)
			throw new Exception("OpenStatement with no FileNameExpression");
		if (FileNumberExpression == null)
			throw new Exception("OpenStatement with no FileNumberExpression");

		int fileNumber = FileNumberExpression.EvaluateAndCoerceToInt(context, stackFrame);

		if (context.Files.ContainsKey(fileNumber))
			throw RuntimeException.FileAlreadyOpen(Source);

		var fileName = (StringVariable)FileNameExpression.Evaluate(context, stackFrame);

		if ((OpenMode == OpenMode.Output)
		 && (OpenMode == OpenMode.Append))
		{
			if (context.Machine.DOS.FileIsOpenAsOneOf(fileName.Value, context.Files.Values.Select(openFile => openFile.FileHandle)))
				throw RuntimeException.FileAlreadyOpen(Source);
		}

		int? recordLength = RecordLengthExpression?.EvaluateAndCoerceToInt(context, stackFrame);

		var openFile = new OpenFile();

		var openMode =
			OpenMode switch
			{
				OpenMode.Random or OpenMode.Binary or OpenMode.Append => FileMode.OpenOrCreate,
				OpenMode.Input => FileMode.Open,
				OpenMode.Output => FileMode.Create,

				_ => throw new Exception("Unrecognized OpenMode value " + OpenMode)
			};

		IEnumerable<OSOpenMode> attemptAccessModes;

		switch (OpenMode)
		{
			case OpenMode.Input:
			{
				if ((AccessMode == AccessMode.Unspecified) || (AccessMode == AccessMode.Read))
					attemptAccessModes = Attempt_Read;
				else
					throw RuntimeException.IllegalFunctionCall(Source);

				break;
			}
			case OpenMode.Output:
			{
				if ((AccessMode == AccessMode.Unspecified) || (AccessMode == AccessMode.Write))
					attemptAccessModes = Attempt_Write;
				else
					throw RuntimeException.IllegalFunctionCall(Source);

				break;
			}
			case OpenMode.Append:
			{
				switch (AccessMode)
				{
					case AccessMode.Unspecified: attemptAccessModes = Attempt_ReadWrite_Write; break;
					case AccessMode.Write: attemptAccessModes = Attempt_Write; break;

					default: throw RuntimeException.IllegalFunctionCall(Source);
				}

				break;
			}
			case OpenMode.Random:
			case OpenMode.Binary:
			{
				switch (AccessMode)
				{
					case AccessMode.Unspecified: attemptAccessModes = Attempt_ReadWrite_Write_Read; break;
					case AccessMode.Read: attemptAccessModes = Attempt_Read; break;
					case AccessMode.Write: attemptAccessModes = Attempt_Write; break;
					case AccessMode.ReadWrite: attemptAccessModes = Attempt_ReadWrite; break;

					default: throw RuntimeException.IllegalFunctionCall(Source);
				}

				break;
			}

			default: throw RuntimeException.IllegalFunctionCall(Source);
		}

		OSOpenMode shareMode = OSOpenMode.Share_Compatibility;

		switch (LockMode)
		{
			case LockMode.LockRead: shareMode |= OSOpenMode.Share_DenyRead; break;
			case LockMode.LockWrite: shareMode |= OSOpenMode.Share_DenyWrite; break;
			case LockMode.LockReadWrite: shareMode |= OSOpenMode.Share_DenyReadWrite; break;
		}

		try
		{
			DOSError lastError = DOSError.None;

			foreach (var accessMode in attemptAccessModes)
			{
				try
				{
					openFile.FileHandle = context.Machine.DOS.OpenFile(
						fileName.Value.ToString(),
						openMode,
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

			if (OpenMode == OpenMode.Append)
				context.Machine.DOS.SeekFile(openFile.FileHandle, 0, MoveMethod.FromEnd);

			openFile.IOMode =
				OpenMode switch
				{
					OpenMode.Random => OpenFileIOMode.Random,
					OpenMode.Binary => OpenFileIOMode.Binary,
					OpenMode.Input => OpenFileIOMode.Input,
					OpenMode.Output or OpenMode.Append => OpenFileIOMode.Output,

					_ => throw new Exception("Unrecognized OpenMode value " + OpenMode)
				};

			openFile.OpenedForAppend = (OpenMode == OpenMode.Append);

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

			if (OpenMode == OpenMode.Random)
				openFile.ConfigureFields(System.Array.Empty<FileRecordField>(), context);

			context.Files[fileNumber] = openFile;
		}
		catch (DOSException ex)
		{
			throw RuntimeException.ForDOSError(ex.ToDOSError(), Source);
		}
	}
}
