using System;
using System.IO;

using QBX.LexicalAnalysis;
using QBX.Parser;

namespace QBX.CodeModel.Statements;

public class VariableScopeDeclaration : IRenderableCode
{
	public Identifier Name { get; set; } = Identifier.Empty;
	public bool IsArray { get; set; }
	public DataType? Type { get; set; }
	public Identifier? UserType { get; set; }

	public Token? NameToken;
	public Token? TypeToken;

	public void Render(TextWriter writer)
	{
		writer.Write(Name);

		if (IsArray)
			writer.Write("()");

		if (Type.HasValue && (UserType != null))
			throw new Exception("Internal error: VariableScopeDeclaration specifies both Type and UserTYpe");

		if (Type.HasValue)
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
