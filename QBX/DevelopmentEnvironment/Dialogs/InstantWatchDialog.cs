using QBX.DevelopmentEnvironment.Dialogs.Widgets;
using System;

namespace QBX.DevelopmentEnvironment.Dialogs;

public class InstantWatchDialog : Dialog
{
	Border bdrExpression;
	Label lblExpression;
	Border bdrValue;
	Label lblValue;

	Button cmdAddWatch;
	Button cmdCancel;
	Button cmdHelp;

	Watch? _watch;

	public bool EnableAddWatch
	{
		get => cmdAddWatch.IsEnabled;
		set => cmdAddWatch.IsEnabled = value;
	}

	public event Action? AddWatchClicked;

	protected void OnAddWatchClicked()
	{
		AddWatchClicked?.Invoke();
	}

	public InstantWatchDialog(Configuration configuration)
		: base(configuration)
	{
		Title = "Instant Watch";

		Width = 49;
		Height = 12;

		lblExpression = new Label();
		lblExpression.X = 2;
		lblExpression.Y = 2;
		lblExpression.Width = 43;
		lblExpression.Height = 1;

		bdrExpression = new Border();
		bdrExpression.Title = "Expression";
		bdrExpression.Enclose(lblExpression);

		lblValue = new Label();
		lblValue.X = 2;
		lblValue.Y = 6;
		lblValue.Width = 43;
		lblValue.Height = 1;

		bdrValue = new Border();
		bdrValue.Title = "Value";
		bdrValue.Enclose(lblValue);

		cmdAddWatch = new Button();
		cmdAddWatch.X = 4;
		cmdAddWatch.Y = 9;
		cmdAddWatch.Width = 13;
		cmdAddWatch.Text = "Add Watch";
		cmdAddWatch.AcceleratorKeyIndex = 0;
		cmdAddWatch.Activated = OnAddWatchClicked;

		cmdCancel = new Button();
		cmdCancel.X = 21;
		cmdCancel.Y = 9;
		cmdCancel.Width = 10;
		cmdCancel.Text = "Cancel";
		cmdCancel.Activated = OnClosed;

		cmdHelp = new Button();
		cmdHelp.X = 35;
		cmdHelp.Y = 9;
		cmdHelp.Width = 8;
		cmdHelp.Text = "Help";
		cmdHelp.AcceleratorKeyIndex = 0;

		Widgets.Add(bdrExpression);
		Widgets.Add(bdrValue);

		Widgets.Add(cmdAddWatch);
		Widgets.Add(cmdCancel);
		Widgets.Add(cmdHelp);

		SetFocus(cmdAddWatch);
	}

	public void SetExpression(string text)
	{
	}

	public void SetWatch(Watch watch)
	{
		_watch = watch;
		Update();
	}

	public void Update()
	{
		if (_watch != null)
		{
			lblExpression.Text = _watch.Expression;
			lblValue.Text = _watch.LastValueFormatted?.ToString() ?? "<Not available>";
		}
	}
}
