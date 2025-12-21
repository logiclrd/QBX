using QBX.CodeModel.Expressions;

namespace QBX.CodeModel.Statements;

public class DoStatement : Statement
{
	public override StatementType Type => StatementType.Do;

	public DoConditionType ConditionType { get; set; }
	public Expression? Expression { get; set; }

	protected virtual void RenderStatementName(TextWriter writer)
		=> writer.Write("DO");

	protected override void RenderImplementation(TextWriter writer)
	{
		RenderStatementName(writer);

		switch (ConditionType)
		{
			case DoConditionType.None:
				if (Expression != null)
					throw new Exception("Internal error: Expression supplied on empty DO/LOOP");

				return;

			case DoConditionType.While: writer.Write(" WHILE "); break;
			case DoConditionType.Until: writer.Write(" UNTIL "); break;
		}

		if (Expression == null)
			throw new Exception("Internal error: No expression supplied on " + Type.ToString().ToUpperInvariant() + " " + ConditionType.ToString().ToUpperInvariant());

		Expression.Render(writer);
	}
}
