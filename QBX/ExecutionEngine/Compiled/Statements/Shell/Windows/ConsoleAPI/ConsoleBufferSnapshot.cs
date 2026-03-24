using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;

namespace QBX.ExecutionEngine.Compiled.Statements.Shell.Windows.ConsoleAPI;

public class ConsoleBufferSnapshot
{
	public int ConsoleWidth, ConsoleHeight;

	public int CursorX, CursorY;

	public int SnapshotX, SnapshotY;
	public int SnapshotWidth, SnapshotHeight;

	public byte[]? Buffer;

	[MemberNotNull(nameof(Buffer))]
	public void ResizeBuffer(int width, int height)
	{
		int newSize = width * height * 2;

		if (Buffer == null)
			Buffer = new byte[newSize];
		else if (width == SnapshotWidth)
			System.Array.Resize(ref Buffer, newSize);
		else
		{
			var newBuffer = new byte[newSize];

			int w = Math.Min(width, SnapshotWidth);
			int h = Math.Min(height, SnapshotHeight);

			var oldBufferSpan = Buffer.AsSpan();
			var newBufferSpan = newBuffer.AsSpan();

			for (int y = 0; y < h; y++)
			{
				var oldScan = oldBufferSpan.Slice(y * SnapshotWidth, w);
				var newScan = newBufferSpan.Slice(y * width, w);

				oldScan.CopyTo(newScan);
			}

			Buffer = newBuffer;
		}

		SnapshotWidth = width;
		SnapshotHeight = height;
	}

	public void Serialize(Stream stream)
	{
		int bufferSize = SnapshotWidth * SnapshotHeight * 2;

		if ((Buffer == null) || (Buffer.Length != bufferSize))
			throw new Exception("Buffer is not the correct size");

		var writer = new BinaryWriter(stream, Encoding.ASCII, leaveOpen: true);

		writer.Write(ConsoleWidth);
		writer.Write(ConsoleHeight);

		writer.Write(CursorX);
		writer.Write(CursorY);

		writer.Write(SnapshotX);
		writer.Write(SnapshotY);
		writer.Write(SnapshotWidth);
		writer.Write(SnapshotHeight);

		writer.Write(Buffer);
	}

	public static ConsoleBufferSnapshot Deserialize(Stream stream)
	{
		var reader = new BinaryReader(stream, Encoding.ASCII, leaveOpen: true);

		var snapshot = new ConsoleBufferSnapshot();

		snapshot.ConsoleWidth = reader.ReadInt32();
		snapshot.ConsoleHeight = reader.ReadInt32();

		snapshot.CursorX = reader.ReadInt32();
		snapshot.CursorY = reader.ReadInt32();

		snapshot.SnapshotX = reader.ReadInt32();
		snapshot.SnapshotY = reader.ReadInt32();
		snapshot.SnapshotWidth = reader.ReadInt32();
		snapshot.SnapshotHeight = reader.ReadInt32();

		int bufferSize = snapshot.SnapshotWidth * snapshot.SnapshotHeight * 2;

		snapshot.Buffer = reader.ReadBytes(bufferSize);

		return snapshot;
	}
}
