using System;

namespace QBX.LexicalAnalysis;

public class KeywordFunctionAttribute(bool parameters = true, bool noParameters = false) : Attribute
{
	public bool TakesParameters => parameters;
	public bool TakesNoParameters => noParameters || !parameters;
}
