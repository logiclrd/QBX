using System.Text;

namespace QBX.Tests.Utility;

public static class StringExtensions
{
	public static string RemoveTrailingWhitespace(this string str)
	{
		str = str.TrimEnd('\r', '\n');

		var buffer = new StringBuilder();

		var reader = new StringReader(str);

		string? line = reader.ReadLine();

		while (line != null)
		{
			buffer.Append(line.TrimEnd());
			line = reader.ReadLine();
		}

		return buffer.ToString();
	}

	public static string ExpandTabs(this string str)
	{
		if (!str.Contains('\t'))
			return str;

		var builder = new StringBuilder();

		foreach (char ch in str)
		{
			if (ch != '\t')
				builder.Append(ch);
			else
			{
				do
					builder.Append(' ');
				while (builder.Length % 8 != 0);
			}
		}

		return builder.ToString();
	}
}
