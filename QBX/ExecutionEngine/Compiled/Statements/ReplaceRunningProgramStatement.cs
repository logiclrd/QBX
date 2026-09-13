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
			ConfigureContext(context);

			throw new ReplaceRunningProgram(fileName, source);
		}
		catch (DOSException ex)
		{
			throw RuntimeException.ForDOSError(ex.ToDOSError(), Source);
		}
	}
}
