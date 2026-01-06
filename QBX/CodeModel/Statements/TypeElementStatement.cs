using System;
using System.IO;

using QBX.LexicalAnalysis;

namespace QBX.CodeModel.Statements;

public class TypeElementStatement : Statement
{
	public override StatementType Type => StatementType.TypeElement;

	public string Name { get; set; } = "";
	public VariableDeclarationSubscriptList? Subscripts { get; set; }
	public DataType ElementType { get; set; }
	public string? ElementUserType { get; set; }
	public string? AlignmentWhitespace { get; set; }

	public Token? TypeToken { get; set; }

	protected override void RenderImplementation(TextWriter writer)
	{
		writer.Write(Name);

		if (Subscripts != null)
		{
			if (Subscripts.Subscripts.Count == 0)
				throw new Exception("Internal error: TypeElementStatement with an empty (non-null) Subscripts collection");

			Subscripts.Render(writer);
		}

		if (AlignmentWhitespace != null)
			writer.Write(AlignmentWhitespace);
		else
			writer.Write(' ');

		writer.Write("AS ");

		if ((ElementType != DataType.Unspecified) && (ElementUserType != null))
			throw new Exception("Internal error: TypeElementStatement specifies both built-in and user-defined type");

		if (ElementType != DataType.Unspecified)
			writer.Write(ElementType);
		else if (ElementUserType != null)
			writer.Write(ElementUserType);
		else
			throw new Exception("Internal error: TypeElementStatement with no type");
	}
}
