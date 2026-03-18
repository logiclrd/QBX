using System.IO;

using QBX.CodeModel.Expressions;
using QBX.Parser;

namespace QBX.CodeModel.Statements;

public class DefFnStatement : SubroutineOpeningStatement
{
	public override StatementType Type => StatementType.DefFn;

	protected override string StatementName => "DEF";

	static readonly Identifier UninitializedName = Identifier.Standalone("FN");

	public DefFnStatement()
	{
		Name = UninitializedName;
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
