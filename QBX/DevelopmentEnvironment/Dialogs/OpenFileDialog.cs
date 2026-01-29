using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;

using QBX.DevelopmentEnvironment.Dialogs.Widgets;
using QBX.ExecutionEngine;
using QBX.ExecutionEngine.Execution;

namespace QBX.DevelopmentEnvironment.Dialogs;

public class OpenFileDialog : DialogWithDirectoryList
{
	Label lblFileName;
	Border bdrFileName;
	TextInput txtFileName;

	// from base: Label lblCurrentDirectory;

	Label lblFiles;
	HorizontalListBox lstFiles;

	// from base: Label lblDirectories;
	// from base: VerticalListBox lstDirectories;

	Button cmdOK;
	Button cmdCancel;
	Button cmdHelp;

	public event Action<RuntimeException>? Error;
	public event Action<string>? FileSelected;

	public OpenFileDialog(OpenFileDialogTitle title, Configuration configuration)
		: base(configuration)
	{
		InitializeComponent();

		switch (title)
		{
			case OpenFileDialogTitle.OpenProgram: Title = "Open Program"; break;
			case OpenFileDialogTitle.LoadFile: Title = "Load File"; break;
		}

		SetCurrentDirectory(Environment.CurrentDirectory);
	}

	[MemberNotNull(nameof(lblFileName))]
	[MemberNotNull(nameof(bdrFileName))]
	[MemberNotNull(nameof(txtFileName))]
	[MemberNotNull(nameof(lblFiles))]
	[MemberNotNull(nameof(lstFiles))]
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
		lblFiles = new Label();
		lstFiles = new HorizontalListBox();
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
		txtFileName.Text = new StringValue(Filter);
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
		cmdCancel.Activated = cmdCancel_Activated;

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

	protected override void RefreshLists()
	{
		try
		{
			lstFiles.Clear();

			RefreshLists(
				fileEntry =>
				{
					if (FilterExpression.IsMatch(fileEntry.Name))
						lstFiles.Items.Add(new ListBoxItem(fileEntry.Name));
				});

			lstFiles.Items.Sort();
			lstFiles.RecalculateColumns();
			lstFiles.IsTabStop = (lstFiles.Items.Count > 0);
		}
		catch { }
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

				CurrentDirectory = rootString;
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

					string subpath = Path.Combine(CurrentDirectory, component);

					if (!Directory.Exists(subpath))
					{
						Error?.Invoke(RuntimeException.PathNotFound());
						break;
					}

					CurrentDirectory = subpath;
					txtFileName.Text.Set(input);
					txtFileName.SelectAll();
				}
				else
				{
					SetCurrentDirectory(CurrentDirectory);

					if (input.IndexOfAny('*', '?') >= 0)
						SetFilter(input.ToString());
					else
					{
						var selectedFilePath = Path.Combine(CurrentDirectory, input.ToString());
						FileSelected?.Invoke(selectedFilePath);

						RestoreCurrentDirectory = false;
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
			txtFileName.Text.Set(lstDirectories.SelectedValue).Append(Filter);
	}

	private void cmdOK_Activated()
	{
		txtFileName.Activate();
	}

	private void cmdCancel_Activated()
	{
		Close();
	}
}
