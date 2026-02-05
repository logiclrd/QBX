using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

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
}
