using System;

using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;

namespace QBX.ExecutionEngine.Compiled.Functions;

public class BOFFunction : Function
{
	public override DataType Type => DataType.Integer;

	public Evaluable? FileNumberExpression;

	protected override void SetArgument(int index, Evaluable value)
	{
		if (!value.Type.IsNumeric)
			throw CompilerException.TypeMismatch(value.Source);

		FileNumberExpression = value;
	}

	public override void CollapseConstantSubexpressions()
	{
		CollapseConstantExpression(ref FileNumberExpression);
	}

	public override Variable Evaluate(ExecutionContext context, StackFrame stackFrame)
	{
		if (FileNumberExpression == null)
			throw new Exception("BOFFunction with no FileNumberExpression");

		int fileNumber = FileNumberExpression.EvaluateAndCoerceToInt(context, stackFrame);

		if (!context.Files.TryGetValue(fileNumber, out var openFile))
			throw RuntimeException.BadFileNameOrNumber(FileNumberExpression.Source?.Token ?? Source?.Token);

		// This function is exclusively for use with ISAM tables.
		// It is implemented here for completeness but will always
		// fail because it isn't possible to open ISAM tables
		// as that is not presently emulated by QBX.
		throw RuntimeException.BadFileMode(Source);
	}
}
