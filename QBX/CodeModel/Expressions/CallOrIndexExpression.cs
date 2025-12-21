using QBX.LexicalAnalysis;

namespace QBX.CodeModel.Expressions;

public class CallOrIndexExpression : Expression
{
	public string Name { get; set; }
	public ExpressionList Arguments { get; set; }

	public CallOrIndexExpression(Token nameToken, ExpressionList arguments)
	{
		Token = nameToken;

		Name = nameToken.Value ?? throw new Exception("Internal error: token missing value");
		Arguments = arguments;
	}

	public override void Render(TextWriter writer)
	{
		writer.Write(Name);
		writer.Write('(');
		Arguments.Render(writer);
		writer.Write(')');
	}
}
