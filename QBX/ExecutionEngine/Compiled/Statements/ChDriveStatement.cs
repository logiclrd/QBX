using System;
using System.IO;
using System.Runtime.InteropServices;

using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class ChDriveStatement(CodeModel.Statements.ChDriveStatement source) : Executable(source)
{
	public Evaluable? DriveLetterExpression;

	public override void Execute(ExecutionContext context, StackFrame stackFrame)
	{
		if (DriveLetterExpression == null)
			throw new Exception("ChDriveStatement with no DriveLetterExpression");

		var driveLetterResult = (StringVariable)DriveLetterExpression.Evaluate(context, stackFrame);

		string driveLetter = driveLetterResult.ValueString;

		try
		{
			if (driveLetter.Length > 0)
			{
				if (!char.IsAsciiLetter(driveLetter[0]))
					throw RuntimeException.IllegalFunctionCall(Source);

				if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
				{
					driveLetter = driveLetter[0] + ":";

					Environment.CurrentDirectory = driveLetter;
				}
				else
				{
					if (char.ToUpperInvariant(driveLetter[0]) != 'C')
						throw RuntimeException.DeviceUnavailable(Source);
				}
			}
		}
		catch (IOException ex)
		{
			throw RuntimeException.ForIOException(ex);
		}
	}
}
