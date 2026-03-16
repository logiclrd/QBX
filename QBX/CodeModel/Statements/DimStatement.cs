using System.Collections.Generic;
using System.IO;

namespace QBX.CodeModel.Statements;

public class DimStatement : Statement
{
	public override StatementType Type => StatementType.Dim;

	public bool Shared { get; set; }
	public IReadOnlyList<VariableDeclaration> Declarations => _declarations;

	List<VariableDeclaration> _declarations = new List<VariableDeclaration>();

	public virtual bool AlwaysDeclareArrays => true;
	public virtual bool DeclareScalars => true;

	public virtual void AddDeclaration(VariableDeclaration declaration)
	{
		_declarations.Add(declaration);
	}

	protected virtual string StatementName => "DIM";

	protected virtual void RenderPreserveFlag(TextWriter writer)
	{
		// Empty base implementation for DIM.
	}

	protected virtual void RenderBlockName(TextWriter writer)
	{
		// Empty base implementation for DIM.
	}

	protected override void RenderImplementation(TextWriter writer)
	{
		writer.Write(StatementName);

		RenderPreserveFlag(writer);

		if (Shared)
			writer.Write(" SHARED");

		RenderBlockName(writer);

		writer.Write(' ');

		for (int i = 0; i < Declarations.Count; i++)
		{
			if (i > 0)
				writer.Write(", ");

			Declarations[i].Render(writer);
		}
	}
}
