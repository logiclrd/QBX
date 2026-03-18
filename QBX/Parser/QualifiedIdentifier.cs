using QBX.CodeModel;

namespace QBX.Parser;

public class QualifiedIdentifier(Identifier identifier, TypeCharacter typeCharacter) : Identifier(identifier.Value, identifier.Repository)
{
	public TypeCharacter TypeCharacter => typeCharacter;

	public Identifier UnqualifiedIdentifier => identifier;

	public override string Value => ToString();

	public override string ToString()
	{
		return identifier.ToString() + typeCharacter.Character;
	}
}
