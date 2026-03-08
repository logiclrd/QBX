using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using System.Text.RegularExpressions;

using Microsoft.Win32.SafeHandles;

using QBX.DevelopmentEnvironment.Dialogs.Widgets;
using QBX.Hardware;

namespace QBX.DevelopmentEnvironment.Dialogs;

public abstract class DialogWithDirectoryList : Dialog
{
	protected Label lblCurrentDirectory;
	protected Label lblDirectories;
	protected VerticalListBox<string> lstDirectories;

	protected string CurrentDirectory = "C:\\";

	protected string SavedCurrentDirectory = Environment.CurrentDirectory;
	protected bool RestoreCurrentDirectory = true;

	const string InitialFilter = "*.BAS";

	static string s_filter = InitialFilter;
	static Regex s_filterExpression = TranslateWildcardsToRegex(InitialFilter);

	protected static string Filter => s_filter;
	protected static Regex FilterExpression => s_filterExpression;

	protected virtual void SetFilter(string newFilter)
	{
		s_filter = newFilter;
		s_filterExpression = TranslateWildcardsToRegex(s_filter);

		RefreshLists();
	}

	static Regex TranslateWildcardsToRegex(string pattern)
	{
		var ignoreCase = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
			? RegexOptions.IgnoreCase
			: RegexOptions.None;

		return new Regex(
			"^" + Regex.Escape(pattern).Replace(@"\*", ".*").Replace(@"\?", ".") + "$",
			RegexOptions.Singleline | ignoreCase);
	}

	/// <summary>
	/// The subclass must call SetCurrentDirectory after completing initialization.
	/// </summary>
	protected DialogWithDirectoryList(Machine machine, Configuration configuration)
		: base(machine, configuration)
	{
		InitializeComponent();
	}

	[MemberNotNull(nameof(lblCurrentDirectory))]
	[MemberNotNull(nameof(lblDirectories))]
	[MemberNotNull(nameof(lstDirectories))]
	void InitializeComponent()
	{
		lblCurrentDirectory = new Label();
		lblDirectories = new Label();
		lstDirectories = new VerticalListBox<string>();

		lblCurrentDirectory.Text = "C:\\";

		lblDirectories.Text = "Dirs/Drives";
		lblDirectories.AccessKeyIndex = 0;
		lblDirectories.FocusTarget = lstDirectories;
	}

	[MemberNotNull(nameof(CurrentDirectory))]
	protected void SetCurrentDirectory(string newPath)
	{
		newPath = Path.GetFullPath(newPath);

		try
		{
			Environment.CurrentDirectory = newPath;
		}
		catch { }

		if (newPath.Length <= lblCurrentDirectory.Width)
			lblCurrentDirectory.Text = newPath;
		else
		{
			int remainingChars = lblCurrentDirectory.Width - 6;

			string prefix = newPath.Substring(0, 3);
			string suffix = newPath.Substring(newPath.Length - remainingChars);

			if (suffix[0] != '\\')
			{
				int separatorIndex = suffix.IndexOf('\\');

				suffix = suffix.Substring(separatorIndex);
			}

			lblCurrentDirectory.Text = prefix + "..." + suffix;
		}

		CurrentDirectory = newPath;

		RefreshLists();
	}

	protected virtual void RefreshLists() => RefreshLists(enumeratedFile: null);

	protected void RefreshLists(Action<FileInfo>? enumeratedFile = null)
	{
		try
		{
			int maxDirectoryWidth = lstDirectories.Width - 2;

			lstDirectories.Clear();

			foreach (var entry in new DirectoryInfo(CurrentDirectory).EnumerateFileSystemInfos())
			{
				if (entry is FileInfo fileInfo)
					enumeratedFile?.Invoke(fileInfo);
				else
					lstDirectories.Items.Add(ListBoxItem.Create(entry.Name, entry.Name + Path.DirectorySeparatorChar));
			}

			lstDirectories.Items.Sort();

			if (Path.GetDirectoryName(CurrentDirectory) != null)
				lstDirectories.Items.Insert(0, ListBoxItem.Create("..", ".." + Path.DirectorySeparatorChar));

			foreach (var driveInfo in DriveInfo.GetDrives())
			{
				string name = driveInfo.Name;

				if ((name.Length >= 2) && (name[1] == ':'))
					lstDirectories.Items.Add(ListBoxItem.Create($"[-{name[0]}-]", driveInfo.RootDirectory.FullName.TrimEnd('\\')));
				else
				{
					if (!HideDrive(driveInfo))
					{
						if (name.Length + 2 > maxDirectoryWidth)
							name = ".." + name.Substring(name.Length - maxDirectoryWidth + 2);

						lstDirectories.Items.Add(ListBoxItem.Create($"[{name}]", driveInfo.RootDirectory.FullName));
					}
				}
			}
		}
		catch { }
	}

	static bool HideDrive(DriveInfo driveInfo)
	{
		// On Linux, DriveInfo.GetDrives() returns a lot of mount points that aren't appropriate in this context.

		if (driveInfo.DriveType == DriveType.Unknown)
			return true;

		switch (driveInfo.DriveFormat)
		{
			case "squashfs":
			case "udev":
				return true;
		}

		string path = driveInfo.RootDirectory.FullName;

		string[] components = path.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries);

		if (components.Length > 0)
		{
			string firstComponent = components[0];

			switch (firstComponent)
			{
				case "boot":
				case "dev":
				case "proc":
				case "run":
				case "sys":
					return true;
			}
		}

		return false;
	}

	protected override void OnClosed()
	{
		if (RestoreCurrentDirectory)
		{
			try
			{
				Environment.CurrentDirectory = SavedCurrentDirectory;
			}
			catch {}
		}

		base.OnClosed();
	}

	static readonly char[] DirectorySeparators = [Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar];

	protected static string GetCanonicalName(string relativePath)
		=> GetCanonicalName(relativePath, Environment.CurrentDirectory);

	protected static string GetCanonicalName(string relativePath, string relativeTo)
	{
		if ((Path.GetPathRoot(relativePath) is string pathRoot)
		 && !string.IsNullOrWhiteSpace(pathRoot))
		{
			if (DirectorySeparators.Contains(relativePath[relativePath.Length - 1]))
			{
				relativePath = relativePath.Substring(pathRoot.Length);
				relativePath = relativePath.TrimStart(DirectorySeparators);

				return Path.Join(
					pathRoot.ToUpperInvariant(),
					GetCanonicalName(relativePath, pathRoot));
			}
			else
			{
				relativePath = relativePath.Substring(pathRoot.Length);

				return pathRoot.ToUpperInvariant()
					+ GetCanonicalName(relativePath, Path.GetFullPath(pathRoot));
			}
		}
		else
		{
			int separatorIndex = relativePath.IndexOfAny(DirectorySeparators);

			if (separatorIndex == 0)
			{
				// Relative path starts with a path separator; jump to root
				relativePath = relativePath.TrimStart(DirectorySeparators);

				return Path.Join(
					Path.GetPathRoot(relativeTo),
					GetCanonicalName(relativePath));
			}
			else
			{
				if (separatorIndex < 0)
					separatorIndex = relativePath.Length;

				string component = relativePath.Substring(0, separatorIndex);

				if (separatorIndex + 1 <= relativePath.Length)
					separatorIndex++;

				relativePath = relativePath.Substring(separatorIndex);
				relativePath = relativePath.TrimStart(DirectorySeparators);

				if (component == ".")
					return GetCanonicalName(relativePath, relativeTo);

				if (component == "..")
				{
					relativeTo = Path.GetDirectoryName(relativeTo) ?? relativeTo;

					if (relativePath != "")
						return GetCanonicalName(relativePath, relativeTo);
					else
						return relativeTo;
				}

				if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
				{
					var fileHandle = CreateFileW(
						Path.Combine(relativeTo, component),
						dwDesiredAccess: 0,
						FILE_SHARE_READWRITE | FILE_SHARE_DELETE,
						lpSecurityAttributes: IntPtr.Zero,
						OPEN_EXISTING,
						FILE_FLAG_BACKUP_SEMANTICS,
						hTemplateFile: IntPtr.Zero);

					if (!fileHandle.IsInvalid)
					{
						try
						{
							var buffer = new StringBuilder();

							buffer.Capacity = 100;

							int result = GetFinalPathNameByHandleW(
								fileHandle,
								buffer,
								buffer.Capacity,
								FILE_NAME_NORMALIZED | VOLUME_NAME_NONE);

							if ((Marshal.GetLastWin32Error() == ERROR_INSUFFICIENT_BUFFER)
							 && (result > 0)
							 && (result < 33000))
							{
								buffer.Capacity = result;

								result = GetFinalPathNameByHandleW(
									fileHandle,
									buffer,
									buffer.Capacity,
									FILE_NAME_NORMALIZED | VOLUME_NAME_NONE);
							}

							if ((Marshal.GetLastWin32Error() == ERROR_NONE)
							 && (result >= 0)
							 && (result < 33000))
							{
								string normalizedPath = buffer.ToString(0, (int)result);

								component = Path.GetFileName(normalizedPath);
							}
						}
						finally
						{
							fileHandle.Close();
						}
					}
				}

				if (relativePath.Length == 0)
					return component;
				else
				{
					return Path.Join(
						component,
						GetCanonicalName(relativePath, Path.Join(relativeTo, component)));
				}
			}
		}
	}

	const string Windows = "Windows";

	const uint FILE_SHARE_READWRITE = 0x00000003;
	const uint FILE_SHARE_DELETE = 0x00000004;

	const uint OPEN_EXISTING = 3;

	const uint FILE_FLAG_BACKUP_SEMANTICS = 0x02000000;

	[DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
	[SupportedOSPlatform(Windows)]
	static extern SafeFileHandle CreateFileW(
		string lpFileName,
		uint dwDesiredAccess,
		uint dwShareMode,
		IntPtr lpSecurityAttributes,
		uint dwCreationDisposition,
		uint dwFlagsAndAttributes,
		IntPtr hTemplateFile);

	const uint FILE_NAME_NORMALIZED = 0x0;

	const uint VOLUME_NAME_NONE = 0x4;

	[DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
	[SupportedOSPlatform(Windows)]
	static extern int GetFinalPathNameByHandleW(
		SafeFileHandle hFile,
		StringBuilder lpszFilePath,
		int cchFilePath,
		uint dwFlags);

	const int ERROR_NONE = 0;
	const int ERROR_INSUFFICIENT_BUFFER = 122;
}
