using System;

using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;
using QBX.OperatingSystem;
using QBX.OperatingSystem.FileStructures;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class OpenStatement(CodeModel.Statements.Statement source) : Executable(source)
{
	public OpenMode OpenMode;
	public AccessMode AccessMode;
	public LockMode LockMode;
	public Evaluable? FileNameExpression;
	public Evaluable? FileNumberExpression;
	public Evaluable? RecordLengthExpression;

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

		var accessModes =
			OpenMode switch
			{
				OpenMode.Input => OperatingSystem.FileStructures.OpenMode.Access_ReadOnly,
				OpenMode.Output or OpenMode.Append => OperatingSystem.FileStructures.OpenMode.Access_WriteOnly,
				OpenMode.Random or OpenMode.Binary => OperatingSystem.FileStructures.OpenMode.Access_ReadWrite,

				_ => throw new Exception("Unrecognized OpenMode value " + OpenMode)
			};

		switch (LockMode)
		{
			case LockMode.LockRead: accessModes |= OperatingSystem.FileStructures.OpenMode.Share_DenyRead; break;
			case LockMode.LockWrite: accessModes |= OperatingSystem.FileStructures.OpenMode.Share_DenyWrite; break;
			case LockMode.LockReadWrite: accessModes |= OperatingSystem.FileStructures.OpenMode.Share_DenyReadWrite; break;
		}

		try
		{
			openFile.FileHandle = context.Machine.DOS.OpenFile(
				fileName.Value.ToString(),
				openMode,
				accessModes);

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

			context.Files[fileNumber] = openFile;
		}
		catch (DOSException ex)
		{
			throw RuntimeException.ForDOSError(ex.ToDOSError(), Source);
		}
	}
}
