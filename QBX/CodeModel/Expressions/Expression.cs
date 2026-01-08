using System.IO;

using QBX.LexicalAnalysis;

namespace QBX.CodeModel.Expressions;

public abstract class Expression : IRenderableCode
{
	public Token? Token { get; set; }

	public virtual bool IsValidAssignmentTarget() => false;
	public virtual bool IsValidIndexSubject() => false;
	public virtual bool IsValidFieldSubject() => false;

	public abstract void Render(TextWriter writer);
}
