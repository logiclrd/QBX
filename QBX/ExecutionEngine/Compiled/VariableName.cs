using QBX.LexicalAnalysis;
using QBX.Parser;

namespace QBX.ExecutionEngine.Compiled;

public class VariableName(Identifier name, Token? nameToken, int variableIndex, bool isLinked)
{
	public Identifier Name => name;
	public Token? NameToken => nameToken;
	public int VariableIndex => variableIndex;
	public bool IsLinked => isLinked;
}
