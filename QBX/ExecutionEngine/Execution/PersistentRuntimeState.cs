namespace QBX.ExecutionEngine.Execution;

public class PersistentRuntimeState(PlayProcessor playProcessor)
{
	public StringValue?[] SoftKeyMacros = new StringValue?[12];
	public PlayProcessor PlayProcessor = playProcessor;
}
