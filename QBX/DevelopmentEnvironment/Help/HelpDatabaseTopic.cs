using System;
using System.Collections.Generic;

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

		return topic;
	}
}

public class CommandNameAttribute(string commandName) : Attribute
{
	public string CommandName => commandName;
}

[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
public class DotAttribute(string commandName) : CommandNameAttribute(commandName) { }
public class ColonAttribute(string commandName) : CommandNameAttribute(commandName) { }

public enum HelpDatabaseTopicCommand
{
	[Dot(".category"), Colon(":c")]
	Category,
	[Dot(".command"), Colon(":x")]
	Command,
	[Dot(".comment"), Dot("..")]
	Comment,
	[Dot(".context")]
	Context,
	[Dot(".end"), Colon(":e")]
	EndPasteSection,
	[Dot(".execute"), Colon(":y")]
	Execute,
	[Dot(".freeze"), Colon(":z")]
	FreezeLines,
	[Dot(".length"), Colon(":l")]
	DefaultWindowSize,
	[Dot(".line")]
	SetLineNumber,
	[Dot(".list"), Colon(":i")]
	TopicList,
	[Dot(".mark"), Colon(":m")]
	Mark,
	[Dot(".next"), Colon(":>")]
	Next,
	[Dot(".paste"), Colon(":p")]
	BeginPasteSection,
	[Dot(".popup"), Colon(":g")]
	ShowAsPopUp,
	[Dot(".previous"), Colon(":<")]
	Previous,
	[Dot(".raw"), Colon(":u")]
	Raw,
	[Dot(".ref"), Colon(":r")]
	ShowInReferenceMenu,
	[Dot(".source")]
	ChainToSource,
	[Dot(".topic"), Colon(":n")]
	TopicName,
}
