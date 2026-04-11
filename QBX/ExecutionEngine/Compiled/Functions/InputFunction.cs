using System;
using System.Collections.Generic;
using System.Linq;

using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;
using QBX.LexicalAnalysis;

namespace QBX.ExecutionEngine.Compiled.Functions;

public abstract class InputFunction : ConstructibleFunction
{
	public override DataType Type => DataType.String;

	Evaluable? _numBytesExpression;

	public Evaluable? NumBytesExpression => _numBytesExpression;

	protected InputFunction(Evaluable? numBytesExpression)
	{
		_numBytesExpression = numBytesExpression;
	}

	public override void CollapseConstantSubexpressions()
	{
		CollapseConstantExpression(ref _numBytesExpression);
	}

	public static InputFunction Construct(Token? token, IEnumerable<Evaluable> arguments)
	{
		var argList = arguments.Take(2).ToList();

		if ((argList.Count < 1) || (argList.Count > 2))
			throw CompilerException.ArgumentCountMismatch(token);

		var numBytesArg = argList[0];

		if (!numBytesArg.Type.IsNumeric)
			throw CompilerException.TypeMismatch(numBytesArg.Source?.Token ?? token);

		switch (argList.Count)
		{
			case 1: return new ConsoleInputFunction(numBytesArg);

			case 2:
			{
				var fileNumberArg = argList[1];

				if (!fileNumberArg.Type.IsNumeric)
					throw CompilerException.TypeMismatch(fileNumberArg.Source?.Token ?? token);

				return new FileInputFunction(numBytesArg, fileNumberArg);
			}

			default: throw new Exception("Internal error");
		}
	}
}

public class ConsoleInputFunction(Evaluable numBytesArgument) : InputFunction(numBytesArgument)
{
	public override Variable Evaluate(ExecutionContext context, StackFrame stackFrame)
	{
		if (NumBytesExpression == null)
			throw new Exception("ConsoleInputFunction with no NumBytesExpression");

		int numBytes = NumBytesExpression.EvaluateAndCoerceToInt(context, stackFrame);

		if (numBytes < 0)
			throw RuntimeException.IllegalFunctionCall(Source);

		var buffer = new StringValue();

		if (numBytes > 0)
		{
			var cancelOnBreak = new System.Threading.CancellationTokenSource();

			void Break()
			{
				cancelOnBreak.Cancel();
			}

			context.Machine.Keyboard.Break += Break;

			try
			{
				buffer.Capacity = numBytes;

				while (buffer.Length < numBytes)
				{
					if (!context.Machine.Keyboard.HasQueuedTangibleInput)
						context.Machine.Keyboard.WaitForInput(cancelOnBreak.Token);

					if (cancelOnBreak.IsCancellationRequested)
						throw new BreakExecution();

					var evt = context.Machine.Keyboard.GetNextEvent();

					if (evt != null)
					{
						string str = evt.ToInKeyString();

						if (str.Length > 0)
							buffer.Append(str[0]);
					}
				}
			}
			finally
			{
				context.Machine.Keyboard.Break -= Break;
			}
		}

		return new StringVariable(buffer);
	}
}

public class FileInputFunction(Evaluable numBytesArgument, Evaluable fileNumberArgument) : InputFunction(numBytesArgument)
{
	public Evaluable FileNumberExpression => fileNumberArgument;

	public override Variable Evaluate(ExecutionContext context, StackFrame stackFrame)
	{
		if (NumBytesExpression == null)
			throw new Exception("FileInputFunction with no NumBytesExpression");
		if (FileNumberExpression == null)
			throw new Exception("FileInputFunction with no FileNumberExpression");

		int numBytes = NumBytesExpression.EvaluateAndCoerceToInt(context, stackFrame);
		int fileNumber = FileNumberExpression.EvaluateAndCoerceToInt(context, stackFrame);

		if (!context.Files.TryGetValue(fileNumber, out var openFile))
			throw RuntimeException.BadFileNameOrNumber(fileNumberArgument.Source?.Token ?? Source?.Token);
		if (numBytes < 0)
			throw RuntimeException.IllegalFunctionCall(Source);

		var buffer = new StringValue();

		buffer.Length = numBytes;

		if (openFile.IOMode == OpenFileIOMode.Random)
		{
			try
			{
				openFile.ReadFromFields(buffer.AsSpan());
			}
			catch (RuntimeException ex)
			{
				throw ex.AddContext(Source);
			}
		}
		else
		{
			int numRead = context.Machine.DOS.Read(
				openFile.FileHandle,
				buffer.AsSpan());

			if (numRead < buffer.Length)
				throw RuntimeException.InputPastEndOfFile(Source);
		}

		return new StringVariable(buffer);
	}
}
