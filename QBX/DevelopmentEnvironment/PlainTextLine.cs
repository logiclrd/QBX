using System.IO;
using System.Text;

using QBX.Utility;

namespace QBX.DevelopmentEnvironment;

public class PlainTextLine(StringBuilder buffer) : IEditableLine
{
	public int SizeInBytes => throw new System.NotImplementedException();

	public TextReader Read() => new StringBuilderReader(buffer);

	public void Render(TextWriter writer, bool includeCRLF = true)
	{
		if (includeCRLF)
			writer.WriteLine(buffer);
		else
			writer.Write(buffer);
	}
}
