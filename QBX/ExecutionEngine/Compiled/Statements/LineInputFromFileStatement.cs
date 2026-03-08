using System;

using QBX.ExecutionEngine.Execution;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class LineInputFromFileStatement(CodeModel.Statements.Statement source) : LineInputStatement(source)
{
	public Evaluable? FileNumberExpression;

	OpenFile? _openFile;

	protected override StringValue ReadLine(ExecutionContext context)
	{
		if (_openFile == null)
			throw new Exception("Internal error");

		return _openFile.ReadLine(context.Machine.DOS);
	}

	public override void Execute(ExecutionContext context, StackFrame stackFrame)
	{
		if (FileNumberExpression is null)
			throw new Exception("LineInputFromFileStatement with no FileNumberExpression");

		int fileNumber = FileNumberExpression.EvaluateAndCoerceToInt(context, stackFrame);

		if (!context.Files.TryGetValue(fileNumber, out _openFile))
			throw RuntimeException.BadFileNameOrNumber(Source);

		base.Execute(context, stackFrame);
	}
}
