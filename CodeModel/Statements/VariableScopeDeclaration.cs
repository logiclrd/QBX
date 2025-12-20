using QBX.CodeModel;

namespace QBX.CodeModel.Statements;

public class VariableScopeDeclaration : IRenderableCode
{
	public string Name { get; set; } = "";
	public bool IsArray { get; set; }
	public string? Type { get; set; }

	public void Render(TextWriter writer)
	{
		writer.Write(Name);

		if (IsArray)
			writer.Write("()");

		if (Type != null)
		{
			writer.Write(" AS ");
			writer.Write(Type);
		}
	}
}
