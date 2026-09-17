using System;
using System.IO;

namespace QBX.CodeModel;

public class ParameterDefinition : VariableDeclarationBase, IRenderableCode
{
	public ParameterRepresentation Representation { get; set; }
	public bool IsArray { get; set; }
	public bool AnyType { get; set; }

	public override bool HasSubscripts => IsArray;

	protected override void RenderLeadIn(TextWriter writer)
	{
		switch (Representation)
		{
			case ParameterRepresentation.BYVAL: writer.Write("BYVAL "); break;
			case ParameterRepresentation.SEG: writer.Write("SEG "); break;
		}
	}

	protected override void RenderArraySubscripts(TextWriter writer)
	{
		if (IsArray)
			writer.Write("()");
	}

	protected override bool RenderTypeClause(Action renderAs, TextWriter writer)
	{
		if (AnyType)
		{
			renderAs();
			writer.Write("ANY");

			return true;
		}

		return false;
	}

	public override void Render(TextWriter writer)
	{
		AsColumn = 0;

		base.Render(writer);
	}
}
