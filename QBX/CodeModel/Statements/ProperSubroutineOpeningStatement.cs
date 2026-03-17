using System;
using System.IO;

namespace QBX.CodeModel.Statements;

public abstract class ProperSubroutineOpeningStatement : SubroutineOpeningStatement
{
	public ScopeType ScopeType =>
		Type switch
		{
			StatementType.Sub => ScopeType.Sub,
			StatementType.Function => ScopeType.Function,

			_ => throw new Exception("Internal error: Proper subroutine is neither a Sub nor a Function")
		};

	protected override void RenderImplementation(TextWriter writer)
	{
		writer.Write(StatementName);
		writer.Write(' ');
		writer.Write(Name);

		Parameters?.Render(writer);

		if (IsStatic)
			writer.Write(" STATIC");
	}
}
