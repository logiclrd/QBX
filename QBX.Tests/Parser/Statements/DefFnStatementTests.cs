/*
 * TODO
 * 
using QBX.CodeModel;
using QBX.CodeModel.Expressions;

namespace QBX.Tests.Parser.Statements;

public class DefFnStatement
{
	public override StatementType Type => StatementType.DefFn;

	public string Name { get; set; } = "FN";
	public ParameterList? Parameters { get; set; }
	public Expression? ExpressionBody { get; set; }

	public override void Render(TextWriter writer)
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

*/
