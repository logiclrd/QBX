using System.Diagnostics.CodeAnalysis;
using System.IO;

using QBX.CodeModel.Expressions;

namespace QBX.CodeModel.Statements;

public class WindowStatement : Statement
{
	public override StatementType Type => StatementType.Window;

	public bool UseScreenCoordinates { get; set; }

	public Expression? X1Expression { get; set; }
	public Expression? Y1Expression { get; set; }
	public Expression? X2Expression { get; set; }
	public Expression? Y2Expression { get; set; }

	[MemberNotNullWhen(false, nameof(X1Expression))]
	[MemberNotNullWhen(false, nameof(Y1Expression))]
	[MemberNotNullWhen(false, nameof(X2Expression))]
	[MemberNotNullWhen(false, nameof(Y2Expression))]
	public bool IsEmpty
	{
		get
		{
			return
				(X1Expression == null) &&
				(Y1Expression == null) &&
				(X2Expression == null) &&
				(Y2Expression == null);
		}
	}

	protected override void RenderImplementation(TextWriter writer)
	{
		writer.Write("WINDOW");

		if (!IsEmpty)
		{
			if (UseScreenCoordinates)
				writer.Write(" SCREEN");

			writer.Write(" (");
			X1Expression.Render(writer);
			writer.Write(", ");
			Y1Expression.Render(writer);
			writer.Write(")-(");
			X2Expression.Render(writer);
			writer.Write(", ");
			Y2Expression.Render(writer);
			writer.Write(')');
		}
	}
}
