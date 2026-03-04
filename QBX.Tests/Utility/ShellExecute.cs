using System.Runtime.InteropServices;

using QBX.OperatingSystem;

namespace QBX.Tests.Utility;

public class ShellExecute
{
	class ExecutableFileType
	{
		public string? Extension;
		public string? InterpreterFileName;

		public ExecutableFileType() { }

		public ExecutableFileType(string extension, string interpreterFileName)
		{
			Extension = extension;
			InterpreterFileName = interpreterFileName;
		}

		public static implicit operator ExecutableFileType(string extension)
			=> new ExecutableFileType() { Extension = extension };
	}

	static readonly ExecutableFileType[] ExecutableExtensions =
		RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
		? [
				new ExecutableFileType(),
				".exe", ".com",
				new	ExecutableFileType(".cmd", "cmd.exe"),
				new ExecutableFileType(".bat", "cmd.exe")
			]
		: ["", ".sh"];

	static IEnumerable<string> EnumeratePathVariableEntriesForLaunchingProgram()
	{
		yield return ""; // try relative path from CWD

		if (Environment.GetEnvironmentVariable("PATH") is string path)
		{
			foreach (string entry in path.Split(Path.PathSeparator))
				yield return entry;
		}
	}

	public static string FindProgramFileOnPath(string fileName, out string? interpreterFileName)
	{
		bool fileNameSpecifiesExtension = (Path.GetExtension(fileName) is not null);

		foreach (string pathToProbe in EnumeratePathVariableEntriesForLaunchingProgram())
		{
			if (!ShortFileNames.TryMap(pathToProbe, out var pathToProbeShort))
				pathToProbeShort = pathToProbe;

			foreach (var fileType in ExecutableExtensions)
			{
				string fileProbePathShort = fileName;

				if (fileType.Extension != null)
					fileProbePathShort = Path.ChangeExtension(fileProbePathShort, fileType.Extension);

				if (!string.IsNullOrWhiteSpace(pathToProbeShort))
					fileProbePathShort = Path.Join(pathToProbeShort, fileProbePathShort);

				string fileProbePath = ShortFileNames.Unmap(ShortFileNames.GetFullPath(fileProbePathShort));

				if (File.Exists(fileProbePath))
				{
					interpreterFileName = fileType.InterpreterFileName;
					return fileProbePath;
				}

				if (fileNameSpecifiesExtension)
					break;
			}
		}

		// Couldn't find anything, let the OS try (and probably fail).
		interpreterFileName = null;
		return fileName;
	}
}
