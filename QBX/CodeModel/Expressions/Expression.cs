using System.IO;

using QBX.CodeModel.Statements;
using QBX.LexicalAnalysis;

namespace QBX.CodeModel.Expressions;

public abstract class Expression : IRenderableCode
{
	public Token? Token { get; set; }

	public virtual bool IsValidAssignmentTarget() => false;
	public virtual bool IsValidIndexSubject() => false;
	public virtual bool IsValidFieldSubject() => false;

	public virtual Expression ClaimTokens(Statement owner)
	{
		Token?.OwnerStatement = owner;
		return this;
	}

	public abstract void Render(TextWriter writer);
}
