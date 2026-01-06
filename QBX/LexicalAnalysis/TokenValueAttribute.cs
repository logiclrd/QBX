using System;

namespace QBX.LexicalAnalysis;

public class TokenValueAttribute(string value) : Attribute
{
	public string Value => value;
}
