using System;
using System.Collections.Generic;

using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;
using QBX.Numbers;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class WriteStatement(CodeModel.Statements.WriteStatement source) : Executable(source)
{
	public Evaluable? FileNumberExpression;
	public List<Evaluable> Arguments = new List<Evaluable>();

	public override void Execute(ExecutionContext context, StackFrame stackFrame)
	{
		if (FileNumberExpression is null)
			throw new Exception("WriteStatement with no FileNumberExpression");

		int fileNumber = FileNumberExpression.EvaluateAndCoerceToInt(context, stackFrame);

		if (!context.Files.TryGetValue(fileNumber, out var openFile))
			throw RuntimeException.BadFileNameOrNumber(Source);

		var emitter = new FilePrintEmitter(context, openFile);

		bool first = true;

		foreach (var argument in Arguments)
		{
			if (first)
				first = false;
			else
				emitter.Emit((byte)',');

			var value = argument.Evaluate(context, stackFrame);

			if (value is not StringVariable stringValue)
				emitter.Emit(NumberFormatter.Format(value, qualify: false, argument.Source));
			else
			{
				emitter.Emit((byte)'"');
				emitter.Emit(stringValue.ValueSpan);
				emitter.Emit((byte)'"');
			}
		}

		emitter.EmitNewLine();
	}
}
