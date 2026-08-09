using System.Collections.Generic;

using QBX.Firmware.Fonts;

namespace QBX.DevelopmentEnvironment;

public class SearchParameters
{
	public string FindWhatString = "";
	public string ChangeToString = "";
	public bool MatchUpperLowercase;
	public bool WholeWord;
	public SearchScope SearchScope;

	public IEqualityComparer<char> Comparer = CP437Encoding.OrdinalComparer;

	public void InitializeComparer()
	{
		if (MatchUpperLowercase)
			Comparer = CP437Encoding.OrdinalComparer;
		else
			Comparer = CP437Encoding.IgnoreCaseComparer;
	}

	public void CopyTo(SearchParameters other)
	{
		other.FindWhatString = this.FindWhatString;
		other.ChangeToString = this.ChangeToString;
		other.MatchUpperLowercase = this.MatchUpperLowercase;
		other.WholeWord = this.WholeWord;
		other.SearchScope = this.SearchScope;
	}
}
