using System.Reflection;

namespace QBX.CodeModel;

[AttributeUsage(AttributeTargets.Field)]
public class DataTypeCharacterAttribute(char ch) : Attribute
{
	public char Character { get; } = ch;

	static Dictionary<DataType, DataTypeCharacterAttribute> s_characterAttributeByType =
		typeof(DataType).GetFields(BindingFlags.Public | BindingFlags.Static)
		.Select(t => (DataType: (DataType)t.GetValue(null)!, Attribute: t.GetCustomAttribute<DataTypeCharacterAttribute>()))
		.Where(t => t.Attribute != null)
		.ToDictionary(key => key.DataType, value => value.Attribute!);

	public static char Get(DataType type)
	{
		if (s_characterAttributeByType.TryGetValue(type, out var attribute))
			return attribute.Character;

		return '\0';
	}
}
