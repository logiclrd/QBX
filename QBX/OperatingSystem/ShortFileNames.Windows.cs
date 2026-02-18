using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

using Microsoft.Win32.SafeHandles;

namespace QBX.OperatingSystem;

public partial class ShortFileNames
{
	[DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
	private static extern int GetShortPathNameW(
		string lpszLongPath,
		StringBuilder lpszShortPath,
		int cchBuffer);

	[DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
	private static extern int SetShortPathNameW(
		SafeFileHandle hFile,
		string lpszShortName);

	[DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
	private static extern SafeFileHandle CreateFileW(
		string lpFileName,
		FileAccessEx dwDesiredAccess,
		FileShare dwShare,
		IntPtr lpSecurityAttributes,
		FileMode dwCreationDisposition,
		CreateFileFlags dwFlagsAndAttributes,
		IntPtr hTemplateFile);

	[Flags]
	enum FileAccessEx
	{
		Delete = 0x00010000,
		GenericWrite = 0x40000000,
	}

	[Flags]
	enum CreateFileFlags
	{
		BackupSemantics = 0x02000000,
	}

	static string GetFullPathWindows(string shortRelativePath)
	{
		var fullPath = Path.GetFullPath(shortRelativePath);

		if (TryMapWindows(fullPath, out var shortPath))
			return shortPath;
		else
			return fullPath;
	}

	static string UnmapWindows(string path) => path; // No unmapping needed; the filesystem accepts short paths directly.

	static void ForgetWindows(string longPath) { } // No action needed on Windows.

	static bool TryMapWindows(string path, out string shortPath)
	{
		var shortPathBuffer = new StringBuilder(path.Length);

		var shortPathLength = GetShortPathNameW(path, shortPathBuffer, shortPathBuffer.Length);

		if (shortPathLength > shortPathBuffer.Length)
		{
			shortPathBuffer.Length = shortPathLength;
			shortPathLength = GetShortPathNameW(path, shortPathBuffer, shortPathBuffer.Length);
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

	static bool TryMapWindows(string path, string shortName)
	{
		var shortPathBuffer = new StringBuilder(path.Length);

		var shortPathLength = GetShortPathNameW(path, shortPathBuffer, shortPathBuffer.Length);

		if (shortPathLength > shortPathBuffer.Length)
		{
			shortPathBuffer.Length = shortPathLength;
			shortPathLength = GetShortPathNameW(path, shortPathBuffer, shortPathBuffer.Length);
		}

		string shortPath = Path.Combine(
			Path.GetDirectoryName(shortPathBuffer.ToString(0, shortPathLength))!,
			shortName);

		var fileHandle = CreateFileW(
			path,
			FileAccessEx.GenericWrite | FileAccessEx.Delete,
			FileShare.None,
			lpSecurityAttributes: IntPtr.Zero,
			FileMode.Open,
			CreateFileFlags.BackupSemantics,
			hTemplateFile: IntPtr.Zero);

		if (fileHandle.IsInvalid)
			return false;

		using (fileHandle)
			SetShortPathNameW(fileHandle, shortName);

		return (Marshal.GetLastPInvokeError() == 0);
	}
}
