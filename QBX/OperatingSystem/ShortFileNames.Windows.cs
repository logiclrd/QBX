using System.Runtime.InteropServices;
using System.Text;

namespace QBX.OperatingSystem;

public partial class ShortFileNames
{
	[DllImport("kernel32", CharSet = CharSet.Auto, SetLastError = true)]
	private static extern int GetShortPathName(
		string lpszLongPath,
		StringBuilder lpszShortPath,
		int cchBuffer);

	static string UnmapWindows(string path) => path; // No unmapping needed; the filesystem accepts short paths directly.

	static bool TryMapWindows(string path, out string shortPath)
	{
		var shortPathBuffer = new StringBuilder(path.Length);

		var shortPathLength = GetShortPathName(path, shortPathBuffer, shortPathBuffer.Length);

		if (shortPathLength > shortPathBuffer.Length)
		{
			shortPathBuffer.Length = shortPathLength;
			shortPathLength = GetShortPathName(path, shortPathBuffer, shortPathBuffer.Length);
		}

		if (Marshal.GetLastPInvokeError() != 0)
		{
			shortPath = "";
			return false;
		}
		else
		{
			shortPath = shortPathBuffer.ToString(0, shortPathLength);
			return true;
		}
	}
}
