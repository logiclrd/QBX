using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace QBX.CodeModel;

public class ColumnTrackingTextWriter(TextWriter wrapped) : TextWriter
{
	public override Encoding Encoding => wrapped.Encoding;

	public int Column;

	void UpdateColumnForSpan(ReadOnlySpan<char> charsWritten)
	{
		int newlineIndex = charsWritten.LastIndexOfAny(new[] { '\r', '\n' });

		if (newlineIndex < 0)
			Column += charsWritten.Length;
		else
			Column = charsWritten.Length - newlineIndex - 1;
	}

	public override void Write(bool value)
	{
		wrapped.Write(value);
		Column += value ? 4 : 5;
	}

	public override void Write(char value)
	{
		wrapped.Write(value);

		if ((value == '\r') || (value == '\n'))
			Column = 0;
		else
			Column++;
	}

	public override void Write(char[] buffer, int index, int count)
	{
		wrapped.Write(buffer, index, count);
		UpdateColumnForSpan(buffer.AsSpan().Slice(index, count));
	}

	public override void Write(char[]? buffer)
	{
		if (buffer != null)
		{
			wrapped.Write(buffer);
			UpdateColumnForSpan(buffer);
		}
	}

	public override void Write(ReadOnlySpan<char> buffer)
	{
		wrapped.Write(buffer);
		UpdateColumnForSpan(buffer);
	}

	public override Task WriteAsync(char value)
	{
		if ((value == '\r') || (value == '\n'))
			Column = 0;
		else
			Column++;

		return wrapped.WriteAsync(value);
	}
}
