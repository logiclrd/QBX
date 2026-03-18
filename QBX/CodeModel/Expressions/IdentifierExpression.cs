using System;
using System.IO;

using QBX.LexicalAnalysis;
using QBX.Parser;

namespace QBX.CodeModel.Expressions;

public class IdentifierExpression : Expression
{
	public Identifier Identifier { get; set; }

	public override bool IsValidAssignmentTarget() => true;
	public override bool IsValidIndexSubject() => true;
	public override bool IsValidFieldSubject() => true;

	public IdentifierExpression(Token token, Identifier identifier)
	{
		Token = token;
		Identifier = identifier;
	}

	protected override void RenderImplementation(TextWriter writer)
	{
		writer.Write(Identifier);
	}
}
