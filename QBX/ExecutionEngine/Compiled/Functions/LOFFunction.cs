using System;

using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;
using QBX.OperatingSystem.FileStructures;

namespace QBX.ExecutionEngine.Compiled.Functions;

public class LOFFunction : Function
{
	public override DataType Type => DataType.Long;

	public Evaluable? FileNumberExpression;

	protected override void SetArgument(int index, Evaluable value)
	{
		if (!value.Type.IsNumeric)
			throw CompilerException.TypeMismatch(value.Source);

		FileNumberExpression = value;
	}

	public override Variable Evaluate(ExecutionContext context, StackFrame stackFrame)
	{
		if (FileNumberExpression == null)
			throw new Exception("LOFFunction with no FileNumberExpression");

		int fileNumber = FileNumberExpression.EvaluateAndCoerceToInt(context, stackFrame);

		if (!context.Files.TryGetValue(fileNumber, out var openFile))
			throw RuntimeException.BadFileNameOrNumber(FileNumberExpression.Source?.Token ?? Source?.Token);

		uint savedOffset = context.Machine.DOS.SeekFile(openFile.FileHandle, 0, MoveMethod.FromCurrent);

		try
		{
			uint length = context.Machine.DOS.SeekFile(openFile.FileHandle, 0, MoveMethod.FromEnd);

			return new LongVariable((int)length);
		}
		finally
		{
			context.Machine.DOS.SeekFile(openFile.FileHandle, savedOffset, MoveMethod.FromBeginning);
		}
	}
}
