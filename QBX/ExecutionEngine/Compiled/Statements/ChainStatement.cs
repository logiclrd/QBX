using System;
using System.IO;

using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;
using QBX.OperatingSystem;
using QBX.OperatingSystem.FileDescriptors;

using OSFileMode = QBX.OperatingSystem.FileStructures.FileMode;
using OSOpenMode = QBX.OperatingSystem.FileStructures.OpenMode;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class ChainStatement(CodeModel.Statements.Statement source) : Executable(source)
{
	public Evaluable? FileNameExpression;

	public override void Execute(ExecutionContext context, StackFrame stackFrame)
	{
		if (FileNameExpression == null)
			throw new Exception("ChainStatement with no FileNameExpression");

		var fileNameResult = (StringVariable)FileNameExpression.Evaluate(context, stackFrame);

		string fileName = fileNameResult.ValueString;

		try
		{
			int fileHandle = -1;
			bool openSucceeded = false;

			if (Path.GetExtension(fileName) == "")
			{
				fileHandle = context.Machine.DOS.OpenFile(
					fileName,
					OSFileMode.Open,
					OSOpenMode.Access_ReadOnly | OSOpenMode.Share_DenyNone);

				switch (context.Machine.DOS.LastError)
				{
					case DOSError.None: openSucceeded = true; break;
					case DOSError.FileNotFound: fileName = fileName.TrimEnd('.') + ".BAS"; break;
					default: throw RuntimeException.ForDOSError(context.Machine.DOS.LastError, Source);
				}
			}

			if (!openSucceeded) // try again because we've altered fileName
			{
				fileHandle = context.Machine.DOS.OpenFile(
					fileName,
					OSFileMode.Open,
					OSOpenMode.Access_ReadOnly | OSOpenMode.Share_DenyNone);

				if (context.Machine.DOS.LastError != DOSError.None)
					throw RuntimeException.ForDOSError(context.Machine.DOS.LastError, Source);
			}

			if ((fileHandle < 2) || (fileHandle >= context.Machine.DOS.Files.Count))
				throw RuntimeException.ForDOSError(DOSError.InvalidHandle, Source);

			var fileDescriptor = context.Machine.DOS.Files[fileHandle];

			if (fileDescriptor is not RegularFileDescriptor regularFileDescriptor)
				throw RuntimeException.ForDOSError(DOSError.GeneralFailure, Source);

			var reader = new StreamReader(regularFileDescriptor.UnderlyingStream);
			string actualFilePath = regularFileDescriptor.PhysicalPath;

			context.LoadReplacement(reader, actualFilePath);

			throw new ChainExecution();
		}
		catch (DOSException ex)
		{
			throw RuntimeException.ForDOSError(ex.ToDOSError(), Source);
		}
	}
}
