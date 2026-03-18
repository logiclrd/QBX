using System.IO;

using QBX.CodeModel.Expressions;
using QBX.Parser;

namespace QBX.CodeModel.Statements;

public class ConstDefinition : IRenderableCode
{
	public Identifier Identifier { get; set; }
	public Expression Value { get; set; }

	public ConstDefinition(Identifier identifier, Expression value)
	{
		Identifier = identifier;
		Value = value;
	}

	public void Render(TextWriter writer)
	{
		writer.Write(Identifier);
		writer.Write(" = ");
		Value.Render(writer);
	}
}
