using System.IO;

using QBX.CodeModel.Expressions;

namespace QBX.CodeModel.Statements;

public class DefFnStatement : Statement
{
	public override StatementType Type => StatementType.DefFn;

	public string Name { get; set; } = "FN";
	public ParameterList? Parameters { get; set; }
	public Expression? ExpressionBody { get; set; }

	protected override void RenderImplementation(TextWriter writer)
	{
		writer.Write("DEF {0}", Name);

		if (Parameters != null)
		{
			writer.Write(" (");
			Parameters.Render(writer);
			writer.Write(')');
		}

		if (ExpressionBody != null)
		{
			writer.Write(" = ");
			ExpressionBody.Render(writer);
		}
	}
}
