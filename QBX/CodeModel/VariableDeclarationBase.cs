using System;
using System.IO;

using QBX.LexicalAnalysis;
using QBX.Parser;

namespace QBX.CodeModel;

public abstract class VariableDeclarationBase : IRenderableCode
{
	public Identifier Name { get; set; } = Identifier.Empty;
	public DataType Type { get; set; } = DataType.Unspecified;
	public int AsColumn { get; set; }
	public Identifier? FixedStringLength { get; set; }
	public Identifier? UserType { get; set; }

	public bool HasExplicitTypeClause => (Type != DataType.Unspecified) || (UserType != null);

	public Token? NameToken;
	public Token? TypeToken;
	public Token? FixedStringLengthToken;

	protected abstract void RenderArraySubscripts(TextWriter writer);

	public void Render(TextWriter writer)
	{
		var wrapper = new ColumnTrackingTextWriter(writer);

		wrapper.Write(Name);

		RenderArraySubscripts(wrapper);

		if ((Type != DataType.Unspecified) && (UserType != null))
			throw new Exception("Internal error: " + GetType().Name + " specifies both Type and UserType");

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
