using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace QBX.CodeModel;

public class TrimEndTextWriter(TextWriter wrapped) : TextWriter
{
	public override Encoding Encoding => wrapped.Encoding;

	StringBuilder _trailingWhiteSpace = new StringBuilder();

	void AcceptWhiteSpace()
	{
		if (_trailingWhiteSpace.Length > 0)
		{
			foreach (var chunk in _trailingWhiteSpace.GetChunks())
				wrapped.Write(chunk);

			_trailingWhiteSpace.Length = 0;
		}
	}

	public override void Write(char value)
	{
		if (char.IsWhiteSpace(value))
			_trailingWhiteSpace.Append(value);
		else
		{
			AcceptWhiteSpace();
			wrapped.Write(value);
		}
	}

	public override void Write(ReadOnlySpan<char> buffer)
	{
		int lastConcreteChar = buffer.Length - 1;

		while ((lastConcreteChar >= 0) && char.IsWhiteSpace(buffer[lastConcreteChar]))
			lastConcreteChar--;

		if (lastConcreteChar < 0)
			_trailingWhiteSpace.Append(buffer);
		else
		{
			AcceptWhiteSpace();
			wrapped.Write(buffer.Slice(0, lastConcreteChar + 1));
			_trailingWhiteSpace.Append(buffer.Slice(lastConcreteChar + 1));
		}
	}

	public override void Write(char[]? buffer)
	{
		if (buffer != null)
			Write(buffer.AsSpan());
	}

	public override void Write(char[] buffer, int index, int count)
	{
		Write(buffer.AsSpan().Slice(index, count));
	}

	public override void Write(string? value)
	{
		if (value != null)
			Write(value.AsSpan());
	}

	public override async Task WriteAsync(ReadOnlyMemory<char> bufferMemory, CancellationToken cancellationToken = default)
	{
		var buffer = bufferMemory.Span;

		int lastConcreteChar = buffer.Length - 1;

		while ((lastConcreteChar >= 0) && char.IsWhiteSpace(buffer[lastConcreteChar]))
			lastConcreteChar--;

		if (lastConcreteChar < 0)
			_trailingWhiteSpace.Append(buffer);
		else
		{
			AcceptWhiteSpace();

			await wrapped.WriteAsync(bufferMemory.Slice(0, lastConcreteChar + 1), cancellationToken);

			buffer = bufferMemory.Span;

			_trailingWhiteSpace.Append(buffer.Slice(lastConcreteChar + 1).ToArray());
		}
	}

	public override Task WriteAsync(char[] buffer, int index, int count)
	{
		return WriteAsync(buffer.AsMemory().Slice(index, count));
	}

	public override Task WriteAsync(string? value)
	{
		return WriteAsync(value.AsMemory());
	}
}

