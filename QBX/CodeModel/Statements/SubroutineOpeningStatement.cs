using QBX.LexicalAnalysis;
using QBX.Parser;

namespace QBX.CodeModel.Statements;

public abstract class SubroutineOpeningStatement : Statement
{
	public Identifier Name { get; set; } = Identifier.Empty;
	public ParameterList? Parameters { get; set; }
	public bool IsStatic { get; set; }

	public Token? NameToken;

	public override bool ExtraSpace => !IsStatic;

	protected abstract string StatementName { get; }
}
