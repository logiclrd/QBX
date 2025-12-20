using QBX.LexicalAnalysis;

namespace QBX.CodeModel;

public enum DataType
{
	[DataTypeCharacter('%')] [DataTypeToken(TokenType.INTEGER)]  INTEGER,
	[DataTypeCharacter('&')] [DataTypeToken(TokenType.LONG)]     LONG,
	[DataTypeCharacter('!')] [DataTypeToken(TokenType.SINGLE)]   SINGLE,
	[DataTypeCharacter('#')] [DataTypeToken(TokenType.DOUBLE)]   DOUBLE,
	[DataTypeCharacter('$')] [DataTypeToken(TokenType.STRING)]   STRING,
	[DataTypeCharacter('@')] [DataTypeToken(TokenType.CURRENCY)] CURRENCY,
}
