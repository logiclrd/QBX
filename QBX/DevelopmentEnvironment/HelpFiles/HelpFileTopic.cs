using System;
using System.Collections.Generic;

namespace QBX.DevelopmentEnvironment.HelpFiles;

public class HelpFileTopic
{
	// TODO
	public List<HelpFileTopicLine> Lines = new List<HelpFileTopicLine>();

	public static HelpFileTopic Parse(ReadOnlySpan<byte> data)
	{
		var topic = new HelpFileTopic();

		while (data.Length > 0)
			topic.Lines.Add(HelpFileTopicLine.Parse(ref data));

		return topic;
	}
}

