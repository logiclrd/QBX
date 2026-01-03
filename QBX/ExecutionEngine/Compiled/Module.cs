namespace QBX.ExecutionEngine.Compiled;

public class Module
{
	public Routine? MainElement;
	public Dictionary<string, Routine> Elements = new Dictionary<string, Routine>();
}
