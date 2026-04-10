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

		try
		{
			if (_openFile.DataParser != null)
			{
				if (_openFile.DataParser.IsAtEnd)
					throw RuntimeException.InputPastEndOfFile(Source);

				return new StringValue(_openFile.DataParser.ReadLine(Source));
			}
			else
				return _openFile.ReadLine(context.Machine.DOS) ?? throw RuntimeException.InputPastEndOfFile(Source);
		}
		catch (RuntimeException e)
		{
			throw e.AddContext(Source);
		}
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
