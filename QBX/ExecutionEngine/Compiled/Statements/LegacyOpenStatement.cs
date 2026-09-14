using System;
using System.Collections.Generic;
using System.Linq;

using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;
using QBX.OperatingSystem.FileStructures;

using OSOpenMode = QBX.OperatingSystem.FileStructures.OpenMode;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class LegacyOpenStatement(CodeModel.Statements.LegacyOpenStatement source) : OpenStatementBase(source)
{
	public Evaluable? ModeExpression;
	public Evaluable? FileNumberExpression;
	public Evaluable? FileNameExpression;
	public Evaluable? RecordLengthExpression;

	protected override void ExecuteImplementation(ExecutionContext context, StackFrame stackFrame)
	{
		if (ModeExpression == null)
			throw new Exception("OpenStatement with no ModeExpression");
		if (FileNameExpression == null)
			throw new Exception("OpenStatement with no FileNameExpression");
		if (FileNumberExpression == null)
			throw new Exception("OpenStatement with no FileNumberExpression");

		var mode2Value = (StringVariable)ModeExpression.Evaluate(context, stackFrame);

		string mode2 = mode2Value.ToString();

		if (mode2.Length != 1)
			throw RuntimeException.BadFileMode(Source);

		OpenMode openMode;

		switch (char.ToUpperInvariant(mode2[0]))
		{
			case 'O': openMode = OpenMode.Output; break;
			case 'I': openMode = OpenMode.Input; break;
			case 'R': openMode = OpenMode.Random; break;
			case 'B': openMode = OpenMode.Binary; break;
			case 'A': openMode = OpenMode.Append; break;

			default:
				throw RuntimeException.BadFileMode(Source);
		}

		int fileNumber = FileNumberExpression.EvaluateAndCoerceToInt(context, stackFrame);

		if (context.Files.ContainsKey(fileNumber))
			throw RuntimeException.FileAlreadyOpen(Source);

		var fileName = (StringVariable)FileNameExpression.Evaluate(context, stackFrame);

		if ((openMode == OpenMode.Output)
		 && (openMode == OpenMode.Append))
		{
			if (context.Machine.DOS.FileIsOpenAsOneOf(fileName.Value, context.Files.Values.Select(openFile => openFile.FileHandle)))
				throw RuntimeException.FileAlreadyOpen(Source);
		}

		int? recordLength = RecordLengthExpression?.EvaluateAndCoerceToInt(context, stackFrame);

		var openFile = new OpenFile();

		var fileMode =
			openMode switch
			{
				OpenMode.Random or OpenMode.Binary or OpenMode.Append => FileMode.OpenOrCreate,
				OpenMode.Input => FileMode.Open,
				OpenMode.Output => FileMode.Create,

				_ => throw new Exception($"Unrecognized open mode \"{mode2}\" ({openMode})")
			};

		IEnumerable<OSOpenMode> attemptAccessModes;

		switch (openMode)
		{
			case OpenMode.Input: attemptAccessModes = Attempt_Read; break;
			case OpenMode.Output: attemptAccessModes = Attempt_Write; break;
			case OpenMode.Append: attemptAccessModes = Attempt_ReadWrite_Write; break;
			case OpenMode.Random: attemptAccessModes = Attempt_ReadWrite_Write_Read; break;
			case OpenMode.Binary: attemptAccessModes = Attempt_ReadWrite_Write_Read; break;

			default: throw RuntimeException.IllegalFunctionCall(Source);
		}

		PerformOpen(
			fileName.ToString(),
			openMode,
			fileMode,
			shareMode: OSOpenMode.Share_Compatibility,
			attemptAccessModes,
			recordLength,
			fileNumber,
			context);
	}
}
