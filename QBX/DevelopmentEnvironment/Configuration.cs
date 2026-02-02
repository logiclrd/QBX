using System;

namespace QBX.DevelopmentEnvironment;

public class Configuration
{
	public class DisplayAttributeConfiguration
	{
		public DisplayAttribute MenuBarNormalText = new(0, 7, "Menu Bar Normal Text");
		public DisplayAttribute MenuBarSelectedItem = new(7, 0, "Menu Bar Selected Item");
		public DisplayAttribute MenuBarAndPullDownMenuAccessKeys = new(15, 7, "Menu Bar and Pull-Down Menu Access Keys");
		public DisplayAttribute MenuBarSelectedItemAccessKey = new(15, 0, "Menu Bar Selected Item Access Key");
		public DisplayAttribute PullDownMenuandListBoxNormalText = new(0, 7, "Pull-Down Menu and List Box Normal Text");
		public DisplayAttribute PullDownMenuSelectedItem = new(7, 0, "Pull-Down Menu Selected Item");
		public DisplayAttribute PullDownMenuSelectedItemAccessKey = new(15, 0, "Pull-Down Menu Selected Item Access Key");
		public DisplayAttribute PullDownMenuBorder = new(0, 7, "Pull-Down Menu Border");
		public DisplayAttribute PullDownMenuandDialogBoxShadow = new(8, 0, "Pull-Down Menu and Dialog Box Shadow");
		public DisplayAttribute PullDownMenuandDialogBoxDisabledItems = new(8, 7, "Pull-Down Menu and Dialog Box Disabled Items");
		public DisplayAttribute DialogBoxNormalText = new(0, 7, "Dialog Box Normal Text");
		public DisplayAttribute DialogBoxAccessKeys = new(15, 7, "Dialog Box Access Keys");
		public DisplayAttribute DialogBoxCommandButtons = new(0, 7, "Dialog Box Command Buttons");
		public DisplayAttribute DialogBoxDepressedCommandButton = new(7, 0, "Dialog Box Depressed Command Button");
		public DisplayAttribute DialogBoxActiveCommandButtonBorderCharacters = new(15, 7, "Dialog Box Active Command Button Border Characters");
		public DisplayAttribute ListBoxSelectedItem = new(7, 0, "List Box Selected Item");
		public DisplayAttribute ScrollBarsandScrollArrows = new(0, 7, "Scroll Bars and Scroll Arrows");
		public DisplayAttribute ScrollBarPositionIndicatorBox = new(7, 0, "Scroll Bar Position Indicator Box");
		public DisplayAttribute DebugWatchWindowNormalText = new(0, 3, "Debug Watch Window Normal Text");
		public DisplayAttribute DebugWatchWindowHighlightedText = new(15, 3, "Debug Watch Window Highlighted Text");
		public DisplayAttribute HelpWindowNormalText = new(7, 0, "Help Window Normal Text");
		public DisplayAttribute HelpWindowHighlightedText = new(15, 0, "Help Window Highlighted Text");
		public DisplayAttribute HelpWindowHyperlinkBorderCharacters = new(10, 0, "Help Window Hyperlink Border Characters");
		public DisplayAttribute ProgramViewWindowNormalText = new(7, 1, "Program View Window Normal Text");
		public DisplayAttribute ProgramViewWindowCurrentStatement = new(15, 1, "Program View Window Current Statement");
		public DisplayAttribute ProgramViewWindowBreakpointLines = new(7, 4, "Program View Window Breakpoint Lines");
		public DisplayAttribute ProgramViewWindowIncludedLines = new(14, 1, "Program View Window Included Lines");
		public DisplayAttribute ReferenceBarNormalText = new(0, 3, "Reference Bar Normal Text");
		public DisplayAttribute ReferenceBarHighlightedText = new(15, 3, "Reference Bar Highlighted Text");
		public DisplayAttribute ReferenceBarStatusIndicators = new(0, 3, "Reference Bar Status Indicators");

		public void ForceBlackAndWhite()
		{
			// TODO
			// TODO: this and ConfigureForNoHighIntensity also affect the range of
			// attributes listed by the Options->Display dialog
		}

		public void ConfigureForNoHighIntensity()
		{
			// TODO
		}
	}

	public readonly DisplayAttributeConfiguration DisplayAttributes = new DisplayAttributeConfiguration();

	public bool ShowScrollBars = true;
	public int TabSize = 8;

	public string? IncludeFileSearchPath;
	public string? HelpFileSearchPath;

	public MouseButtonAction RightMouseButtonAction = MouseButtonAction.ContextSensitiveHelp;

	public bool EnableSyntaxChecking = true;
}
