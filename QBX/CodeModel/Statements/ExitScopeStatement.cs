using System;
using System.IO;

namespace QBX.CodeModel.Statements;

public class ExitScopeStatement : Statement
{
	public override StatementType Type => StatementType.ExitScope;

	public ScopeType ScopeType { get; set; }

	protected override void RenderImplementation(TextWriter writer)
	{
		switch (ScopeType)
		{
			case ScopeType.Def:
			case ScopeType.Sub:
			case ScopeType.Function:
			{
				var compilationElement = this.CodeLine?.CompilationElement;

				if (compilationElement != null)
				{
					switch (compilationElement.Type)
					{
						case CompilationElementType.Main: ScopeType = ScopeType.Def; break;
						case CompilationElementType.Sub: ScopeType = ScopeType.Sub; break;
						case CompilationElementType.Function: ScopeType = ScopeType.Function; break;
					}
				}

				break;
			}
		}

		switch (ScopeType)
		{
			case ScopeType.Def: writer.Write("EXIT DEF"); break;
			case ScopeType.Sub: writer.Write("EXIT SUB"); break;
			case ScopeType.Function: writer.Write("EXIT FUNCTION"); break;
			case ScopeType.Do: writer.Write("EXIT DO"); break;
			case ScopeType.For: writer.Write("EXIT FOR"); break;

			default: throw new Exception("Internal error: Invalid ScopeType");
		}
	}
}
