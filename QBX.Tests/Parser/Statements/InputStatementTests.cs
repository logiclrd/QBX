/*
using QBX.CodeModel.Expressions;

namespace QBX.Tests.Parser.Statements;

public class InputStatement
{
	public override StatementType Type => StatementType.Input;

	public bool EchoNewLine { get; set; } = true;
	public string? PromptString { get; set; }
	public bool PromptQuestionMark { get; set; } = true;
	public Expression? FileNumberExpression { get; set; }
	public List<string> Variables { get; } = new List<string>();

	public override void Render(TextWriter writer)
	{
		writer.Write("INPUT ");

		if (FileNumberExpression != null)
		{
			if (EchoNewLine || (PromptString != null))
				throw new Exception("Internal error: Mismatched configuration of InputStatement");

			writer.Write('#');
			FileNumberExpression.Render(writer);
		}
		else
		{
			if (!EchoNewLine)
				writer.Write("; ");

			if (PromptString != null)
			{
				writer.Write(PromptString);

				if (PromptQuestionMark)
					writer.Write("; ");
				else
					writer.Write(", ");
			}
		}

		for (int i = 0; i < Variables.Count; i++)
		{
			if (i > 0)
				writer.Write(", ");

			writer.Write(Variables[i]);
		}
	}
}

*/
