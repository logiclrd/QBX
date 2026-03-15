using System.IO;

namespace QBX.DevelopmentEnvironment;

public interface IEditableLine
{
	int SizeInBytes { get; }

	void Render(TextWriter buffer, bool includeCRLF = true);
}
