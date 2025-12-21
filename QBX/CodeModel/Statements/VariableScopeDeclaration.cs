using QBX.CodeModel;

namespace QBX.CodeModel.Statements;

public class VariableScopeDeclaration : IRenderableCode
{
	public string Name { get; set; } = "";
	public bool IsArray { get; set; }
	public DataType? Type { get; set; }
	public string? UserType { get; set; }

	public void Render(TextWriter writer)
	{
		writer.Write(Name);

		if (IsArray)
			writer.Write("()");

		if (Type.HasValue && (UserType != null))
			throw new Exception("Internal error: VariableScopeDeclaration specifies both Type and UserTYpe");

		if (Type.HasValue)
		{
			writer.Write(" AS ");
			writer.Write(Type);
		}
		else if (UserType != null)
		{
			writer.Write(" AS ");
			writer.Write(UserType);
		}
	}
}
