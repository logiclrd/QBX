
namespace QBX.LexicalAnalysis;

public class KeywordFunctionAttribute(bool parameters = true) : Attribute
{
	public bool TakesParameters => parameters;
}
