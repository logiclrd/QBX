namespace QBX.CodeModel;

public enum DataType
{
	[DataTypeCharacter('%')] INTEGER,
	[DataTypeCharacter('&')] LONG,
	[DataTypeCharacter('!')] SINGLE,
	[DataTypeCharacter('#')] DOUBLE,
	[DataTypeCharacter('$')] STRING,
	[DataTypeCharacter('@')] CURRENCY,
}
