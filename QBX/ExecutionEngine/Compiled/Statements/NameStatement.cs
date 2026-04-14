using System;

using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;
using QBX.Firmware.Fonts;
using QBX.OperatingSystem;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class NameStatement(CodeModel.Statements.NameStatement source) : Executable(source)
{
	public Evaluable? OldFileSpecExpression;
	public Evaluable? NewFileSpecExpression;

	public override void Execute(ExecutionContext context, StackFrame stackFrame)
	{
		if (OldFileSpecExpression == null)
			throw new Exception("KillStatement with no OldFileSpecExpression");
		if (NewFileSpecExpression == null)
			throw new Exception("KillStatement with no NewFileSpecExpression");

		var oldFileSpec = (StringVariable)OldFileSpecExpression.Evaluate(context, stackFrame);
		var newFileSpec = (StringVariable)NewFileSpecExpression.Evaluate(context, stackFrame);

		try
		{
			var oldFileSpecValue = oldFileSpec.Value;
			var newFileSpecValue = newFileSpec.Value;

			if (!IsValidFileSpec(oldFileSpecValue))
				throw RuntimeException.BadFileName(OldFileSpecExpression.Source);
			if (!IsValidFileSpec(newFileSpecValue))
				throw RuntimeException.BadFileName(NewFileSpecExpression.Source);

			context.Machine.DOS.RenameFile(
				oldFileSpec.Value,
				newFileSpec.Value);
		}
		catch (DOSException ex)
		{
			throw RuntimeException.ForDOSError(ex.ToDOSError(), Source);
		}
	}

	static bool IsValidFileSpec(StringValue spec)
	{
		for (int i = 0; i < spec.Length; i++)
		{
			byte ch = spec[i];

			if (ch == ':')
			{
				if (i != 1)
					return false;
				if (!char.IsAsciiLetter(CP437Encoding.GetCharSemantic(spec[0])))
					return false;
			}
			else
			{
				if (!ShortPath.IsDirectorySeparator(ch)
				 && !ShortPath.IsValid(ch)
				 && (ch != (byte)'.'))
					return false;
			}
		}

		return true;
	}
}
