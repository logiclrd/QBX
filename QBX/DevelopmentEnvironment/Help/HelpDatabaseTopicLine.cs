using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using QBX.ExecutionEngine.Execution;
using QBX.Firmware;
using QBX.Firmware.Fonts;

namespace QBX.DevelopmentEnvironment.Help;

public class HelpDatabaseTopicLine
{
	public List<HelpDatabaseTopicLineTextSpan> TextSpans = new List<HelpDatabaseTopicLineTextSpan>();
	public List<HelpDatabaseTopicLineLink>? Links = null;

	public bool IsCommandLine =>
		TextSpans.Any() &&
		(TextSpans[0].Text.StartsWith((byte)'.') || TextSpans[0].Text.StartsWith((byte)':'));

	public override string ToString() => ToPlainTextString();

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

		var ret = new HelpDatabaseTopicLine();

		if (attributeBlock.Length >= 1)
		{
			int spanLength = attributeBlock[0];

			if (spanLength != 0xFF)
			{
				attributeBlock = attributeBlock.Slice(1);

				if (spanLength > textBlock.Length)
					spanLength = textBlock.Length;

				// First span is implicitly the default attributes.
				var spanAttributes = default(HelpDatabaseTopicLineTextAttributes);

				if (spanLength > 0)
				{
					ret.TextSpans.Add(
						new HelpDatabaseTopicLineTextSpan(
							new StringValue(textBlock.Slice(0, spanLength)),
							spanAttributes));

					textBlock = textBlock.Slice(spanLength);
				}

				while (attributeBlock.Length >= 2)
				{
					if (attributeBlock[0] == 0xFF)
					{
						attributeBlock = attributeBlock.Slice(1);
						break;
					}

					spanAttributes = (HelpDatabaseTopicLineTextAttributes)attributeBlock[0];
					spanLength = attributeBlock[1];

					attributeBlock = attributeBlock.Slice(2);

					if (spanLength > textBlock.Length)
						spanLength = textBlock.Length;

					if (spanLength > 0)
					{
						ret.TextSpans.Add(
							new HelpDatabaseTopicLineTextSpan(
								new StringValue(textBlock.Slice(0, spanLength)),
								spanAttributes));

						textBlock = textBlock.Slice(spanLength);
					}
				}
			}
		}

		if (textBlock.Length > 0)
		{
			ret.TextSpans.Add(
				new HelpDatabaseTopicLineTextSpan(
					new StringValue(textBlock),
					HelpDatabaseTopicLineTextAttributes.None));
		}

		while (attributeBlock.Length >= 4)
		{
			var link = new HelpDatabaseTopicLineLink();

			link.StartIndex = attributeBlock[0] - 1;
			link.EndIndex = attributeBlock[1] - 1;

			if (attributeBlock[2] == 0)
			{
				if (attributeBlock.Length < 5)
					break;

				link.TargetTopicIndex = MemoryMarshal.Cast<byte, ushort>(attributeBlock.Slice(3, 2))[0];
				link.TargetTopicIndex &= 0x7FFF;

				attributeBlock = attributeBlock.Slice(5);
			}
			else
			{
				var contextStringBuilder = new StringValue();

				attributeBlock = attributeBlock.Slice(2);

				while ((attributeBlock.Length > 0) && (attributeBlock[0] != 0))
				{
					contextStringBuilder.Append(attributeBlock[0]);
					attributeBlock = attributeBlock.Slice(1);
				}

				link.TargetContextString = contextStringBuilder.ToString();

				attributeBlock = attributeBlock.Slice(1); // the NUL character
			}

			ret.Links ??= new List<HelpDatabaseTopicLineLink>();
			ret.Links.Add(link);
		}

		return ret;
	}

	static CP437Encoding s_cp437 = new CP437Encoding(ControlCharacterInterpretation.Graphic);

	public string ToPlainTextString()
	{
		var buffer = new StringWriter();

		RenderPlainText(buffer);

		return buffer.ToString();
	}

	public void RenderPlainText(TextWriter writer)
	{
		foreach (var span in TextSpans)
			writer.Write(s_cp437.GetString(span.Text.AsSpan()));
	}

	[ThreadStatic]
	static byte[]? s_spaces;

	public void RenderFormatted(Configuration configuration, TextLibrary target, int startIndex, int length)
	{
		var normal = configuration.DisplayAttributes.HelpWindowNormalText;
		var highlighted = configuration.DisplayAttributes.HelpWindowHighlightedText;
		var linkBorder = configuration.DisplayAttributes.HelpWindowHyperlinkBorderCharacters;

		foreach (var span in TextSpans)
		{
			if (span.Text.Length < startIndex)
			{
				startIndex -= span.Text.Length;
				continue;
			}

			var text = span.Text.AsSpan().Slice(startIndex);

			if (text.Length > length)
				text = text.Slice(0, length);

			if (span.Attributes.HasFlag(HelpDatabaseTopicLineTextAttributes.Italic))
				linkBorder.Set(target);
			else if (span.Attributes.HasFlag(HelpDatabaseTopicLineTextAttributes.Bold))
				highlighted.Set(target);
			else
				normal.Set(target);

			target.WriteText(text);

			startIndex = Math.Max(0, startIndex - text.Length);
			length -= text.Length;

			if (length == 0)
				break;
		}

		if (length > 0)
		{
			normal.Set(target);

			if ((s_spaces == null) || (s_spaces.Length < length))
			{
				s_spaces = new byte[length * 2];
				s_spaces.AsSpan().Fill((byte)' ');
			}

			target.WriteText(s_spaces.AsSpan().Slice(0, length));
		}
	}
}

