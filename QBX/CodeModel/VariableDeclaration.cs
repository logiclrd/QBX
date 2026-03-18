using System;
using System.IO;

using QBX.LexicalAnalysis;
using QBX.Parser;

namespace QBX.CodeModel;

public class VariableDeclaration : IRenderableCode
{
	public Identifier Name { get; set; } = Identifier.Empty;
	public VariableDeclarationSubscriptList? Subscripts { get; set; }
	public DataType Type { get; set; } = DataType.Unspecified;
	public Identifier? UserType { get; set; }

	public Token? NameToken;
	public Token? TypeToken;

	public void Render(TextWriter writer)
	{
		writer.Write(Name);
		Subscripts?.Render(writer);

		if ((Type != DataType.Unspecified) && (UserType != null))
			throw new Exception("Internal error: VariableDeclaration specifies both Type and UserType");

		if (Type != DataType.Unspecified)
		{
			writer.Write(" AS ");
			writer.Write(Type);
		}
		else if (UserType != null)
		{
			writer.Write(" AS ");
			writer.Write(UserType);
		}
	}
}
