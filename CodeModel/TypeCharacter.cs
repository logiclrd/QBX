namespace QBX.CodeModel;

public class TypeCharacter : IRenderableCode
{
	public DataType Type { get; set; }

	public void Render(TextWriter writer)
	{
		writer.Write(DataTypeCharacterAttribute.Get(Type));
	}
}
