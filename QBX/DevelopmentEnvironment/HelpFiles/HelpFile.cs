using System;
using System.Collections.Generic;

namespace QBX.DevelopmentEnvironment.HelpFiles;

public class HelpFile
{
	public bool CaseSensitiveContextStrings;
	public bool IsProtected;

	public string DatabaseName = "";

	public List<HelpFileTopic> Topics = new List<HelpFileTopic>();

	public Dictionary<string, HelpFileTopic> TopicByContextString = new Dictionary<string, HelpFileTopic>(StringComparer.OrdinalIgnoreCase);

	public List<string> GlobalContextStrings = new List<string>();

	public void AddTopic(HelpFileTopic topic, IEnumerable<string> contextStrings)
	{
		Topics.Add(topic);

		foreach (var contextString in contextStrings)
		{
			TopicByContextString[contextString] = topic;

			if (!contextString.StartsWith('@'))
				GlobalContextStrings.Add(contextString);
		}
	}
}

