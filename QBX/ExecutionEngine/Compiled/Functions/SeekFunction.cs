using System;

using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;
using QBX.OperatingSystem.FileDescriptors;

namespace QBX.ExecutionEngine.Compiled.Functions;

public class SeekFunction : Function
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
			throw new Exception("SeekFunction with no FileNumberExpression");

		int fileNumber = FileNumberExpression.EvaluateAndCoerceToInt(context, stackFrame);

		if (!context.Files.TryGetValue(fileNumber, out var openFile))
			throw RuntimeException.BadFileNameOrNumber(FileNumberExpression.Source?.Token ?? Source?.Token);

		int filePointer;

		if (openFile.IOMode == OpenFileIOMode.Random)
			filePointer = openFile.CurrentRecordNumber;
		else
		{
			if ((openFile.FileHandle < 2)
			 || (openFile.FileHandle >= context.Machine.DOS.Files.Count)
			 || (context.Machine.DOS.Files[openFile.FileHandle] is not FileDescriptor fileDescriptor))
				throw RuntimeException.IllegalFunctionCall(Source); // internal error

			filePointer = checked((int)fileDescriptor.FilePointer);
		}

		return new LongVariable(filePointer + 1);
	}
}
