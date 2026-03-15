namespace QBX.ExecutionEngine.Compiled;

public class CommonVariableLinkGroup(string commonBlockName, VariableLink[] links)
{
	public string CommonBlockName => commonBlockName;
	public VariableLink[] LinkedVariables => links;
}
