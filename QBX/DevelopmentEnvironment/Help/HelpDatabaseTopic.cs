using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace QBX.DevelopmentEnvironment.Help;

public class HelpDatabaseTopic(HelpDatabase database)
{
	public string TopicName = "Unknown";

	public HelpDatabase Database => database;

	public List<string> Statements = new List<string>();
	public List<HelpDatabaseTopicLine> Lines = new List<HelpDatabaseTopicLine>();

	public string GetStatementArgument(string statementName)
	{
		foreach (string statement in Statements)
			if (statement.StartsWith(statementName))
				return statement.Substring(statementName.Length).TrimStart();

		return "";
	}

	static readonly Dictionary<HelpDatabaseTopicCommand, FieldInfo> s_commandFieldInfo =
		typeof(HelpDatabaseTopicCommand).GetFields(BindingFlags.Static | BindingFlags.Public)
		.Select(fieldInfo => (FieldInfo: fieldInfo, Value: fieldInfo.GetValue(default) as HelpDatabaseTopicCommand?))
		.Where(item => item.Value.HasValue)
		.ToDictionary(key => key.Value!.Value, element => element.FieldInfo);

	static readonly string ImpossiblePrefix = new string('A', 300);

	public string GetStatementArgument(HelpDatabaseTopicCommand command)
	{
		if (s_commandFieldInfo.TryGetValue(command, out var fieldInfo))
		{
			string dotPrefix = ImpossiblePrefix;
			string colonPrefix = ImpossiblePrefix;

			if (fieldInfo.GetCustomAttribute<DotAttribute>() is DotAttribute dot)
				dotPrefix = dot.CommandName + " ";
			if (fieldInfo.GetCustomAttribute<ColonAttribute>() is ColonAttribute colon)
				colonPrefix = colon.CommandName; // No required space separator

			foreach (string statement in Statements)
			{
				if (statement.StartsWith(dotPrefix))
					return statement.Substring(dotPrefix.Length).TrimStart();
				if (statement.StartsWith(colonPrefix))
					return statement.Substring(colonPrefix.Length).TrimStart();
			}
		}

		return "";
	}

	public static HelpDatabaseTopic Parse(HelpDatabase database, ReadOnlySpan<byte> data)
	{
		var topic = new HelpDatabaseTopic(database);

		while (data.Length > 0)
		{
			var line = HelpDatabaseTopicLine.Parse(ref data);

			if (line.IsCommandLine)
				topic.Statements.Add(line.ToPlainTextString());
			else
				topic.Lines.Add(line);
		}

		topic.TopicName = topic.GetStatementArgument(HelpDatabaseTopicCommand.TopicName);

		return topic;
	}
}

public class CommandNameAttribute(string commandName) : Attribute
{
	public string CommandName => commandName;
}
