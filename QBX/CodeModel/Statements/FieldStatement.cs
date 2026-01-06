using System;
using System.Collections.Generic;
using System.IO;

using QBX.CodeModel.Expressions;

namespace QBX.CodeModel.Statements;

public class FieldStatement : Statement
{
	public override StatementType Type => StatementType.Field;

	public Expression? FileNumberExpression { get; set; }
	public List<FieldDefinition> FieldDefinitions { get; } = new List<FieldDefinition>();

	protected override void RenderImplementation(TextWriter writer)
	{
		if (FileNumberExpression == null)
			throw new Exception("Internal error: FieldStatement with no FileNumberExpression");
		if (FieldDefinitions.Count == 0)
			throw new Exception("Internal error: FieldStatement with no field definitions");

		writer.Write("FIELD #");
		FileNumberExpression.Render(writer);

		for (int i = 0; i < FieldDefinitions.Count; i++)
		{
			writer.Write(", ");
			FieldDefinitions[i].Render(writer);
		}
	}
}
