using QBX.CodeModel.Expressions;
using QBX.DevelopmentEnvironment.Help;

namespace QBX.DevelopmentEnvironment;

public class SearchPositionIndex
{
	public HelpDatabaseTopic? HelpTopic;
	public int LoadedFileIndex;
	public int ElementIndex;

	public int LineIndex;
	public int CharacterOffset;

	public SearchPositionIndex Clone()
	{
		var ret = new SearchPositionIndex();

		ret.HelpTopic = this.HelpTopic;
		ret.LoadedFileIndex = this.LoadedFileIndex;
		ret.ElementIndex = this.ElementIndex;
		ret.LineIndex = this.LineIndex;

		return ret;
	}

	public static bool operator <(SearchPositionIndex a, SearchPositionIndex b)
	{
		if ((a.HelpTopic != null) && (b.HelpTopic != null))
			return a.HelpTopic.TopicIndex < b.HelpTopic.TopicIndex;

		if (a.LoadedFileIndex < b.LoadedFileIndex)
			return true;
		if (a.LoadedFileIndex > b.LoadedFileIndex)
			return false;

		if (a.ElementIndex < b.ElementIndex)
			return true;
		if (a.ElementIndex > b.ElementIndex)
			return false;

		if (a.LineIndex < b.LineIndex)
			return true;
		if (a.LineIndex > b.LineIndex)
			return false;

		if (a.CharacterOffset < b.CharacterOffset)
			return true;
		if (a.CharacterOffset > b.CharacterOffset)
			return false;

		return false;
	}

	public static bool operator >(SearchPositionIndex a, SearchPositionIndex b)
		=> b < a;
}

