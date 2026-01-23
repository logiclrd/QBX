using System;
using System.IO;

using QBX.CodeModel.Expressions;

namespace QBX.CodeModel.Statements;

public class LineInputStatement : Statement
{
	public override StatementType Type => StatementType.LineInput;

	public Expression? FileNumberExpression { get; set;  }
	public bool EchoNewLine { get; set; } = true;
	public string? PromptString { get; set; }
	public char PromptStringSeparatorCharacter { get; set; } = ',';
	public Expression? TargetExpression { get; set; }

	protected override void RenderImplementation(TextWriter writer)
	{
		writer.Write("LINE INPUT ");

		if (FileNumberExpression != null)
		{
			if (EchoNewLine || (PromptString != null))
				throw new Exception("Internal error: mismatched configuration of LineInputStatement");

			writer.Write('#');
			FileNumberExpression.Render(writer);
			writer.Write(", ");
		}
		else
		{
			if (!EchoNewLine)
				writer.Write("; ");

			if (PromptString != null)
			{
				writer.Write('"');
				writer.Write(PromptString);
				writer.Write('"');
				writer.Write(PromptStringSeparatorCharacter);
				writer.Write(' ');
			}

			if (TargetExpression == null)
				throw new Exception("Internal error: LineInputStatement with no TargetExpression");

			TargetExpression.Render(writer);
		}
	}
}
