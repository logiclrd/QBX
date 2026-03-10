using System;

using QBX.ExecutionEngine.Execution;

namespace QBX.DevelopmentEnvironment.HelpFiles;

public class HelpFileTopicLine
{
	public StringValue Text = null!;
	public byte[] AttributeData = System.Array.Empty<byte>();

	public override string ToString() => Text.ToString();

	public static HelpFileTopicLine Parse(ref ReadOnlySpan<byte> data)
	{
		int textBlockLength = data[0];
		data = data.Slice(1);

		var textBlock = data.Slice(0, textBlockLength - 1);
		data = data.Slice(textBlockLength - 1);

		int attributeBlockLength = data[0];
		data = data.Slice(1);

		var attributeBlock = data.Slice(0, attributeBlockLength - 1);
		data = data.Slice(attributeBlockLength - 1);

		return
			new HelpFileTopicLine()
			{
				Text = new StringValue(textBlock),
				AttributeData = attributeBlock.ToArray(),
			};
	}
}

