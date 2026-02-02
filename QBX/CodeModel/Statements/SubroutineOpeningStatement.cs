using QBX.LexicalAnalysis;

namespace QBX.CodeModel.Statements;

public abstract class SubroutineOpeningStatement : Statement
{
	public string Name { get; set; } = "";
	public ParameterList? Parameters { get; set; }
	public bool IsStatic { get; set; }

	public Token? NameToken;

	public override bool ExtraSpace => !IsStatic;

	protected abstract string StatementName { get; }
}
