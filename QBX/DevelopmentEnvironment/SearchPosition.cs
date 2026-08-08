using QBX.DevelopmentEnvironment.Help;

namespace QBX.DevelopmentEnvironment;

public class SearchPosition
{
	public IEditableUnit? Unit;
	public IEditableElement? Element;
	public IEditableLine? Line;

	public HelpDatabaseTopic? HelpTopic;

	public int LineIndex;
	public int CharacterOffset;

	public SearchPosition Clone()
	{
		var ret = new SearchPosition();

		ret.Unit = Unit;
		ret.Element = Element;
		ret.Line = Line;

		ret.HelpTopic = HelpTopic;

		ret.LineIndex = LineIndex;
		ret.CharacterOffset = CharacterOffset;

		return ret;
	}
}

