using System;

using QBX.ExecutionEngine.Compiled.Expressions;
using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;
using QBX.OperatingSystem;
using QBX.OperatingSystem.FileDescriptors;

namespace QBX.ExecutionEngine.Compiled.Functions;

public class EOFFunction : Function
{
	public override DataType Type => DataType.Integer;

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

		if (openFile.IOMode == OpenFileIOMode.Output)
			throw RuntimeException.BadFileMode(Source);

		if ((openFile.FileHandle < 2)
		 || (openFile.FileHandle >= context.Machine.DOS.Files.Count)
		 || (context.Machine.DOS.Files[openFile.FileHandle] is not FileDescriptor fileDescriptor))
			throw RuntimeException.IllegalFunctionCall(Source); // internal error

		bool atEOF;

		if (fileDescriptor.AtSoftEOF)
			atEOF = true;
		else if ((fileDescriptor is RegularFileDescriptor regularFileDescriptor)
					&& (regularFileDescriptor.FilePointer >= regularFileDescriptor.Length))
			atEOF = true;
		else if ((openFile.IOMode == OpenFileIOMode.Random) || (openFile.IOMode == OpenFileIOMode.Binary))
			atEOF = false;
		else
		{
			try
			{
				if (!fileDescriptor.TryReadByte(out byte b))
					atEOF = true;
				else
				{
					atEOF = false;
					fileDescriptor.ReadBuffer.Inject(b);
				}
			}
			catch (DOSException ex)
			{
				throw RuntimeException.ForDOSError(ex.ToDOSError(), Source);
			}
		}

		short returnValue = atEOF ? IntegerLiteralValue.True : IntegerLiteralValue.False;

		return new IntegerVariable(returnValue);
	}
}
