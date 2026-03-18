using QBX.Parser;

namespace QBX.ExecutionEngine.Compiled;

public class CommonVariableLinkGroup(Identifier commonBlockName, VariableLink[] links)
{
	public Identifier CommonBlockName => commonBlockName;
	public VariableLink[] LinkedVariables => links;
}
