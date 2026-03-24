using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Versioning;

using QBX.Firmware.Fonts;
using QBX.Platform.Windows;
using QBX.Utility;

namespace QBX.ExecutionEngine.Compiled.Statements.Shell.Windows.ConsoleAPI;

[SupportedOSPlatform(PlatformNames.Windows)]
public class ConsoleDeltaGenerator
{
	ConsoleBufferSnapshot _base = new ConsoleBufferSnapshot();

	static ConsoleBufferSnapshot? CaptureConsoleBuffer()
	{
		var hConsoleOutput = NativeMethods.GetStdHandle(StandardHandles.STD_OUTPUT_HANDLE);

		bool success = NativeMethods.GetConsoleScreenBufferInfo(hConsoleOutput, out var bufferInfo);

		if (!success)
			return null;

		int w = bufferInfo.srWindow.Right - bufferInfo.srWindow.Left + 1;
		int h = bufferInfo.srWindow.Bottom - bufferInfo.srWindow.Top + 1;

		int numTotalChars = w * h;

		var chars = new CHAR_INFO[numTotalChars];

		var readRegion = bufferInfo.srWindow;

		success = NativeMethods.ReadConsoleOutputW(
			hConsoleOutput,
			chars,
			bufferInfo.dwSize,
			new COORD(), // (0, 0)
			ref readRegion);

		if (!success)
			return null;

		var snapshot = new ConsoleBufferSnapshot();

		snapshot.ConsoleWidth = w;
		snapshot.ConsoleHeight = h;

		snapshot.CursorX = bufferInfo.dwCursorPosition.X - bufferInfo.srWindow.Left;
		snapshot.CursorY = bufferInfo.dwCursorPosition.Y - bufferInfo.srWindow.Top;

		snapshot.SnapshotX = 0;
		snapshot.SnapshotY = 0;
		snapshot.SnapshotWidth = snapshot.ConsoleWidth;
		snapshot.SnapshotHeight = snapshot.ConsoleHeight;

		int numTranslatedBytes = 2 * numTotalChars;

		snapshot.Buffer = new byte[numTranslatedBytes];

		for (int i = 0, o = 0; i < numTotalChars; i++, o += 2)
		{
			snapshot.Buffer[o] = CP437Encoding.GetByteGraphic(chars[i].UnicodeChar);
			snapshot.Buffer[o + 1] = unchecked((byte)chars[i].Attributes);
		}

		return snapshot;
	}

	public bool TryGetChangeSnapshot([NotNullWhen(true)] out ConsoleBufferSnapshot? delta)
	{
		var newState = CaptureConsoleBuffer();

		if (newState == null)
		{
			delta = null;
			return false;
		}

		int w = newState.ConsoleWidth;
		int h = newState.ConsoleHeight;

		_base.ResizeBuffer(w, h);

		int x1 = w;
		int x2 = -1;
		int y1 = h;
		int y2 = -1;

		int stride = w * 2;

		var oldBuffer = _base.Buffer.AsSpan();
		var newBuffer = newState.Buffer.AsSpan();

		for (int y = 0; y < h; y++)
		{
			int o = y * stride;

			var oldScan = oldBuffer.Slice(o, stride);
			var newScan = newBuffer.Slice(o, stride);

			bool haveDifference = false;

			for (int p = 0; p < stride; p++)
			{
				if (oldScan[p] != newScan[p])
				{
					haveDifference = true;

					int x = p / 2;

					if (x < x1)
						x1 = x;
					if (y < y1)
						y1 = y;
					if (y > y2)
						y2 = y;

					break;
				}
			}

			if (haveDifference)
			{
				for (int p = stride - 1; p >= 0; p--)
				{
					if (oldScan[p] != newScan[p])
					{
						int x = p / 2;

						if (x > x2)
							x2 = x;

						break;
					}
				}
			}

			if ((x1 == 0) && (x2 + 1 == w)
			 && (y1 == 0) && (y2 + 1 == h))
			{
				// The minimal region is the entire buffer.
				_base = newState;
				delta = newState;

				return true;
			}
		}

		if (((x1 > x2) || (y1 > y2))
		 && (newState.CursorX == _base.CursorX)
		 && (newState.CursorY == _base.CursorY))
		{
			// Nothing changed.
			delta = null;
			return false;
		}

		delta = new ConsoleBufferSnapshot();

		delta.ConsoleWidth = newState.ConsoleWidth;
		delta.ConsoleHeight = newState.ConsoleHeight;

		delta.CursorX = newState.CursorX;
		delta.CursorY = newState.CursorY;

		int deltaW = x2 - x1 + 1;
		int deltaH = y2 - y1 + 1;

		if (deltaW < 0)
			deltaW = 0;
		if (deltaH < 0)
			deltaH = 0;

		int deltaStride = deltaW * 2;

		delta.ResizeBuffer(deltaW, deltaH);

		delta.SnapshotX = x1;
		delta.SnapshotY = y1;
		delta.SnapshotWidth = deltaW;
		delta.SnapshotHeight = deltaH;

		var deltaBuffer = delta.Buffer.AsSpan();

		for (int y = 0; y < deltaH; y++)
		{
			int o = (y + y1) * stride + (x1 * 2);
			int p = y * deltaStride;

			newBuffer.Slice(o, deltaStride).CopyTo(deltaBuffer.Slice(p));
		}

		_base = newState;

		return true;
	}
}
