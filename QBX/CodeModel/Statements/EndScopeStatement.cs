using System;
using System.IO;

namespace QBX.CodeModel.Statements;

public class EndScopeStatement : Statement
{
	public override StatementType Type => StatementType.EndScope;

	public override bool IsLegalInDirectMode => false;

	public ScopeType ScopeType { get; set; }

	protected override void RenderImplementation(TextWriter writer)
	{
		var scopeType = this.ScopeType;

		var compilationElement = this.CodeLine?.CompilationElement;

		if (compilationElement != null)
		{
			switch (compilationElement.Type)
			{
				case CompilationElementType.Sub: scopeType = ScopeType.Sub; break;
				case CompilationElementType.Function: scopeType = ScopeType.Function; break;
			}
		}

		switch (scopeType)
		{
			case ScopeType.Sub: writer.Write("END SUB"); break;
			case ScopeType.Function: writer.Write("END FUNCTION"); break;

			default: throw new Exception("Internal error: Invalid ScopeType");
		}
	}
}
