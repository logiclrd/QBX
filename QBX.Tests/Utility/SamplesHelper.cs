using System.Runtime.CompilerServices;

namespace QBX.Tests.Utility;

public static class SamplesHelper
{
	static string GetSamplesPath([CallerFilePath] string testFilePath = "")
	{
		string utilityNamespacePath = Path.GetDirectoryName(testFilePath)!;
		string testProjectPath = Path.GetDirectoryName(utilityNamespacePath)!;
		string solutionPath = Path.GetDirectoryName(testProjectPath)!;

		return Path.Combine(solutionPath, "Samples");
	}

	public static IEnumerable<string> EnumerateSamples()
	{
		string samplesPath = GetSamplesPath();

		var exclusions = new HashSet<string>(File.ReadAllLines(Path.Combine(samplesPath, ".test-exclude")), StringComparer.InvariantCultureIgnoreCase);

		foreach (var filePath in Directory.EnumerateFiles(samplesPath, "*.BAS"))
		{
			string fileName = Path.GetFileName(filePath);

			if (!exclusions.Contains(fileName))
				yield return filePath;
		}
	}
}
