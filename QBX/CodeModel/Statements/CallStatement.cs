using System.IO;
using System.Linq;

using QBX.CodeModel.Expressions;
using QBX.LexicalAnalysis;

namespace QBX.CodeModel.Statements;

public class CallStatement : Statement
{
	public override StatementType Type => StatementType.Call;

	public CallStatementType CallStatementType { get; set; }
	public string TargetName { get; set; }
	public ExpressionList? Arguments { get; set; }

	public Token? TargetNameToken { get; set; }

	public CallStatement(CallStatementType type, string targetName, ExpressionList? arguments)
	{
		CallStatementType = type;
		TargetName = targetName;
		Arguments = arguments;
	}

	protected override void RenderImplementation(TextWriter writer)
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
