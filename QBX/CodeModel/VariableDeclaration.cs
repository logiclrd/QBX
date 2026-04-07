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
	public Identifier? FixedStringLength { get; set; }
	public Identifier? UserType { get; set; }

	public Token? NameToken;
	public Token? TypeToken;
	public Token? FixedStringLengthToken;

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

			if ((Type == DataType.STRING) && (FixedStringLength != null))
			{
				writer.Write(" * ");
				writer.Write(FixedStringLength.Value);
			}
		}
		else if (UserType != null)
		{
			writer.Write(" AS ");
			writer.Write(UserType);
		}
	}
}
