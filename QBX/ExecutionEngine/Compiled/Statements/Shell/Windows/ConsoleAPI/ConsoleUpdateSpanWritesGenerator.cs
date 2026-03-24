using System;
using System.Runtime.InteropServices;

using QBX.Firmware;

namespace QBX.ExecutionEngine.Compiled.Statements.Shell.Windows.ConsoleAPI;

public class ConsoleUpdateSpanWritesGenerator(VisualLibrary target)
{
	ConsoleBufferSnapshot _base = new ConsoleBufferSnapshot();

	public void ApplySnapshot(ConsoleBufferSnapshot snapshot)
	{
		// exclusive
		int x1 = snapshot.SnapshotX;
		int y1 = snapshot.SnapshotY;
		int x2 = snapshot.SnapshotX + snapshot.SnapshotWidth;
		int y2 = snapshot.SnapshotY + snapshot.SnapshotHeight;

		int width = Math.Min(target.CharacterWidth, Math.Max(_base.ConsoleWidth, x2));
		int height = Math.Min(target.CharacterHeight, Math.Max(_base.ConsoleHeight, y2));

		_base.ResizeBuffer(width, height);

		var baseBufferSpan = _base.Buffer.AsSpan();
		var snapshotBufferSpan = snapshot.Buffer.AsSpan();

		if (target is GraphicsLibrary graphicsTarget)
		{
			// background not supported
			byte attributeMask = unchecked((byte)graphicsTarget.MaximumAttribute);

			attributeMask &= 0x0F;

			for (int i = 1; i < snapshotBufferSpan.Length; i += 2)
				snapshotBufferSpan[i] &= attributeMask;
		}

		int overlayWidth = Math.Min(snapshot.SnapshotWidth, width - x1);
		int overlayHeight = Math.Min(snapshot.SnapshotHeight, height - y1);

		snapshot.ResizeBuffer(overlayWidth, overlayHeight);

		x2 = x1 + overlayWidth;
		y2 = y1 + overlayHeight;

		int baseStride = _base.SnapshotWidth * 2;
		int snapshotStride = snapshot.SnapshotWidth * 2;

		var textTarget = target as TextLibrary;

		using (textTarget?.ShowCursorForScope())
		{
			for (int y = y1; y < y2; y++)
			{
				int o = y * baseStride + x1 * 2;
				int p = (y - y1) * snapshotStride;

				var baseScan = baseBufferSpan.Slice(o, snapshotStride);
				var snapshotScan = snapshotBufferSpan.Slice(p, snapshotStride);

				PushChanges(x1, y, baseScan, snapshotScan);
			}

			target.MoveCursor(snapshot.CursorX, snapshot.CursorY);
		}

		_base.ConsoleWidth = snapshot.ConsoleWidth;
		_base.ConsoleHeight = snapshot.ConsoleHeight;

		_base.CursorX = snapshot.CursorX;
		_base.CursorY = snapshot.CursorY;
	}

	private void PushChanges(int x, int y, Span<byte> baseScan, Span<byte> snapshotScan)
	{
		var baseScanW = MemoryMarshal.Cast<byte, ushort>(baseScan);
		var snapshotScanW = MemoryMarshal.Cast<byte, ushort>(snapshotScan);

		Span<byte> characters = stackalloc byte[snapshotScanW.Length];

		for (int i = 0; i < snapshotScanW.Length; i++)
		{
			if (snapshotScanW[i] != baseScanW[i])
			{
				baseScanW[i] = snapshotScanW[i];
				characters[i] = snapshotScan[i + i];

				int spanStart = i;

				byte attribute = snapshotScan[i + i + 1];

				while ((i + 1 < snapshotScanW.Length)
				    && (snapshotScan[i + i + 2] != baseScan[i + i + 2])
				    && (snapshotScan[i + i + 3] == attribute))
				{
					i++;
					baseScanW[i] = snapshotScanW[i];
					characters[i] = snapshotScan[i + i];
				}

				int spanLength = i - spanStart + 1;

				target.CurrentAttributeByte = attribute;
				target.WriteTextAt(x + spanStart, y, characters.Slice(spanStart, spanLength));
			}
		}
	}
}
