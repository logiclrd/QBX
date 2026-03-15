using System;
using System.IO;

using QBX.LexicalAnalysis;

namespace QBX.CodeModel;

public class VariableDeclaration : IRenderableCode
{
	public string Name { get; set; } = "";
	public VariableDeclarationSubscriptList? Subscripts { get; set; }
	public DataType Type { get; set; } = DataType.Unspecified;
	public string? UserType { get; set; }

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
