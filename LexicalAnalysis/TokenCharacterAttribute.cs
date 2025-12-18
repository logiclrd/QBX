namespace QBX.LexicalAnalysis;

[AttributeUsage(AttributeTargets.Field)]
public class TokenCharacterAttribute(char ch) : Attribute
{
	public char Character { get; } = ch;
}
