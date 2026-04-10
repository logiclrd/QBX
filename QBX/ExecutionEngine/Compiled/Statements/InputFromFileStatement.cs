using System;
using System.Collections.Generic;
using System.IO;

using QBX.ExecutionEngine.Execution;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class InputFromFileStatement(CodeModel.Statements.Statement source) : Executable(source)
{
	public Evaluable? FileNumberExpression;
	public List<Evaluable> TargetExpressions = new List<Evaluable>();

	public override void Execute(ExecutionContext context, StackFrame stackFrame)
	{
		if (FileNumberExpression is null)
			throw new Exception("InputFromFileStatement with no FileNumberExpression");

		int fileNumber = FileNumberExpression.EvaluateAndCoerceToInt(context, stackFrame);

		if (!context.Files.TryGetValue(fileNumber, out var openFile))
			throw RuntimeException.BadFileNameOrNumber(Source);

		if (openFile.DataParser == null)
		{
			openFile.DataParser = new DataParser();
			openFile.DataParser.RestartFromFile(openFile, context.Machine.DOS);
		}

		try
		{
			openFile.DataParser.ReadDataItems(TargetExpressions, context, stackFrame, source);
		}
		catch (RuntimeException e)
		{
			if (e.ErrorNumber == 4) /* Out of data */
				throw RuntimeException.InputPastEndOfFile(Source);

			e.AddContext(Source);
			throw;
		}
	}
}
