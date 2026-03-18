using QBX.LexicalAnalysis;
using QBX.Parser;

namespace QBX.ExecutionEngine.Compiled;

public class VariableName(string name, Token? nameToken, int variableIndex, bool isLinked)
{
	public string Name => name;
	public Token? NameToken => nameToken;
	public int VariableIndex => variableIndex;
	public bool IsLinked => isLinked;
}
