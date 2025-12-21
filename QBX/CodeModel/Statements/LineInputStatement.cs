using QBX.CodeModel.Expressions;
using QBX.LexicalAnalysis;

namespace QBX.CodeModel.Statements;

public class LineInputStatement : Statement
{
	public override StatementType Type => StatementType.LineInput;

	public Expression? FileNumberExpression { get; set;  }
	public bool EchoNewLine { get; set; }
	public string? PromptString { get; set; }
	public string? Variable { get; set; }

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
			if (EchoNewLine)
				writer.Write("; ");

			if (PromptString != null)
			{
				writer.Write(PromptString);
				writer.Write("; ");
			}

			if (Variable == null)
				throw new Exception("Internal error: LineInputStatement with no variable");

			writer.Write(Variable);
		}
	}
}
