using System.IO;

using QBX.LexicalAnalysis;

namespace QBX.CodeModel.Expressions;

public class CallOrIndexExpression : Expression
{
	public Expression Subject { get; set; }
	public ExpressionList Arguments { get; set; }

	public override bool IsValidAssignmentTarget() => true;
	public override bool IsValidMemberSubject() => true;

	public CallOrIndexExpression(Token token, Expression subject, ExpressionList arguments)
	{
		Token = token;

		Subject = subject;
		Arguments = arguments;
	}

	public override void Render(TextWriter writer)
	{
		Subject.Render(writer);
		writer.Write('(');
		Arguments.Render(writer);
		writer.Write(')');
	}
}
