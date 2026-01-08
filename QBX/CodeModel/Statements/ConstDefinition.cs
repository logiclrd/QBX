using System.IO;

using QBX.CodeModel.Expressions;

namespace QBX.CodeModel.Statements;

public class ConstDefinition : IRenderableCode
{
	public string Identifier { get; set; }
	public Expression Value { get; set; }

	public ConstDefinition(string identifier, Expression value)
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
