using System;
using System.IO;
using System.Text;

namespace QBX.Utility;

public class ScopedStreamReader : StreamReader
{
	public ScopedStreamReader(Stream stream)
		: base(stream)
	{
	}

	public ScopedStreamReader(Stream stream, bool detectEncodingFromByteOrderMarks)
		: base(stream, detectEncodingFromByteOrderMarks)
	{
	}

	public ScopedStreamReader(Stream stream, Encoding? encoding)
		: base(stream, encoding)
	{
	}

	public ScopedStreamReader(Stream stream, Encoding? encoding, bool detectEncodingFromByteOrderMarks)
		: base(stream, encoding, detectEncodingFromByteOrderMarks)
	{
	}

	public ScopedStreamReader(Stream stream, Encoding? encoding, bool detectEncodingFromByteOrderMarks, int bufferSize)
		: base(stream, encoding, detectEncodingFromByteOrderMarks, bufferSize)
	{
	}

	public ScopedStreamReader(Stream stream, Encoding? encoding = null, bool detectEncodingFromByteOrderMarks = true, int bufferSize = -1, bool leaveOpen = false)
		: base(stream, encoding, detectEncodingFromByteOrderMarks, bufferSize, leaveOpen)
	{
	}

	public event EventHandler? Closed;

	protected override void Dispose(bool disposing)
	{
		// Close the underlying stream first. If that throws, ensure we still raise the Closed event.

		try
		{
			base.Dispose(disposing);
		}
		catch
		{
			Closed?.Invoke(this, EventArgs.Empty);
			throw;
		}

		Closed?.Invoke(this, EventArgs.Empty);
	}
}
