using QBX.CodeModel.Expressions;

namespace QBX.CodeModel.Statements;

public class CallStatement : Statement
{
	public override StatementType Type => StatementType.Call;

	public CallStatementType CallStatementType { get; set; }
	public string TargetName { get; set; }
	public ExpressionList? Arguments { get; set; }

	public CallStatement(CallStatementType type, string targetName, ExpressionList? arguments)
	{
		CallStatementType = type;
		TargetName = targetName;
		Arguments = arguments;
	}

	public override void Render(TextWriter writer)
	{
		switch (CallStatementType)
		{
			case CallStatementType.Explicit:
				writer.Write("CALL {0}", TargetName);

				if (Arguments != null && Arguments.Expressions.Any())
				{
					writer.Write('(');
					Arguments.Render(writer);
					writer.Write(')');
				}

				break;
			case CallStatementType.Implicit:
				writer.Write(TargetName);

				if (Arguments != null && Arguments.Expressions.Any())
				{
					writer.Write(' ');
					Arguments.Render(writer);
				}

				break;
		}
	}
}
