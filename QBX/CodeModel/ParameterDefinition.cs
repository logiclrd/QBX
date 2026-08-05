using System.IO;

using QBX.LexicalAnalysis;
using QBX.Parser;

namespace QBX.CodeModel;

public class ParameterDefinition : IRenderableCode
{
	public ParameterRepresentation Representation { get; set; }
	public Identifier Name { get; set; } = Identifier.Empty;
	public bool IsArray { get; set; }
	public DataType Type { get; set; } = DataType.Unspecified;
	public Identifier? UserType { get; set; }
	public bool AnyType { get; set; }

	public Token? NameToken { get; set; }
	public Token? TypeToken { get; set; }

	public void Render(TextWriter writer)
	{
		switch (Representation)
		{
			case ParameterRepresentation.BYVAL: writer.Write("BYVAL "); break;
			case ParameterRepresentation.SEG: writer.Write("SEG "); break;
		}

		writer.Write(Name);

		if (IsArray)
			writer.Write("()");

		if (AnyType)
			writer.Write(" AS ANY");
		else if (UserType != null)
			writer.Write(" AS {0}", UserType);
		else if (Type != DataType.Unspecified)
			writer.Write(" AS {0}", Type);
	}
}
