using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;

using QBX.DevelopmentEnvironment.Dialogs.Widgets;
using QBX.ExecutionEngine;
using QBX.ExecutionEngine.Execution;

namespace QBX.DevelopmentEnvironment.Dialogs;

public class SaveFileDialog : DialogWithDirectoryList
{
	Label lblFileName;
	Border bdrFileName;
	TextInput txtFileName;

	// from base: Label lblCurrentDirectory;
	// from base: Label lblDirectories;
	// from base: VerticalListBox lstDirectories;

	Border bdrFormat;

	Canvas cnvFormats;

	RadioButton optBinary;
	Canvas cnvBinaryLabel;
	Label lblBinaryLine1;
	Label lblBinaryLine2;

	RadioButton optText;
	Canvas cnvTextLabel;
	Label lblTextLine1;
	Label lblTextLine2;

	Button cmdOK;
	Button cmdCancel;
	Button cmdHelp;

	bool _showFilter = false;

	protected override void SetFilter(string newFilter)
	{
		base.SetFilter(newFilter);
		_showFilter = true;
	}

	public event Action<RuntimeException>? Error;
	public event Action<string>? TargetPathSpecified;

	public SaveFileDialog(string filePath, Configuration configuration)
		: base(configuration)
	{
		InitializeComponent(Path.GetFileName(filePath));

		SetCurrentDirectory(Path.GetDirectoryName(filePath) ?? Environment.CurrentDirectory);
	}

	[MemberNotNull(nameof(lblFileName))]
	[MemberNotNull(nameof(bdrFileName))]
	[MemberNotNull(nameof(txtFileName))]
	[MemberNotNull(nameof(bdrFormat))]
	[MemberNotNull(nameof(cnvFormats))]
	[MemberNotNull(nameof(optBinary))]
	[MemberNotNull(nameof(cnvBinaryLabel))]
	[MemberNotNull(nameof(lblBinaryLine1))]
	[MemberNotNull(nameof(lblBinaryLine2))]
	[MemberNotNull(nameof(optText))]
	[MemberNotNull(nameof(cnvTextLabel))]
	[MemberNotNull(nameof(lblTextLine1))]
	[MemberNotNull(nameof(lblTextLine2))]
	[MemberNotNull(nameof(cmdOK))]
	[MemberNotNull(nameof(cmdCancel))]
	[MemberNotNull(nameof(cmdHelp))]
	void InitializeComponent(string initialFileName)
	{
		Width = 50;
		Height = 19;

		lblFileName = new Label();
		bdrFileName = new Border();
		txtFileName = new TextInput();
		bdrFormat = new Border();
		cnvFormats = new Canvas();
		optBinary = new RadioButton();
		cnvBinaryLabel = new Canvas();
		lblBinaryLine1 = new Label();
		lblBinaryLine2 = new Label();
		optText = new RadioButton();
		cnvTextLabel = new Canvas();
		lblTextLine1 = new Label();
		lblTextLine2 = new Label();
		cmdOK = new Button();
		cmdCancel = new Button();
		cmdHelp = new Button();

		var formatGroup = new RadioButtonGroup() { optBinary, optText };

		lblFileName.X = 1;
		lblFileName.Y = 1;
		lblFileName.Width = 10;
		lblFileName.Text = "File Name:";
		lblFileName.AccessKeyIndex = 5;
		lblFileName.FocusTarget = bdrFileName;

		txtFileName.X = 13;
		txtFileName.Y = 1;
		txtFileName.Width = 33;
		txtFileName.Height = 1;
		txtFileName.Text = new StringValue(initialFileName);
		txtFileName.GotFocus = txtFileName_GotFocus;

		bdrFileName.Enclose(txtFileName);

		lblCurrentDirectory.X = 1;
		lblCurrentDirectory.Y = 3;
		lblCurrentDirectory.Width = Width - 3;
		lblCurrentDirectory.Text = "C:\\";

		lblDirectories.X = 3;
		lblDirectories.Y = 5;
		lblDirectories.Width = 11;
		lblDirectories.Height = 1;

		lstDirectories.X = 1;
		lstDirectories.Y = 6;
		lstDirectories.Width = 16;
		lstDirectories.Height = 9;
		lstDirectories.SelectionChanged += lstDirectories_SelectionChanged;

		bdrFormat.X = 19;
		bdrFormat.Y = 6;
		bdrFormat.Width = 28;
		bdrFormat.Height = 9;
		bdrFormat.Title = "Format";
		bdrFormat.Child = cnvFormats;
		bdrFormat.IsTabStop = true;

		cnvFormats.X = 20;
		cnvFormats.Y = 7;
		cnvFormats.Width = 26;
		cnvFormats.Height = 7;
		cnvFormats.Children.Add(optBinary);
		cnvFormats.Children.Add(optText);

		optBinary.X = 23;
		optBinary.Y = 7;
		optBinary.Label = cnvBinaryLabel;
		optBinary.RadioButtonGroup = formatGroup;

		cnvBinaryLabel.X = 27;
		cnvBinaryLabel.Y = 7;
		cnvBinaryLabel.Width = 18;
		cnvBinaryLabel.Height = 2;
		cnvBinaryLabel.Children.Add(lblBinaryLine1);
		cnvBinaryLabel.Children.Add(lblBinaryLine2);

		lblBinaryLine1.X = 27;
		lblBinaryLine1.Y = 7;
		lblBinaryLine1.Text = "Binary - Fast load";
		lblBinaryLine1.AccessKeyIndex = 0;
		lblBinaryLine1.FocusTarget = optBinary;
		lblBinaryLine1.AutoSize();

		lblBinaryLine2.X = 28;
		lblBinaryLine2.Y = 8;
		lblBinaryLine2.Text = "and save";
		lblBinaryLine2.AutoSize();

		optText.X = 23;
		optText.Y = 11;
		optText.Label = cnvTextLabel;
		optText.RadioButtonGroup = formatGroup;

		cnvTextLabel.X = 27;
		cnvTextLabel.Y = 11;
		cnvTextLabel.Width = 18;
		cnvTextLabel.Height = 2;
		cnvTextLabel.Children.Add(lblTextLine1);
		cnvTextLabel.Children.Add(lblTextLine2);

		lblTextLine1.X = 27;
		lblTextLine1.Y = 11;
		lblTextLine1.Text = "Text - Readable by";
		lblTextLine1.AccessKeyIndex = 0;
		lblTextLine1.FocusTarget = optText;
		lblTextLine1.AutoSize();

		lblTextLine2.X = 28;
		lblTextLine2.Y = 12;
		lblTextLine2.Text = "other programs";
		lblTextLine2.AutoSize();

		cmdOK.X = 8;
		cmdOK.Y = 16;
		cmdOK.Width = 6;
		cmdOK.Height = 1;
		cmdOK.Text = "OK";
		cmdOK.Activated = cmdOK_Activated;

		cmdCancel.X = 20;
		cmdCancel.Y = 16;
		cmdCancel.Width = 10;
		cmdCancel.Height = 1;
		cmdCancel.Text = "Cancel";
		cmdCancel.Activated = cmdCancel_Activated;

		cmdHelp.X = 36;
		cmdHelp.Y = 16;
		cmdHelp.Width = 8;
		cmdHelp.Height = 1;
		cmdHelp.Text = "Help";
		cmdHelp.AccessKeyIndex = 0;

		Widgets.Add(lblFileName);
		Widgets.Add(bdrFileName);
		Widgets.Add(lblCurrentDirectory);
		Widgets.Add(lblDirectories);
		Widgets.Add(lstDirectories);
		Widgets.Add(bdrFormat);
		Widgets.Add(cmdOK);
		Widgets.Add(cmdCancel);
		Widgets.Add(cmdHelp);

		formatGroup.Select(optText, cnvFormats);

		SetFocus(bdrFileName);
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
						txtFileName.SelectAll();
					else
					{
						string fileName = input.ToString();

						if (fileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
							Error?.Invoke(RuntimeException.BadFileName());
						else
						{
							if (!fileName.Contains('.'))
								fileName = Path.ChangeExtension(fileName, Path.GetExtension(Filter));

							string filePath = Path.Combine(CurrentDirectory, fileName);

							if (filePath.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
								Error?.Invoke(RuntimeException.BadFileName());
							else
							{
								TargetPathSpecified?.Invoke(filePath);

								RestoreCurrentDirectory = false;
							}
						}
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

	private void lstDirectories_SelectionChanged()
	{
		if (lstDirectories.SelectedIndex >= 0)
		{
			txtFileName.Text.Set(lstDirectories.SelectedValue);

			if (_showFilter)
				txtFileName.Text.Append((byte)'\\').Append(Filter);
		}
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
