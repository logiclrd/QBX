using System;
using System.Collections.Generic;
using System.Linq;

using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;
using QBX.OperatingSystem.FileStructures;

using OSOpenMode = QBX.OperatingSystem.FileStructures.OpenMode;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class OpenStatement(CodeModel.Statements.OpenStatement source) : OpenStatementBase(source)
{
	public OpenMode OpenMode;
	public AccessMode AccessMode;
	public LockMode LockMode;
	public Evaluable? FileNameExpression;
	public Evaluable? FileNumberExpression;
	public Evaluable? RecordLengthExpression;

	protected override void ExecuteImplementation(ExecutionContext context, StackFrame stackFrame)
	{
		if (FileNameExpression == null)
			throw new Exception("OpenStatement with no FileNameExpression");
		if (FileNumberExpression == null)
			throw new Exception("OpenStatement with no FileNumberExpression");

		int fileNumber = FileNumberExpression.EvaluateAndCoerceToInt(context, stackFrame);

		if (context.Files.ContainsKey(fileNumber))
			throw RuntimeException.FileAlreadyOpen(Source);

		var fileName = (StringVariable)FileNameExpression.Evaluate(context, stackFrame);

		if ((OpenMode == OpenMode.Output)
		 && (OpenMode == OpenMode.Append))
		{
			if (context.Machine.DOS.FileIsOpenAsOneOf(fileName.Value, context.Files.Values.Select(openFile => openFile.FileHandle)))
				throw RuntimeException.FileAlreadyOpen(Source);
		}

		int? recordLength = RecordLengthExpression?.EvaluateAndCoerceToInt(context, stackFrame);

		var openMode =
			OpenMode switch
			{
				OpenMode.Random or OpenMode.Binary or OpenMode.Append => FileMode.OpenOrCreate,
				OpenMode.Input => FileMode.Open,
				OpenMode.Output => FileMode.Create,

				_ => throw new Exception("Unrecognized OpenMode value " + OpenMode)
			};

		IEnumerable<OSOpenMode> attemptAccessModes;

		switch (OpenMode)
		{
			case OpenMode.Input:
			{
				if ((AccessMode == AccessMode.Unspecified) || (AccessMode == AccessMode.Read))
					attemptAccessModes = Attempt_Read;
				else
					throw RuntimeException.IllegalFunctionCall(Source);

				break;
			}
			case OpenMode.Output:
			{
				if ((AccessMode == AccessMode.Unspecified) || (AccessMode == AccessMode.Write))
					attemptAccessModes = Attempt_Write;
				else
					throw RuntimeException.IllegalFunctionCall(Source);

				break;
			}
			case OpenMode.Append:
			{
				switch (AccessMode)
				{
					case AccessMode.Unspecified: attemptAccessModes = Attempt_ReadWrite_Write; break;
					case AccessMode.ReadWrite: attemptAccessModes = Attempt_ReadWrite; break;
					case AccessMode.Write: attemptAccessModes = Attempt_Write; break;

					default: throw RuntimeException.IllegalFunctionCall(Source);
				}

				break;
			}
			case OpenMode.Random:
			case OpenMode.Binary:
			{
				switch (AccessMode)
				{
					case AccessMode.Unspecified: attemptAccessModes = Attempt_ReadWrite_Write_Read; break;
					case AccessMode.Read: attemptAccessModes = Attempt_Read; break;
					case AccessMode.Write: attemptAccessModes = Attempt_Write; break;
					case AccessMode.ReadWrite: attemptAccessModes = Attempt_ReadWrite; break;

					default: throw RuntimeException.IllegalFunctionCall(Source);
				}

				break;
			}

			default: throw RuntimeException.IllegalFunctionCall(Source);
		}

		OSOpenMode shareMode = OSOpenMode.Share_Compatibility;

		switch (LockMode)
		{
			case LockMode.LockRead: shareMode |= OSOpenMode.Share_DenyRead; break;
			case LockMode.LockWrite: shareMode |= OSOpenMode.Share_DenyWrite; break;
			case LockMode.LockReadWrite: shareMode |= OSOpenMode.Share_DenyReadWrite; break;
		}

		PerformOpen(
			fileName.ToString(),
			OpenMode,
			fileMode: openMode,
			shareMode,
			attemptAccessModes,
			recordLength,
			fileNumber,
			context);
	}
}
