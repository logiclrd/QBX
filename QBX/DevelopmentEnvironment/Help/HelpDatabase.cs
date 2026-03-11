using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace QBX.DevelopmentEnvironment.Help;

public class HelpDatabase(bool caseSensitive)
{
	public bool CaseSensitiveContextStrings => caseSensitive;
	public bool IsProtected;

	public string DatabaseName = "";

	public List<HelpDatabaseTopic> Topics = new List<HelpDatabaseTopic>();

	public Dictionary<string, HelpDatabaseTopic> TopicByContextString = new Dictionary<string, HelpDatabaseTopic>(
		caseSensitive ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase);

	public HashSet<string> GlobalContextStrings = new HashSet<string>(
		caseSensitive ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase);

	public void AddTopic(HelpDatabaseTopic topic, IEnumerable<string> contextStrings)
	{
		Topics.Add(topic);

		foreach (var contextString in contextStrings)
		{
			TopicByContextString[contextString] = topic;

			if (!contextString.StartsWith('@'))
				GlobalContextStrings.Add(contextString);
		}
	}

	public bool TryGetGlobalTopic(string contextString, [NotNullWhen(true)] out HelpDatabaseTopic? topic)
	{
		if (GlobalContextStrings.Contains(contextString))
			return TopicByContextString.TryGetValue(contextString, out topic);

		topic = null;
		return false;
	}

	public bool TryGetTopic(string contextString, [NotNullWhen(true)] out HelpDatabaseTopic? topic)
	{
		return TopicByContextString.TryGetValue(contextString, out topic);
	}
}

