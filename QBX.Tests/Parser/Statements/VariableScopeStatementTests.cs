/*
namespace QBX.Tests.Parser.Statements;

public class VariableScopeStatement
{
	public override StatementType Type => StatementType.VariableScopeStatement;

	public VariableScopeType ScopeType { get; set; }
	public List<VariableScopeDeclaration> Declarations { get; } = new List<VariableScopeDeclaration>();

	public override void Render(TextWriter writer)
	{
		switch (ScopeType)
		{
			case VariableScopeType.Shared: writer.Write("SHARED "); break;
			case VariableScopeType.Static: writer.Write("STATIC "); break;
		}

		for (int i = 0; i < Declarations.Count; i++)
		{
			if (i > 0)
				writer.Write(", ");

			Declarations[i].Render(writer);
		}
	}
}

*/
