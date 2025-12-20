
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace QBX.CodeModel;

public class TypeCharacter(DataType type) : IRenderableCode
{
	public DataType Type { get; set; } = type;

	static Dictionary<char, TypeCharacter> s_typeCharacterByCharacter =
		typeof(DataType).GetFields(BindingFlags.Static | BindingFlags.Public)
		.Select(field =>
			(
				DataType: (DataType)field.GetValue(null)!,
				Character: field.GetCustomAttribute<DataTypeCharacterAttribute>()!.Character
			))
		.ToDictionary(key => key.Character, value => new TypeCharacter(value.DataType));

	internal static bool TryParse(char ch, [NotNullWhen(true)] out TypeCharacter? typeCharacter)
	{

		return s_typeCharacterByCharacter.TryGetValue(ch, out typeCharacter);
	}

	public void Render(TextWriter writer)
	{
		writer.Write(DataTypeCharacterAttribute.Get(Type));
	}
}
