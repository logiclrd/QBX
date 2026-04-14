using System;

using QBX.ExecutionEngine.Execution;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class FileWidthStatement(CodeModel.Statements.FileWidthStatement source) : Executable(source)
{
	public Evaluable? FileNumberExpression;
	public Evaluable? WidthExpression;

	public override void Execute(ExecutionContext context, StackFrame stackFrame)
	{
		if (FileNumberExpression == null)
			throw new Exception("FileNumberWidthStatement with no FileNumberExpression");
		if (WidthExpression == null)
			throw new Exception("FileNumberWidthStatement with no WidthExpression");

		int fileNumber = FileNumberExpression.EvaluateAndCoerceToInt(context, stackFrame);
		int width = WidthExpression.EvaluateAndCoerceToInt(context, stackFrame);

		if ((width <= 0) || (width > OpenFile.NoLineWidthLimit))
			throw RuntimeException.IllegalFunctionCall(Source);

		if (!context.Files.TryGetValue(fileNumber, out var file))
			throw RuntimeException.BadFileNameOrNumber(Source);

		file.LineWidth = width;
	}
}
