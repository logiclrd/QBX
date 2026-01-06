using System.IO;

namespace QBX.CodeModel;

public interface IRenderableCode
{
	void Render(TextWriter writer);
}
