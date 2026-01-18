using System;
using System.IO;

using QBX.CodeModel.Expressions;

namespace QBX.CodeModel.Statements;

public class SoundStatement : Statement
{
	public override StatementType Type => StatementType.Sound;

	public Expression? FrequencyExpression { get; set; }
	public Expression? DurationExpression { get; set; }

	protected override void RenderImplementation(TextWriter writer)
	{
		if ((FrequencyExpression == null) || (DurationExpression == null))
			throw new Exception("Internal error: SoundStatement with a missing Frequency or Duration expression");

		writer.Write("SOUND ");
		FrequencyExpression.Render(writer);
		writer.Write(", ");
		DurationExpression.Render(writer);
	}
}
