using System;
using System.IO;

namespace QBX.CodeModel.Statements;

public class BareLineNumberGoToStatement : GoToStatement
{
	protected override void RenderImplementation(TextWriter writer)
	{
		if (TargetLineNumber == null)
			throw new Exception("BareLineNumberGoToStatement with no TargetLineNumber");

		writer.Write(TargetLineNumber);
	}
}
