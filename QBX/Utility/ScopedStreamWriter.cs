using System;
using System.IO;
using System.Text;

namespace QBX.Utility;

public class ScopedStreamWriter : StreamWriter
{
	public ScopedStreamWriter(Stream stream)
		: base(stream)
	{
	}

	public ScopedStreamWriter(Stream stream, Encoding? encoding)
		: base(stream, encoding)
	{
	}

	public ScopedStreamWriter(Stream stream, Encoding? encoding, int bufferSize)
		: base(stream, encoding, bufferSize)
	{
	}

	public ScopedStreamWriter(Stream stream, Encoding? encoding = null, int bufferSize = -1, bool leaveOpen = false)
		: base(stream, encoding, bufferSize, leaveOpen)
	{
	}

	public event EventHandler? Closed;

	public override void Close()
	{
		// Close the underlying stream first. If that throws, ensure we still raise the Closed event.

		try
		{
			base.Close();
		}
		catch
		{
			Closed?.Invoke(this, EventArgs.Empty);
			throw;
		}

		Closed?.Invoke(this, EventArgs.Empty);
	}
}
