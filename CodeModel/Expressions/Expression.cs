namespace QBX.CodeModel.Expressions;

public abstract class Expression : IRenderableCode
{
	public abstract void Render(TextWriter writer);
}
