using System;
using System.IO;

using QBX.ExecutionEngine.Execution;
using QBX.Firmware.Fonts;

namespace QBX.DevelopmentEnvironment.Help;

public class HelpDatabaseTopicLine
{
	public StringValue Text = null!;
	public byte[] AttributeData = System.Array.Empty<byte>();

	public override string ToString() => Text.ToString();

	public static HelpDatabaseTopicLine Parse(ref ReadOnlySpan<byte> data)
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
			new HelpDatabaseTopicLine()
			{
				Text = new StringValue(textBlock),
				AttributeData = attributeBlock.ToArray(),
			};
	}

	static CP437Encoding s_cp437 = new CP437Encoding(ControlCharacterInterpretation.Graphic);

	public void RenderPlainText(TextWriter writer)
	{
		writer.Write(s_cp437.GetString(Text.AsSpan()));
	}
}

