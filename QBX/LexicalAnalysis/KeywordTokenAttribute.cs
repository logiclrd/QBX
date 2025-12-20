namespace QBX.LexicalAnalysis;

public class KeywordTokenAttribute : Attribute
{
	public string? Keyword { get; set; }

	public KeywordTokenAttribute()
	{
	}

	public KeywordTokenAttribute(string keyword)
	{
		Keyword = keyword;
	}
}
