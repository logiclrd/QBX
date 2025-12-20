using QBX.LexicalAnalysis;

namespace QBX.CodeModel.Statements;

public class DeclareStatement : Statement
{
	public Token DeclarationType { get; set; }
	public string Name { get; set; }
	public TypeCharacter? TypeCharacter { get; set; }
	public ParameterList? Parameters;

	public DeclareStatement(Token declarationType, string name, ParameterList? parameters)
	{
		DeclarationType = declarationType;
		Name = name;
		Parameters = parameters;

		char lastChar = name.Last();

		if (TypeCharacter.TryParse(lastChar, out var typeCharacter))
		{
			TypeCharacter = typeCharacter;
			Name = Name.Remove(Name.Length - 1);
		}
	}

	public override void Render(TextWriter writer)
	{
		writer.Write("DECLARE {0} ", DeclarationType.Type);
		writer.Write(Name);
		TypeCharacter?.Render(writer);

		if (Parameters != null)
		{
			writer.Write(" (");
			Parameters.Render(writer);
			writer.Write(')');
		}
	}
}
