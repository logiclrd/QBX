using QBX.CodeModel.Expressions;

namespace QBX.CodeModel.Statements;

public class UnresolvedWidthStatement : Statement
{
	// Could be WIDTH screenwidth%, screenheight%
	//       or WIDTH device$, linewidth%
	// Needs to be replaced with either DeviceWidthStatement or ScreenWidthStatement
	// based on the data type to which Expression1 resolves.

	public override StatementType Type => StatementType.UnresolvedWidth;

	public Expression? Expression1 { get; set; }
	public Expression? Expression2 { get; set; }

	public override void Render(TextWriter writer)
	{
		if ((Expression1 == null) || (Expression2 == null))
			throw new Exception("Internal error: UnresolvedWidthStatement with a missing Width or Height expression");

		writer.Write("WIDTH ");
		Expression1.Render(writer);
		writer.Write(", ");
		Expression2.Render(writer);
	}
}
