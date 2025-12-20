namespace QBX.CodeModel.Statements;

public abstract class Statement : IRenderableCode
{
	public abstract StatementType Type { get; }
	public virtual bool ExtraSpace => false;

	public abstract void Render(TextWriter writer);
}
