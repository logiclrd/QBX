/*
namespace QBX.Tests.Parser.Statements;

public class ConstStatement
{
	public override StatementType Type => StatementType.Const;

	public List<ConstDeclaration> Declarations { get; set; }

	public ConstStatement(List<ConstDeclaration> declarations)
	{
		Declarations = declarations;
	}

	public override void Render(TextWriter writer)
	{
		writer.Write("CONST ");

		for (int i = 0; i < Declarations.Count; i++)
		{
			if (i > 0)
				writer.Write(", ");

			Declarations[i].Render(writer);
		}
	}
}

*/
