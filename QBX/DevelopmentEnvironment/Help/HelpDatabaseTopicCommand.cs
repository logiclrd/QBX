using System;

namespace QBX.DevelopmentEnvironment.Help;

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
