using System;

using QBX.ExecutionEngine.Compiled.Expressions;
using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;
using QBX.OperatingSystem;
using QBX.OperatingSystem.FileDescriptors;

namespace QBX.ExecutionEngine.Compiled.Functions;

public class FileAttrFunction : Function
{
	public override DataType Type => DataType.Integer;

	protected override int MinArgumentCount => 2;
	protected override int MaxArgumentCount => 2;

	public Evaluable? FileNumberExpression;
	public Evaluable? AttributeExpression;

	const int Attribute_FileMode = 1;
	const int Attribute_DOSFileHandle = 2;

	protected override void SetArgument(int index, Evaluable value)
	{
		if (!value.Type.IsNumeric)
			throw CompilerException.TypeMismatch(value.Source);

		switch (index)
		{
			case 0: FileNumberExpression = value; break;
			case 1: AttributeExpression = value; break;
		}
	}

	public override Variable Evaluate(ExecutionContext context, StackFrame stackFrame)
	{
		if (FileNumberExpression == null)
			throw new Exception("FileAttrFunction with no FileNumberExpression");
		if (AttributeExpression == null)
			throw new Exception("FileAttrFunction with no AttributeExpression");

		int fileNumber = FileNumberExpression.EvaluateAndCoerceToInt(context, stackFrame);

		if (!context.Files.TryGetValue(fileNumber, out var openFile))
			throw RuntimeException.BadFileNameOrNumber(FileNumberExpression.Source?.Token ?? Source?.Token);

		if (openFile.IOMode == OpenFileIOMode.Output)
			throw RuntimeException.BadFileMode(Source);

		if ((openFile.FileHandle < 2)
		 || (openFile.FileHandle >= context.Machine.DOS.Files.Count)
		 || (context.Machine.DOS.Files[openFile.FileHandle] is not FileDescriptor fileDescriptor))
			throw RuntimeException.IllegalFunctionCall(Source); // internal error

		int attribute = AttributeExpression.EvaluateAndCoerceToInt(context, stackFrame);

		short result;

		switch (attribute)
		{
			case Attribute_FileMode:
				if (openFile.OpenedForAppend)
					result = 8;
				else
				{
					result =
						openFile.IOMode switch
						{
							OpenFileIOMode.Input => 1,
							OpenFileIOMode.Output => 2,
							OpenFileIOMode.Random => 4,
							OpenFileIOMode.Binary => 32,

							_ => throw new Exception("Internal error: Unrecognized OpenFileIOMode " + openFile.IOMode)
						};
				}

				break;

			case Attribute_DOSFileHandle:
				result = checked((short)openFile.FileHandle);
				break;

			default:
				throw RuntimeException.IllegalFunctionCall(Source);
		}

		return new IntegerVariable(result);
	}
}
