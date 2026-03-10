using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace QBX.DevelopmentEnvironment.Help;

public class OffsetStream(Stream wrapped, long offset) : Stream
{
	public override long Length => wrapped.Length - offset;

	public override long Position
	{
		get => wrapped.Position - offset;
		set => wrapped.Position = offset + value;
	}

	public override bool CanRead => wrapped.CanRead;
	public override bool CanWrite => wrapped.CanWrite;
	public override bool CanSeek => wrapped.CanSeek;
	public override bool CanTimeout => wrapped.CanTimeout;

	public override int ReadTimeout
	{
		get => wrapped.ReadTimeout;
		set => wrapped.ReadTimeout = value;
	}

	public override int WriteTimeout
	{
		get => wrapped.WriteTimeout;
		set => wrapped.WriteTimeout = value;
	}

	public override void CopyTo(Stream destination, int bufferSize)
	{
		if (Position < 0)
			Position = 0;

		base.CopyTo(destination, bufferSize);
	}

	public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
	{
		if (Position < 0)
			Position = 0;

		return base.CopyToAsync(destination, bufferSize, cancellationToken);
	}

	public override long Seek(long seekOffset, SeekOrigin origin)
	{
		switch (origin)
		{
			case SeekOrigin.Begin:
				return wrapped.Seek(offset + seekOffset, origin) - seekOffset;
			case SeekOrigin.Current:
			case SeekOrigin.End:
				return wrapped.Seek(seekOffset, origin) - seekOffset;

			default:
				throw new ArgumentException(nameof(origin));
		}
	}

	public override void SetLength(long value)
	{
		wrapped.SetLength(value + offset);
	}

	public override int ReadByte() => wrapped.ReadByte();
	public override void WriteByte(byte value) => wrapped.WriteByte(value);

	public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback? callback, object? state)
		=> wrapped.BeginRead(buffer, offset, count, callback, state);
	public override int EndRead(IAsyncResult asyncResult)
		=> wrapped.EndRead(asyncResult);
	public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback? callback, object? state)
		=> wrapped.BeginWrite(buffer, offset, count, callback, state);
	public override void EndWrite(IAsyncResult asyncResult)
		=> wrapped.EndWrite(asyncResult);

	public override int Read(byte[] buffer, int offset, int count) => wrapped.Read(buffer, offset, count);
	public override int Read(Span<byte> buffer) => wrapped.Read(buffer);
	public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
		=> wrapped.ReadAsync(buffer, offset, count, cancellationToken);
	public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
		=> wrapped.ReadAsync(buffer, cancellationToken);

	public override void Write(byte[] buffer, int offset, int count) => wrapped.Write(buffer, offset, count);
	public override void Write(ReadOnlySpan<byte> buffer) => wrapped.Write(buffer);
	public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
		=> wrapped.WriteAsync(buffer, offset, count, cancellationToken);
	public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
		=> wrapped.WriteAsync(buffer, cancellationToken);

	public override void Flush() => wrapped.Flush();
	public override Task FlushAsync(CancellationToken cancellationToken) => wrapped.FlushAsync(cancellationToken);
}

