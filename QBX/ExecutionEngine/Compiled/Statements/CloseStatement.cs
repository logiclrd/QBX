using System.Collections.Generic;

using QBX.ExecutionEngine.Execution;
using QBX.OperatingSystem;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class CloseStatement(CodeModel.Statements.Statement source) : Executable(source)
{
	public List<Evaluable> FileNumberExpressions = new List<Evaluable>();

	public override void Execute(ExecutionContext context, StackFrame stackFrame)
	{
		foreach (var fileNumberExpression in FileNumberExpressions)
		{
			int fileNumber = fileNumberExpression.EvaluateAndCoerceToInt(context, stackFrame);

			if (context.Files.TryGetValue(fileNumber, out var openFile))
			{
				try
				{
					context.Machine.DOS.CloseFile(openFile.FileHandle);
				}
				catch (DOSException ex)
				{
					throw RuntimeException.ForDOSError(ex.ToDOSError(), Source);
				}
				finally
				{
					openFile.ClearFieldConfiguration(context);
					context.Files.Remove(fileNumber);
				}
			}
		}
	}
}
