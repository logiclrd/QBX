using QBX.LexicalAnalysis;

namespace QBX.DevelopmentEnvironment;

public class ErrorInfo
{
	public string Message = "";
	public int? Number;
	public Token? Context;
	public ErrorSource Source;
}
