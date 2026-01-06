using System;

using QBX.LexicalAnalysis;

namespace QBX.CodeModel;

[AttributeUsage(AttributeTargets.Field)]
public class DataTypeTokenAttribute(TokenType tokenType) : Attribute
{
	public TokenType TokenType => tokenType;
}
