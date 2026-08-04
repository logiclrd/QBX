using QBX.DevelopmentEnvironment.Help;

namespace QBX.DevelopmentEnvironment;

public class BookMark
{
	public BookMarkTargetType TargetType;
	public IEditableUnit? Unit;
	public IEditableElement? Element;
	public HelpDatabaseTopic? HelpTopic;
	public int CursorX, CursorY;
}
