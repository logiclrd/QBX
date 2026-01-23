using System.Collections.Generic;
using System.IO;
using System.Linq;

using QBX.LexicalAnalysis;

namespace QBX.CodeModel.Statements;

public abstract class Statement : IRenderableCode
{
	public CodeLine? CodeLine { get; set; }

	public abstract StatementType Type { get; }
	public string Indentation { get; set; } = "";

	public virtual bool ExtraSpace => false;

	public Token? FirstToken { get; set; }
	public int SourceColumn { get; set; }
	public int SourceLength { get; set; }

	public Statement? TrueSource { get; set; }
	public bool IsBreakpoint;

	public int LineNumberForErrorReporting { get; set; }

	public virtual IEnumerable<Statement> Substatements => Enumerable.Empty<Statement>();

	public void Render(TextWriter writer)
	{
		writer.Write(Indentation);
		RenderImplementation(writer);
	}

	protected abstract void RenderImplementation(TextWriter writer);
}
