using System;
using System.IO;

using QBX.LexicalAnalysis;

namespace QBX.CodeModel.Expressions;

public class IdentifierExpression : Expression
{
	public string Identifier { get; set; }

	public override bool IsValidAssignmentTarget() => true;
	public override bool IsValidIndexSubject() => true;
	public override bool IsValidFieldSubject() => true;

	public IdentifierExpression(Token token)
	{
		Token = token;
		Identifier = token.Value ?? throw new Exception("Internal error: Identifier token has no value");
	}

	public override void Render(TextWriter writer)
	{
		writer.Write(Identifier);
	}
}
