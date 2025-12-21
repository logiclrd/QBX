namespace QBX.CodeModel;

public class ParameterDefinition : IRenderableCode
{
	public bool IsByVal { get; set; }
	public string Name { get; set; } = "";
	public string ActualName { get; set; } = "";
	public bool IsArray { get; set; }
	public DataType? Type { get; set; }
	public string? UserType { get; set; }
	public bool AnyType { get; set; }

	public void Render(TextWriter writer)
	{
		if (IsByVal)
			writer.Write("BYVAL ");

		writer.Write(Name);

		if (IsArray)
			writer.Write("()");

		if (AnyType)
			writer.Write(" AS ANY");
		else if (UserType != null)
			writer.Write(" AS {0}", UserType);
		else if (Type != null)
			writer.Write(" AS {0}", Type);
	}
}
