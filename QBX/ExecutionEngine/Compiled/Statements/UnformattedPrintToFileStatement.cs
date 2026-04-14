using System;

using QBX.ExecutionEngine.Execution;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class UnformattedPrintToFileStatement(CodeModel.Statements.PrintStatement source) : UnformattedPrintStatement(source)
{
	public Evaluable? FileNumberExpression;

	protected override PrintEmitter CreateEmitter(ExecutionContext context, StackFrame stackFrame)
	{
		if (FileNumberExpression is null)
			throw new Exception("UnformattedPrintToFileStatement with no FileNumberExpression");

		int fileNumber = FileNumberExpression.EvaluateAndCoerceToInt(context, stackFrame);

		if (!context.Files.TryGetValue(fileNumber, out var openFile))
			throw RuntimeException.BadFileNameOrNumber(Source);

		return new FilePrintEmitter(context, openFile);
	}
}
