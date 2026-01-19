using System;
using System.IO;

namespace QBX.CodeModel.Statements;

public class OnErrorStatement : Statement
{
	public override StatementType Type => StatementType.OnError;

	public bool LocalHandler { get; set; }
	public OnErrorAction Action { get; set; }
	public string? TargetLineNumber;
	public string? TargetLabel;

	protected override void RenderImplementation(TextWriter writer)
	{
		writer.Write("ON ");

		if (LocalHandler)
			writer.Write("LOCAL ");

		writer.Write("ERROR ");

		switch (Action)
		{
			case OnErrorAction.DoNotHandle: writer.Write("GOTO 0"); break;
			case OnErrorAction.ResumeNext: writer.Write("RESUME NEXT"); break;
			case OnErrorAction.GoToHandler:
				if ((TargetLineNumber == null) && (TargetLabel == null))
					throw new Exception("Internal error: OnErrorStatement specifies action GoToHandler but has no target line");
				if ((TargetLineNumber != null) && (TargetLabel != null))
					throw new Exception($"Internal error: OnErrorStatement with both line number and label");

				writer.Write("GOTO ");

				if (TargetLineNumber != null)
					writer.Write(TargetLineNumber);
				else
					writer.Write(TargetLabel);

				break;
		}
	}
}
