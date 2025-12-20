using QBX.LexicalAnalysis;

namespace QBX.CodeModel.Expressions;

public abstract class Expression : IRenderableCode
{
	public Token? Token { get; set; }

	public abstract void Render(TextWriter writer);
}
