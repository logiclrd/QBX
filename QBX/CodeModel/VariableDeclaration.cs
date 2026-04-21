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
	public int AsColumn { get; set; }
	public Identifier? FixedStringLength { get; set; }
	public Identifier? UserType { get; set; }

	public Token? NameToken;
	public Token? TypeToken;
	public Token? FixedStringLengthToken;

	public void Render(TextWriter writer)
	{
		var wrapper = new ColumnTrackingTextWriter(writer);

		wrapper.Write(Name);
		Subscripts?.Render(wrapper);

		if ((Type != DataType.Unspecified) && (UserType != null))
			throw new Exception("Internal error: VariableDeclaration specifies both Type and UserType");

		if (Type != DataType.Unspecified)
		{
			wrapper.Write(' ');
			while (wrapper.Column < AsColumn)
				wrapper.Write(' ');

			wrapper.Write("AS ");
			wrapper.Write(Type);

			if ((Type == DataType.STRING) && (FixedStringLength != null))
			{
				wrapper.Write(" * ");
				wrapper.Write(FixedStringLength.Value);
			}
		}
		else if (UserType != null)
		{
			wrapper.Write(' ');
			while (wrapper.Column < AsColumn)
				wrapper.Write(' ');

			wrapper.Write("AS ");
			wrapper.Write(UserType);
		}
	}
}
