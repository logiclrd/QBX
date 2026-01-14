using System.IO;

using QBX.LexicalAnalysis;

namespace QBX.CodeModel.Statements;

public abstract class Statement : IRenderableCode
{
	public CodeLine? CodeLine { get; set; }

	public abstract StatementType Type { get; }
	public string Indentation { get; set; } = "";

	public virtual bool ExtraSpace => false;

	public Token? FirstToken { get; set; }
	public int SourceLength { get; set; }

	public void Render(TextWriter writer)
	{
		writer.Write(Indentation);
		RenderImplementation(writer);
	}

	protected abstract void RenderImplementation(TextWriter writer);
}
