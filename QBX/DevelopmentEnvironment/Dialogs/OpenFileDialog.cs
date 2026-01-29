using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

using QBX.DevelopmentEnvironment.Dialogs.Widgets;
using QBX.ExecutionEngine;
using QBX.ExecutionEngine.Execution;

namespace QBX.DevelopmentEnvironment.Dialogs;

public class OpenFileDialog : Dialog
{
	Label lblFileName;
	Border bdrFileName;
	TextInput txtFileName;

	Label lblCurrentDirectory;

	Label lblFiles;
	HorizontalListBox lstFiles;

	Label lblDirectories;
	VerticalListBox lstDirectories;

	Button cmdOK;
	Button cmdCancel;
	Button cmdHelp;
	
	const string InitialFilter = "*.BAS";

	string _filter = InitialFilter;
	Regex _filterExpression = TranslateWildcardsToRegex(InitialFilter);
	string _currentDirectory;

	public event Action<RuntimeException>? Error;
	public event Action<string>? FileSelected;

	public OpenFileDialog(Configuration configuration)
		: base(configuration)
	{
		InitializeComponent();

		SetCurrentDirectory(Environment.CurrentDirectory);
	}

	[MemberNotNull(nameof(lblFileName))]
	[MemberNotNull(nameof(bdrFileName))]
	[MemberNotNull(nameof(txtFileName))]
	[MemberNotNull(nameof(lblCurrentDirectory))]
	[MemberNotNull(nameof(lblFiles))]
	[MemberNotNull(nameof(lstFiles))]
	[MemberNotNull(nameof(lblDirectories))]
	[MemberNotNull(nameof(lstDirectories))]
	[MemberNotNull(nameof(cmdOK))]
	[MemberNotNull(nameof(cmdCancel))]
	[MemberNotNull(nameof(cmdHelp))]
	void InitializeComponent()
	{
		Width = 67;
		Height = 21;

		lblFileName = new Label();
		bdrFileName = new Border();
		txtFileName = new TextInput();
		lblCurrentDirectory = new Label();
		lblFiles = new Label();
		lstFiles = new HorizontalListBox();
		lblDirectories = new Label();
		lstDirectories = new VerticalListBox();
		cmdOK = new Button();
		cmdCancel = new Button();
		cmdHelp = new Button();

		lblFileName.X = 1;
		lblFileName.Y = 1;
		lblFileName.Width = 10;
		lblFileName.Text = "File Name:";
		lblFileName.AccessKeyIndex = 5;
		lblFileName.FocusTarget = bdrFileName;

		txtFileName.X = 13;
		txtFileName.Y = 1;
		txtFileName.Width = 50;
		txtFileName.Height = 1;
		txtFileName.Text = new StringValue(_filter);
		txtFileName.GotFocus = txtFileName_GotFocus;

		bdrFileName.Enclose(txtFileName);

		lblCurrentDirectory.X = 1;
		lblCurrentDirectory.Y = 4;
		lblCurrentDirectory.Width = Width - 3;
		lblCurrentDirectory.Text = "C:\\";

		lblFiles.X = 21;
		lblFiles.Y = 5;
		lblFiles.Width = 5;
		lblFiles.Height = 1;
		lblFiles.Text = "Files";
		lblFiles.AccessKeyIndex = 0;
		lblFiles.FocusTarget = lstFiles;

		lstFiles.X = 1;
		lstFiles.Y = 6;
		lstFiles.Width = 45;
		lstFiles.Height = 11;
		lstFiles.SelectionChanged = lstFiles_SelectionChanged;

		lblDirectories.X = 50;
		lblDirectories.Y = 5;
		lblDirectories.Width = 11;
		lblDirectories.Height = 1;
		lblDirectories.Text = "Dirs/Drives";
		lblDirectories.AccessKeyIndex = 0;
		lblDirectories.FocusTarget = lstDirectories;

		lstDirectories.X = 48;
		lstDirectories.Y = 6;
		lstDirectories.Width = 16;
		lstDirectories.Height = 11;
		lstDirectories.SelectionChanged = lstDirectories_SelectionChanged;

		cmdOK.X = 11;
		cmdOK.Y = 18;
		cmdOK.Width = 6;
		cmdOK.Height = 1;
		cmdOK.Text = "OK";
		cmdOK.Activated = cmdOK_Activated;

		cmdCancel.X = 27;
		cmdCancel.Y = 18;
		cmdCancel.Width = 10;
		cmdCancel.Height = 1;
		cmdCancel.Text = "Cancel";

		cmdHelp.X = 48;
		cmdHelp.Y = 18;
		cmdHelp.Width = 8;
		cmdHelp.Height = 1;
		cmdHelp.Text = "Help";
		cmdHelp.AccessKeyIndex = 0;

		Widgets.Add(lblFileName);
		Widgets.Add(bdrFileName);
		Widgets.Add(lblCurrentDirectory);
		Widgets.Add(lblFiles);
		Widgets.Add(lstFiles);
		Widgets.Add(lblDirectories);
		Widgets.Add(lstDirectories);
		Widgets.Add(cmdOK);
		Widgets.Add(cmdCancel);
		Widgets.Add(cmdHelp);

		SetFocus(bdrFileName);
	}

	void SetFilter(string newFilter)
	{
		_filter = newFilter;
		_filterExpression = TranslateWildcardsToRegex(_filter);

		RefreshLists();
	}

	[MemberNotNull(nameof(_currentDirectory))]
	void SetCurrentDirectory(string newPath)
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

		_currentDirectory = newPath;

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

	void RefreshLists()
	{
		try
		{
			int maxDirectoryWidth = lstDirectories.Width - 2;

			lstFiles.Clear();
			lstDirectories.Clear();

			foreach (var entry in new DirectoryInfo(_currentDirectory).EnumerateFileSystemInfos())
			{
				if (entry is FileInfo)
				{
					if (_filterExpression.IsMatch(entry.Name))
						lstFiles.Items.Add(new ListBoxItem(entry.Name));
				}
				else
					lstDirectories.Items.Add(new ListBoxItem(entry.Name, entry.Name + Path.DirectorySeparatorChar));
			}

			lstFiles.Items.Sort();
			lstDirectories.Items.Sort();

			if (Path.GetDirectoryName(_currentDirectory) != null)
				lstDirectories.Items.Insert(0, new ListBoxItem("..", ".." + Path.DirectorySeparatorChar));

			foreach (var driveInfo in DriveInfo.GetDrives())
			{
				string name = driveInfo.Name;

				if ((name.Length >= 2) && (name[1] == ':'))
					lstDirectories.Items.Add(new ListBoxItem($"[-{name[0]}-]", driveInfo.RootDirectory.FullName.TrimEnd('\\')));
				else
				{
					if (!HideDrive(driveInfo))
					{
						if (name.Length + 2 > maxDirectoryWidth)
							name = ".." + name.Substring(name.Length - maxDirectoryWidth + 2);

						lstDirectories.Items.Add(new ListBoxItem($"[{name}]", driveInfo.RootDirectory.FullName));
					}
				}
			}

			lstFiles.RecalculateColumns();

			lstFiles.IsTabStop = (lstFiles.Items.Count > 0);
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

	protected override void OnActivated()
	{
		// Repeat as needed:
		// - If txtFileName starts with a path root, navigate to it.
		// - If txtFileName contains a path separator character, try to enter the directory
		//   named by the first component.
		// - If txtFileName contains any wildcard characters, pass it to SetFilter.
		// - Else raise main Item Selected event.

		var input = txtFileName.Text.ToString().AsSpan();

		while (true)
		{
			if (Path.IsPathRooted(input))
			{
				var root = Path.GetPathRoot(input);

				if (root.IsEmpty)
					break;

				input = input.Slice(root.Length);

				string rootString = root.ToString();

				if (!Directory.Exists(rootString))
				{
					Error?.Invoke(RuntimeException.DeviceUnavailable());
					break;
				}

				_currentDirectory = rootString;
				txtFileName.Text.Set(input);
				txtFileName.SelectAll();
			}
			else
			{
				int separatorIndex = input.IndexOfAny(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

				if (separatorIndex > 0)
				{
					var component = input.Slice(0, separatorIndex).ToString();

					input = input.Slice(separatorIndex + 1);

					string subpath = Path.Combine(_currentDirectory, component);

					if (!Directory.Exists(subpath))
					{
						Error?.Invoke(RuntimeException.PathNotFound());
						break;
					}

					_currentDirectory = subpath;
					txtFileName.Text.Set(input);
					txtFileName.SelectAll();
				}
				else
				{
					SetCurrentDirectory(_currentDirectory);

					if (input.IndexOfAny('*', '?') >= 0)
						SetFilter(input.ToString());
					else
					{
						var selectedFilePath = Path.Combine(_currentDirectory, input.ToString());
						FileSelected?.Invoke(selectedFilePath);
					}

					break;
				}
			}
		}
	}

	private void txtFileName_GotFocus()
	{
		txtFileName.SelectAll();
	}

	private void lstFiles_SelectionChanged()
	{
		if (lstFiles.SelectedIndex >= 0)
			txtFileName.Text.Set(lstFiles.SelectedValue);
	}

	private void lstDirectories_SelectionChanged()
	{
		if (lstDirectories.SelectedIndex >= 0)
			txtFileName.Text.Set(lstDirectories.SelectedValue).Append(_filter);
	}

	private void lstDirectories_Activated()
	{
		if ((lstDirectories.SelectedIndex >= 0)
		 && (lstDirectories.SelectedValue != ""))
		{
			SetCurrentDirectory(Path.Combine(_currentDirectory, lstDirectories.SelectedValue));
			txtFileName.SelectAll();
			SetFocus(bdrFileName);
		}
	}

	private void cmdOK_Activated()
	{
		if (lstFiles.SelectedIndex >= 0)
			FileSelected?.Invoke(lstFiles.SelectedValue);
	}
}
