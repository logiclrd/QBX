using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace QBX.DevelopmentEnvironment;

public class Configuration
{
	public class DisplayAttributeConfiguration
	{
		public IReadOnlyList<DisplayAttribute> Palette;

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

		public IEnumerable<DisplayAttribute> AllItems { get; private set; }

		public DisplayAttributeConfiguration()
		{
			LoadFullColourConfiguration();
		}

		[MemberNotNull(nameof(AllItems))]
		void RefreshAllItems()
		{
			AllItems = GetType().GetFields(BindingFlags.Public | BindingFlags.Instance)
				.Where(field => field.FieldType == typeof(DisplayAttribute))
				.Select(field => field.GetValue(this))
				.OfType<DisplayAttribute>()
				.ToList();
		}

		[
			MemberNotNull(nameof(Palette)), MemberNotNull(nameof(MenuBarNormalText)), MemberNotNull(nameof(MenuBarSelectedItem)),
			MemberNotNull(nameof(MenuBarAndPullDownMenuAccessKeys)), MemberNotNull(nameof(MenuBarSelectedItemAccessKey)),
			MemberNotNull(nameof(PullDownMenuandListBoxNormalText)), MemberNotNull(nameof(PullDownMenuSelectedItem)),
			MemberNotNull(nameof(PullDownMenuSelectedItemAccessKey)), MemberNotNull(nameof(PullDownMenuBorder)),
			MemberNotNull(nameof(PullDownMenuandDialogBoxShadow)), MemberNotNull(nameof(PullDownMenuandDialogBoxDisabledItems)),
			MemberNotNull(nameof(DialogBoxNormalText)), MemberNotNull(nameof(DialogBoxAccessKeys)),
			MemberNotNull(nameof(DialogBoxCommandButtons)), MemberNotNull(nameof(DialogBoxDepressedCommandButton)),
			MemberNotNull(nameof(DialogBoxActiveCommandButtonBorderCharacters)), MemberNotNull(nameof(ListBoxSelectedItem)),
			MemberNotNull(nameof(ScrollBarsandScrollArrows)), MemberNotNull(nameof(ScrollBarPositionIndicatorBox)),
			MemberNotNull(nameof(DebugWatchWindowNormalText)), MemberNotNull(nameof(DebugWatchWindowHighlightedText)),
			MemberNotNull(nameof(HelpWindowNormalText)), MemberNotNull(nameof(HelpWindowHighlightedText)),
			MemberNotNull(nameof(HelpWindowHyperlinkBorderCharacters)), MemberNotNull(nameof(ProgramViewWindowNormalText)),
			MemberNotNull(nameof(ProgramViewWindowCurrentStatement)), MemberNotNull(nameof(ProgramViewWindowBreakpointLines)),
			MemberNotNull(nameof(ProgramViewWindowIncludedLines)), MemberNotNull(nameof(ReferenceBarNormalText)),
			MemberNotNull(nameof(ReferenceBarHighlightedText)), MemberNotNull(nameof(ReferenceBarStatusIndicators)),
			MemberNotNull(nameof(AllItems))
		]
		public void LoadFullColourConfiguration()
		{
			Palette =
				[
					new(0, 0, "Black"),
					new(1, 0, "Blue"),
					new(2, 0, "Green"),
					new(3, 0, "Cyan"),
					new(4, 0, "Red"),
					new(5, 0, "Magenta"),
					new(6, 0, "Brown"),
					new(7, 0, "White"),
					new(8, 0, "Gray"),
					new(9, 0, "Bright Blue"),
					new(10, 0, "Bright Green"),
					new(11, 0, "Bright Cyan"),
					new(12, 0, "Bright Red"),
					new(13, 0, "Pink"),
					new(14, 0, "Yellow"),
					new(15, 0, "Bright White"),
				];

			MenuBarNormalText = new(0, 7, "Menu Bar Normal Text");
			MenuBarSelectedItem = new(7, 0, "Menu Bar Selected Item");
			MenuBarAndPullDownMenuAccessKeys = new(15, 7, "Menu Bar and Pull-Down Menu Access Keys");
			MenuBarSelectedItemAccessKey = new(15, 0, "Menu Bar Selected Item Access Key");
			PullDownMenuandListBoxNormalText = new(0, 7, "Pull-Down Menu and List Box Normal Text");
			PullDownMenuSelectedItem = new(7, 0, "Pull-Down Menu Selected Item");
			PullDownMenuSelectedItemAccessKey = new(15, 0, "Pull-Down Menu Selected Item Access Key");
			PullDownMenuBorder = new(0, 7, "Pull-Down Menu Border");
			PullDownMenuandDialogBoxShadow = new(8, 0, "Pull-Down Menu and Dialog Box Shadow");
			PullDownMenuandDialogBoxDisabledItems = new(8, 7, "Pull-Down Menu and Dialog Box Disabled Items");
			DialogBoxNormalText = new(0, 7, "Dialog Box Normal Text");
			DialogBoxAccessKeys = new(15, 7, "Dialog Box Access Keys");
			DialogBoxCommandButtons = new(0, 7, "Dialog Box Command Buttons");
			DialogBoxDepressedCommandButton = new(7, 0, "Dialog Box Depressed Command Button");
			DialogBoxActiveCommandButtonBorderCharacters = new(15, 7, "Dialog Box Active Command Button Border Characters");
			ListBoxSelectedItem = new(7, 0, "List Box Selected Item");
			ScrollBarsandScrollArrows = new(0, 7, "Scroll Bars and Scroll Arrows");
			ScrollBarPositionIndicatorBox = new(7, 0, "Scroll Bar Position Indicator Box");
			DebugWatchWindowNormalText = new(0, 3, "Debug Watch Window Normal Text");
			DebugWatchWindowHighlightedText = new(15, 3, "Debug Watch Window Highlighted Text");
			HelpWindowNormalText = new(7, 0, "Help Window Normal Text");
			HelpWindowHighlightedText = new(15, 0, "Help Window Highlighted Text");
			HelpWindowHyperlinkBorderCharacters = new(10, 0, "Help Window Hyperlink Border Characters");
			ProgramViewWindowNormalText = new(7, 1, "Program View Window Normal Text");
			ProgramViewWindowCurrentStatement = new(15, 1, "Program View Window Current Statement");
			ProgramViewWindowBreakpointLines = new(7, 4, "Program View Window Breakpoint Lines");
			ProgramViewWindowIncludedLines = new(14, 1, "Program View Window Included Lines");
			ReferenceBarNormalText = new(0, 3, "Reference Bar Normal Text");
			ReferenceBarHighlightedText = new(15, 3, "Reference Bar Highlighted Text");
			ReferenceBarStatusIndicators = new(0, 3, "Reference Bar Status Indicators");

			RefreshAllItems();
		}

		public void LoadNoHighIntensityConfiguration()
		{
			Palette =
				[
					new(0, 0, "Black"),
					new(1, 0, "Blue"),
					new(2, 0, "Green"),
					new(3, 0, "Cyan"),
					new(4, 0, "Red"),
					new(5, 0, "Magenta"),
					new(6, 0, "Brown"),
					new(7, 0, "White"),
				];

			// It is not a mistake that some of these have high-intensity attributes. That's what QB does.
			MenuBarNormalText = new(0, 7, "Menu Bar Normal Text");
			MenuBarSelectedItem = new(7, 0, "Menu Bar Selected Item");
			MenuBarAndPullDownMenuAccessKeys = new(4, 7, "Menu Bar and Pull-Down Menu Access Keys");
			MenuBarSelectedItemAccessKey = new(2, 0, "Menu Bar Selected Item Access Key");
			PullDownMenuandListBoxNormalText = new(0, 7, "Pull-Down Menu and List Box Normal Text");
			PullDownMenuSelectedItem = new(7, 0, "Pull-Down Menu Selected Item");
			PullDownMenuSelectedItemAccessKey = new(2, 0, "Pull-Down Menu Selected Item Access Key");
			PullDownMenuBorder = new(0, 7, "Pull-Down Menu Border");
			PullDownMenuandDialogBoxShadow = new(8, 0, "Pull-Down Menu and Dialog Box Shadow");
			PullDownMenuandDialogBoxDisabledItems = new(6, 7, "Pull-Down Menu and Dialog Box Disabled Items");
			DialogBoxNormalText = new(0, 7, "Dialog Box Normal Text");
			DialogBoxAccessKeys = new(4, 7, "Dialog Box Access Keys");
			DialogBoxCommandButtons = new(0, 7, "Dialog Box Command Buttons");
			DialogBoxDepressedCommandButton = new(7, 0, "Dialog Box Depressed Command Button");
			DialogBoxActiveCommandButtonBorderCharacters = new(4, 7, "Dialog Box Active Command Button Border Characters");
			ListBoxSelectedItem = new(7, 0, "List Box Selected Item");
			ScrollBarsandScrollArrows = new(0, 7, "Scroll Bars and Scroll Arrows");
			ScrollBarPositionIndicatorBox = new(7, 0, "Scroll Bar Position Indicator Box");
			DebugWatchWindowNormalText = new(0, 3, "Debug Watch Window Normal Text");
			DebugWatchWindowHighlightedText = new(4, 3, "Debug Watch Window Highlighted Text");
			HelpWindowNormalText = new(7, 0, "Help Window Normal Text");
			HelpWindowHighlightedText = new(4, 0, "Help Window Highlighted Text");
			HelpWindowHyperlinkBorderCharacters = new(2, 0, "Help Window Hyperlink Border Characters");
			ProgramViewWindowNormalText = new(7, 1, "Program View Window Normal Text");
			ProgramViewWindowCurrentStatement = new(2, 1, "Program View Window Current Statement");
			ProgramViewWindowBreakpointLines = new(7, 4, "Program View Window Breakpoint Lines");
			ProgramViewWindowIncludedLines = new(14, 1, "Program View Window Included Lines");
			ReferenceBarNormalText = new(0, 3, "Reference Bar Normal Text");
			ReferenceBarHighlightedText = new(0, 3, "Reference Bar Highlighted Text");
			ReferenceBarStatusIndicators = new(0, 3, "Reference Bar Status Indicators");

			RefreshAllItems();
		}

		public void LoadBlackAndWhiteConfiguration()
		{
			Palette =
				[
					new(0, 0, "Black"),
					new(7, 0, "White"),
					new(8, 0, "Gray"),
					new(15, 0, "Bright White"),
				];

			MenuBarNormalText = new(0, 7, "Menu Bar Normal Text");
			MenuBarSelectedItem = new(7, 0, "Menu Bar Selected Item");
			MenuBarAndPullDownMenuAccessKeys = new(15, 7, "Menu Bar and Pull-Down Menu Access Keys");
			MenuBarSelectedItemAccessKey = new(15, 0, "Menu Bar Selected Item Access Key");
			PullDownMenuandListBoxNormalText = new(0, 7, "Pull-Down Menu and List Box Normal Text");
			PullDownMenuSelectedItem = new(7, 0, "Pull-Down Menu Selected Item");
			PullDownMenuSelectedItemAccessKey = new(15, 0, "Pull-Down Menu Selected Item Access Key");
			PullDownMenuBorder = new(0, 7, "Pull-Down Menu Border");
			PullDownMenuandDialogBoxShadow = new(7, 0, "Pull-Down Menu and Dialog Box Shadow");
			PullDownMenuandDialogBoxDisabledItems = new(0, 7, "Pull-Down Menu and Dialog Box Disabled Items");
			DialogBoxNormalText = new(0, 7, "Dialog Box Normal Text");
			DialogBoxAccessKeys = new(15, 7, "Dialog Box Access Keys");
			DialogBoxCommandButtons = new(0, 7, "Dialog Box Command Buttons");
			DialogBoxDepressedCommandButton = new(7, 0, "Dialog Box Depressed Command Button");
			DialogBoxActiveCommandButtonBorderCharacters = new(15, 7, "Dialog Box Active Command Button Border Characters");
			ListBoxSelectedItem = new(7, 0, "List Box Selected Item");
			ScrollBarsandScrollArrows = new(0, 7, "Scroll Bars and Scroll Arrows");
			ScrollBarPositionIndicatorBox = new(7, 0, "Scroll Bar Position Indicator Box");
			DebugWatchWindowNormalText = new(7, 0, "Debug Watch Window Normal Text");
			DebugWatchWindowHighlightedText = new(15, 0, "Debug Watch Window Highlighted Text");
			HelpWindowNormalText = new(7, 0, "Help Window Normal Text");
			HelpWindowHighlightedText = new(15, 0, "Help Window Highlighted Text");
			HelpWindowHyperlinkBorderCharacters = new(15, 7, "Help Window Hyperlink Border Characters");
			ProgramViewWindowNormalText = new(7, 0, "Program View Window Normal Text");
			ProgramViewWindowCurrentStatement = new(15, 0, "Program View Window Current Statement");
			ProgramViewWindowBreakpointLines = new(0, 7, "Program View Window Breakpoint Lines");
			ProgramViewWindowIncludedLines = new(15, 0, "Program View Window Included Lines");
			ReferenceBarNormalText = new(0, 7, "Reference Bar Normal Text");
			ReferenceBarHighlightedText = new(0, 7, "Reference Bar Highlighted Text");
			ReferenceBarStatusIndicators = new(0, 7, "Reference Bar Status Indicators");

			RefreshAllItems();
		}

		public DisplayAttributeConfiguration(DisplayAttributeConfiguration clone)
		{
			LoadFullColourConfiguration();
			CopyFrom(clone);
		}

		public void CopyFrom(DisplayAttributeConfiguration other)
		{
			Palette = other.Palette;

			MenuBarNormalText?.CopyFrom(other.MenuBarNormalText);
			MenuBarSelectedItem?.CopyFrom(other.MenuBarSelectedItem);
			MenuBarAndPullDownMenuAccessKeys?.CopyFrom(other.MenuBarAndPullDownMenuAccessKeys);
			MenuBarSelectedItemAccessKey?.CopyFrom(other.MenuBarSelectedItemAccessKey);
			PullDownMenuandListBoxNormalText?.CopyFrom(other.PullDownMenuandListBoxNormalText);
			PullDownMenuSelectedItem?.CopyFrom(other.PullDownMenuSelectedItem);
			PullDownMenuSelectedItemAccessKey?.CopyFrom(other.PullDownMenuSelectedItemAccessKey);
			PullDownMenuBorder?.CopyFrom(other.PullDownMenuBorder);
			PullDownMenuandDialogBoxShadow?.CopyFrom(other.PullDownMenuandDialogBoxShadow);
			PullDownMenuandDialogBoxDisabledItems?.CopyFrom(other.PullDownMenuandDialogBoxDisabledItems);
			DialogBoxNormalText?.CopyFrom(other.DialogBoxNormalText);
			DialogBoxAccessKeys?.CopyFrom(other.DialogBoxAccessKeys);
			DialogBoxCommandButtons?.CopyFrom(other.DialogBoxCommandButtons);
			DialogBoxDepressedCommandButton?.CopyFrom(other.DialogBoxDepressedCommandButton);
			DialogBoxActiveCommandButtonBorderCharacters?.CopyFrom(other.DialogBoxActiveCommandButtonBorderCharacters);
			ListBoxSelectedItem?.CopyFrom(other.ListBoxSelectedItem);
			ScrollBarsandScrollArrows?.CopyFrom(other.ScrollBarsandScrollArrows);
			ScrollBarPositionIndicatorBox?.CopyFrom(other.ScrollBarPositionIndicatorBox);
			DebugWatchWindowNormalText?.CopyFrom(other.DebugWatchWindowNormalText);
			DebugWatchWindowHighlightedText?.CopyFrom(other.DebugWatchWindowHighlightedText);
			HelpWindowNormalText?.CopyFrom(other.HelpWindowNormalText);
			HelpWindowHighlightedText?.CopyFrom(other.HelpWindowHighlightedText);
			HelpWindowHyperlinkBorderCharacters?.CopyFrom(other.HelpWindowHyperlinkBorderCharacters);
			ProgramViewWindowNormalText?.CopyFrom(other.ProgramViewWindowNormalText);
			ProgramViewWindowCurrentStatement?.CopyFrom(other.ProgramViewWindowCurrentStatement);
			ProgramViewWindowBreakpointLines?.CopyFrom(other.ProgramViewWindowBreakpointLines);
			ProgramViewWindowIncludedLines?.CopyFrom(other.ProgramViewWindowIncludedLines);
			ReferenceBarNormalText?.CopyFrom(other.ReferenceBarNormalText);
			ReferenceBarHighlightedText?.CopyFrom(other.ReferenceBarHighlightedText);
			ReferenceBarStatusIndicators?.CopyFrom(other.ReferenceBarStatusIndicators);
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
