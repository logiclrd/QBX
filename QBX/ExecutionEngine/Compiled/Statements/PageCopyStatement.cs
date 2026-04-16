using System;

using QBX.ExecutionEngine.Execution;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class PageCopyStatement(CodeModel.Statements.PageCopyStatement source) : Executable(source)
{
	public Evaluable? SourcePageExpression;
	public Evaluable? DestinationPageExpression;

	public override void Execute(ExecutionContext context, StackFrame stackFrame)
	{
		if (SourcePageExpression == null)
			throw new Exception("PageCopyStatement with no SourcePageExpression");
		if (DestinationPageExpression == null)
			throw new Exception("PageCopyStatement with no DestinationPageExpression");

		int sourcePage = SourcePageExpression.EvaluateAndCoerceToInt(context, stackFrame);
		int destinationPage = DestinationPageExpression.EvaluateAndCoerceToInt(context, stackFrame);

		try
		{
			context.VisualLibrary.CopyPage(sourcePage, destinationPage);
		}
		catch (ArgumentException)
		{
			throw RuntimeException.IllegalFunctionCall();
		}
	}
}
