using System.IO;

using QBX.CodeModel.Statements;
using QBX.LexicalAnalysis;

namespace QBX.CodeModel.Expressions;

public abstract class Expression : IRenderableCode
{
	public Token? Token { get; set; }
	public bool IsFileNumberArgument { get; set; }

	public virtual bool IsValidAssignmentTarget() => false;
	public virtual bool IsValidIndexSubject() => false;
	public virtual bool IsValidFieldSubject() => false;

	public virtual Expression ClaimTokens(Statement owner)
	{
		Token?.OwnerStatement = owner;
		return this;
	}

	public void Render(TextWriter writer)
	{
		if (IsFileNumberArgument)
			writer.Write('#');

		RenderImplementation(writer);
	}

	protected abstract void RenderImplementation(TextWriter writer);
}
