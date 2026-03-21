using System;
using System.IO;

using QBX.CodeModel.Expressions;

namespace QBX.CodeModel.Statements;

public class ChDriveStatement : Statement
{
	public override StatementType Type => StatementType.ChDrive;

	public Expression? DriveLetterExpression { get; set; }

	protected override void RenderImplementation(TextWriter writer)
	{
		if (DriveLetterExpression == null)
			throw new Exception("ChDriveStatement with no DriveLetterExpression");

		writer.Write("CHDRIVE ");
		DriveLetterExpression.Render(writer);
	}
}
