using System;
using System.IO;

using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;
using QBX.OperatingSystem;
using QBX.OperatingSystem.FileDescriptors;

using OSFileMode = QBX.OperatingSystem.FileStructures.FileMode;
using OSOpenMode = QBX.OperatingSystem.FileStructures.OpenMode;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class ReplaceRunningProgramStatement(CodeModel.Statements.Statement source) : Executable(source)
{
	public Evaluable? FileNameExpression;

	public override bool CanExecuteDirectWithoutEmbedding => FileNameExpression?.CanEvaluateDirectWithoutEmbedding ?? true;

	protected virtual void ConfigureContext(ExecutionContext context) { }

	protected override void ExecuteImplementation(ExecutionContext context, StackFrame stackFrame)
	{
		if (FileNameExpression == null)
			throw new Exception(GetType().Name + " with no FileNameExpression");

		var fileNameResult = (StringVariable)FileNameExpression.Evaluate(context, stackFrame);

		string fileName = fileNameResult.ValueString;

		try
		{
			int fileHandle = context.Machine.DOS.OpenFile(
				fileName,
				OSFileMode.Open,
				OSOpenMode.Access_ReadOnly | OSOpenMode.Share_DenyWrite);

			if (context.Machine.DOS.LastError == DOSError.FileNotFound)
			{
				fileName = fileName.TrimEnd('.') + ".BAS";

				fileHandle = context.Machine.DOS.OpenFile(
					fileName,
					OSFileMode.Open,
					OSOpenMode.Access_ReadOnly | OSOpenMode.Share_DenyWrite);
			}

			if (context.Machine.DOS.LastError != DOSError.None)
				throw RuntimeException.ForDOSError(context.Machine.DOS.LastError, Source);

			ConfigureContext(context);

			throw new ReplaceRunningProgram(fileName, fileHandle, source);
		}
		catch (DOSException ex)
		{
			throw RuntimeException.ForDOSError(ex.ToDOSError(), Source);
		}
	}
}
