namespace QBX.ExecutionEngine.Execution;

public static class StringValueExtensions
{
	public static StringValue ToStringValue(this string str)
		=> new StringValue(str);
}
