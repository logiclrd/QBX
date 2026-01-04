namespace QBX.ExecutionEngine.Compiled;

public class Module
{
	public Routine? MainRoutine;
	public Dictionary<string, Routine> Routines = new Dictionary<string, Routine>();
}
