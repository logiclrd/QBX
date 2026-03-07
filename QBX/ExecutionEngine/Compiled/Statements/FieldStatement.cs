using System;
using System.Collections.Generic;
using System.Linq;

using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class FieldStatement(CodeModel.Statements.Statement source) : Executable(source)
{
	public Evaluable? FileNumberExpression;
	public List<FieldMapping> FieldMappings = new List<FieldMapping>();

	public override void Execute(ExecutionContext context, StackFrame stackFrame)
	{
		if (FileNumberExpression == null)
			throw new Exception($"SeekStatement with no FileNumberExpression");

		int fileNumber = FileNumberExpression.EvaluateAndCoerceToInt(context, stackFrame);

		if (!context.Files.TryGetValue(fileNumber, out var openFile))
			throw RuntimeException.BadFileNameOrNumber(Source);

		var fields = FieldMappings.Select(mapping =>
			new FileRecordField(
				mapping.WidthExpression.EvaluateAndCoerceToInt(context, stackFrame),
				(StringVariable)mapping.StringVariableExpression.Evaluate(context, stackFrame)))
			.ToList();

		openFile.ConfigureFields(fields, context);
	}
}
