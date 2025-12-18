namespace QBX.CodeModel;

public abstract class Statement : IRenderableCode
{
	public StatementType Type { get; set; } = default;
	public virtual bool ExtraSpace => false;

	public abstract void Render(TextWriter writer);
}
