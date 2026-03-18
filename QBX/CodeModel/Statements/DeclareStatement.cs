using System.IO;
using System.Linq;

using QBX.LexicalAnalysis;
using QBX.Parser;

namespace QBX.CodeModel.Statements;

public class DeclareStatement : Statement
{
	public override StatementType Type => StatementType.Declare;

	public Token DeclarationType { get; set; }
	public Identifier Name { get; set; }
	public bool IsCDecl { get; set; }
	public string? Alias { get; set; }
	public TypeCharacter? TypeCharacter { get; set; }
	public ParameterList? Parameters;

	public Token? NameToken;

	public DeclareStatement(Token declarationType, Identifier name, Token? nameToken, ParameterList? parameters)
	{
		DeclarationType = declarationType;
		Name = name;
		NameToken = nameToken;
		Parameters = parameters;

		char lastChar = name.Value.Last();

		if (name is QualifiedIdentifier qualifiedName)
		{
			TypeCharacter = qualifiedName.TypeCharacter;
			Name = qualifiedName.UnqualifiedIdentifier;
		}
	}

	protected override void RenderImplementation(TextWriter writer)
	{
		writer.Write("DECLARE {0} ", DeclarationType.Type);
		writer.Write(Name);
		TypeCharacter?.Render(writer);

		if (IsCDecl)
			writer.Write(" CDECL");

		if (Alias != null)
		{
			writer.Write(" ALIAS \"");
			writer.Write(Alias);
			writer.Write('"');
		}

		Parameters?.Render(writer);
	}
}
