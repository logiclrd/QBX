using System.IO;

using QBX.Parser;

namespace QBX.CodeModel;

public class Label : IRenderableCode
{
	public string Indentation { get; set; } = "";
	public Identifier Name { get; set; } = Identifier.Empty;

	public void Render(TextWriter writer)
	{
		writer.Write(Indentation);

		writer.Write(Name);
		writer.Write(':');
	}
}
