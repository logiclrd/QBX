using System;
using System.Linq;

using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;
using QBX.OperatingSystem;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class KillStatement(CodeModel.Statements.KillStatement source) : Executable(source)
{
	public Evaluable? FilePatternExpression;

	public override void Execute(ExecutionContext context, StackFrame stackFrame)
	{
		if (FilePatternExpression == null)
			throw new Exception("KillStatement with no FilePatternExpression");

		var filePattern = (StringVariable)FilePatternExpression.Evaluate(context, stackFrame);

		try
		{
			context.Machine.DOS.DeleteFiles(
				filePattern.ValueString,
				(shortName, fullPath) =>
				{
					if (context.Machine.DOS.FileIsOpenAsOneOf(fullPath, context.Files.Values.Select(openFile => openFile.FileHandle)))
						throw RuntimeException.FileAlreadyOpen(Source);
				});
		}
		catch (DOSException ex)
		{
			throw RuntimeException.ForDOSError(ex.ToDOSError(), Source);
		}
	}
}
