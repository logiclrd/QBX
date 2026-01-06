using System.IO;

namespace QBX.CodeModel;

public class Label : IRenderableCode
{
	public string Indentation { get; set; } = "";
	public string Name { get; set; } = "";

	public void Render(TextWriter writer)
	{
		writer.Write(Indentation);

		writer.Write(Name);
		writer.Write(':');
	}
}
