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

	public virtual bool HasSubscripts => false;
	public bool HasExplicitTypeClause => (Type != DataType.Unspecified) || (UserType != null);

	public Token? NameToken;
	public Token? TypeToken;
	public Token? FixedStringLengthToken;

	protected virtual void RenderLeadIn(TextWriter writer) { }

	protected abstract void RenderArraySubscripts(TextWriter writer);

	protected virtual bool RenderTypeClause(Action renderAs, TextWriter writer) => false;

	public virtual void Render(TextWriter writer)
	{
		var wrapper = new ColumnTrackingTextWriter(writer);

		RenderLeadIn(writer);

		wrapper.Write(Name);

		RenderArraySubscripts(wrapper);

		if ((Type != DataType.Unspecified) && (UserType != null))
			throw new Exception("Internal error: " + GetType().Name + " specifies both Type and UserType");

		void RenderAs()
		{
			wrapper.Write(' ');
			while (wrapper.Column < AsColumn)
				wrapper.Write(' ');

			wrapper.Write("AS ");
		}

		if (!RenderTypeClause(RenderAs, writer))
		{
			if (Type != DataType.Unspecified)
			{
				RenderAs();
				wrapper.Write(Type);

				if ((Type == DataType.STRING) && (FixedStringLength != null))
				{
					wrapper.Write(" * ");
					wrapper.Write(FixedStringLength.Value);
				}
			}
			else if (UserType != null)
			{
				RenderAs();
				wrapper.Write(UserType);
			}
		}
	}
}
