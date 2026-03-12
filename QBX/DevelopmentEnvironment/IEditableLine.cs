using System.IO;

namespace QBX.DevelopmentEnvironment;

public interface IEditableLine
{
	void Render(TextWriter buffer, bool includeCRLF = true);
}
