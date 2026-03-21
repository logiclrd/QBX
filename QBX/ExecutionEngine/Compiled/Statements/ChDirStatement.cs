using System;
using System.IO;

using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;
using QBX.OperatingSystem;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class ChDirStatement(CodeModel.Statements.Statement source) : Executable(source)
{
	public Evaluable? PathExpression;

	public override void Execute(ExecutionContext context, StackFrame stackFrame)
	{
		if (PathExpression == null)
			throw new Exception("ChDirStatement with no PathExpression");

		var pathResult = (StringVariable)PathExpression.Evaluate(context, stackFrame);

		string path = pathResult.ValueString;

		try
		{
			if (path.Length == 0)
				throw RuntimeException.PathNotFound(Source);

			Environment.CurrentDirectory = ShortFileNames.Unmap(path);
		}
		catch (IOException ex)
		{
			throw RuntimeException.ForIOException(ex);
		}
	}
}
