using System;
using System.Diagnostics;

using QBX.ExecutionEngine.Execution.Variables;
using QBX.Firmware.Fonts;
using QBX.OperatingSystem;

using ExecutionContext = QBX.ExecutionEngine.Execution.ExecutionContext;
using StackFrame = QBX.ExecutionEngine.Execution.StackFrame;

namespace QBX.ExecutionEngine.Compiled.Functions;

public class ShellFunction : Function
{
	public Evaluable? CommandString;

	protected override void SetArgument(int index, Evaluable value)
	{
		if (!value.Type.IsString)
			throw CompilerException.TypeMismatch(value.Source);

		CommandString = value;
	}

	public override void CollapseConstantSubexpressions()
	{
		CollapseConstantExpression(ref CommandString);
	}

	public override DataType Type => DataType.Long;

	static CP437Encoding s_cp437 = new CP437Encoding(ControlCharacterInterpretation.Semantic);

	public override Variable Evaluate(ExecutionContext context, StackFrame stackFrame)
	{
		if (CommandString == null)
			throw new Exception("ShellFunction with no CommandString");

		var commandLine = ((StringVariable)CommandString.Evaluate(context, stackFrame)).Value;

		var commandSpan = commandLine.AsSpan();

		while ((commandSpan.Length > 0) && ShortPath.IsSpace(commandSpan[0]))
			commandSpan = commandSpan.Slice(1);

		if (commandSpan.Length == 0)
			throw RuntimeException.BadFileName(CommandString.Source);

		int fileNameStart, fileNameEnd;
		ReadOnlySpan<byte> argumentSpan;

		if (commandSpan[0] == '"')
		{
			fileNameStart = 1;
			fileNameEnd = commandSpan.Slice(1).IndexOf((byte)'"');

			if (fileNameEnd >= 0)
				argumentSpan = commandSpan.Slice(fileNameEnd + 1);
			else
			{
				fileNameEnd = commandSpan.Length;
				argumentSpan = commandSpan.Slice(fileNameEnd);
			}
		}
		else
		{
			fileNameStart = 0;
			fileNameEnd = 1;

			while ((fileNameEnd < commandSpan.Length) && !ShortPath.IsSpace(commandSpan[fileNameEnd]))
				fileNameEnd++;

			argumentSpan = commandSpan.Slice(fileNameEnd);
		}

		while ((argumentSpan.Length > 0) && ShortPath.IsSpace(argumentSpan[0]))
			argumentSpan = argumentSpan.Slice(1);

		var psi = new ProcessStartInfo();

		psi.FileName = s_cp437.GetString(commandSpan.Slice(fileNameStart, fileNameEnd - fileNameStart));
		psi.Arguments = s_cp437.GetString(argumentSpan);
		psi.UseShellExecute = false;

		var process = Process.Start(psi);

		if (process == null)
			throw RuntimeException.IllegalFunctionCall(Source);

		return new LongVariable(process.Id);
	}
}
