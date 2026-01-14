using System.IO;

using QBX.CodeModel.Expressions;

namespace QBX.CodeModel.Statements;

public class DefFnStatement : SubroutineOpeningStatement
{
	public override StatementType Type => StatementType.DefFn;

	protected override string StatementName => "DEF";

	public DefFnStatement()
	{
		Name = "FN";
	}

	public Expression? ExpressionBody { get; set; }

	protected override void RenderImplementation(TextWriter writer)
	{
		writer.Write("DEF {0}", Name);

		Parameters?.Render(writer);

		if (ExpressionBody != null)
		{
			writer.Write(" = ");
			ExpressionBody.Render(writer);
		}
	}
}
