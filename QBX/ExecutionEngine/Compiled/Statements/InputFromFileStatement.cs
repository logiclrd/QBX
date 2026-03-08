using System;
using System.Collections.Generic;

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

		var inputLine = openFile.ReadLine(context.Machine.DOS);

		var parser = new DataParser();

		parser.AddDataSource(
			DataParser.ParseDataItems(inputLine.ToString()));

		parser.ReadDataItems(TargetExpressions, context, stackFrame, source);

		if (!parser.IsAtEnd)
			throw RuntimeException.InputPastEndOfFile(Source);
	}
}
