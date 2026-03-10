using QBX.ExecutionEngine.Execution;

namespace QBX.DevelopmentEnvironment.Help;

public class HelpDatabaseTopicLineTextSpan(StringValue text, HelpDatabaseTopicLineTextAttributes attributes)
{
	public StringValue Text => text;
	public HelpDatabaseTopicLineTextAttributes Attributes => attributes;
}

