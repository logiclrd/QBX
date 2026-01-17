using System;
using System.Collections.Generic;
using System.IO;

using QBX.ExecutionEngine;

namespace QBX.CodeModel.Statements;

public class DataStatement : Statement
{
	public override StatementType Type => StatementType.Data;

	public string? RawString { get; set; }

	public DataStatement(string dataString)
	{
		RawString = dataString;
	}

	protected override void RenderImplementation(TextWriter writer)
	{
		writer.Write("DATA");

		writer.Write(RawString);
	}
}
