using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;

using QBX.Firmware;
using QBX.Firmware.Fonts;
using QBX.Utility;

namespace QBX.Terminal;

public class TerminalEmulator(VisualLibrary visual)
{
	public const int TabWidth = 9;
	public List<int> TabStops = new List<int>();
	public TerminalAttribute Attribute = new TerminalAttribute(visual);
	public State SavedState;

	// Input arrives as a byte sequence. That byte sequence is turned into a char
	// sequence using InputEncoding, and then the char sequence is turned back
	// into bytes using the current OutputEncoding, which must map any char to a
	// single byte.

	public Decoder InputDecoder = Encoding.UTF8.GetDecoder();

	static readonly CP437Encoding s_cp437 = new CP437Encoding(ControlCharacterInterpretation.Semantic);

	public int OutputEncodingIndex = 0;
	public Encoding[] OutputEncodings = [s_cp437, s_cp437]; // G0 and G1

	public struct State
	{
		public int CursorX;
		public int CursorY;
		public TerminalAttributeState Attribute;
		public int EncodingIndex;
		public Encoding G0Encoding;
		public Encoding G1Encoding;
	}

	public void Reset()
	{
		TabStops.Clear();

		visual.CurrentAttributeByte = 7;
		visual.Clear();

		InputDecoder = Encoding.UTF8.GetDecoder();

		OutputEncodingIndex = 0;

		OutputEncodings[0] = s_cp437;
		OutputEncodings[1] = s_cp437;

		SaveState();
	}

	public void SaveState()
	{
		SavedState.CursorX = visual.CursorX;
		SavedState.CursorY = visual.CursorY;
		SavedState.Attribute = Attribute.State;

		SavedState.EncodingIndex = OutputEncodingIndex;
		SavedState.G0Encoding = OutputEncodings[0];
		SavedState.G1Encoding = OutputEncodings[1];
	}

	public void RestoreState()
	{
		visual.MoveCursor(SavedState.CursorX, SavedState.CursorY);
		Attribute.State = SavedState.Attribute;

		OutputEncodingIndex = SavedState.EncodingIndex;
		OutputEncodings[0] = SavedState.G0Encoding;
		OutputEncodings[1] = SavedState.G1Encoding;

		Attribute.Commit();
	}

	public void LineFeed()
	{
		if (visual.CursorY + 1 >= visual.CharacterLineWindowEnd)
			visual.ScrollTextUp();
		else
			visual.MoveCursor(visual.CursorX, visual.CursorY + 1);
	}

	public void ReverseLineFeed()
	{
		if (visual.CursorY == visual.CharacterLineWindowStart)
			visual.ScrollTextUp();
		else
			visual.MoveCursor(visual.CursorX, visual.CursorY - 1);
	}

	public void NewLine()
	{
		if (visual.CursorY + 1 >= visual.CharacterLineWindowEnd)
		{
			visual.ScrollTextUp();
			visual.MoveCursor(0, visual.CursorY);
		}
		else
			visual.MoveCursor(0, visual.CursorY + 1);
	}

	public void SetHorizontalTabStop()
	{
		if (visual.CursorX % TabWidth == 0)
			return;

		int insertLocation = TabStops.BinarySearch(visual.CursorX);

		if (insertLocation < 0)
			TabStops.Insert(~insertLocation, visual.CursorX);
	}

	public void ClearHorizontalTabStop()
	{
		int deleteLocation = TabStops.BinarySearch(visual.CursorX);

		if (deleteLocation >= 0)
			TabStops.RemoveAt(deleteLocation);
	}

	public void ShowCursor()
	{
		(visual as TextLibrary)?.ShowCursor();
	}

	public void HideCursor()
	{
		(visual as TextLibrary)?.HideCursor();
	}

	public void MoveCursor(int x = -1, int y = -1, int dx = 0, int dy = 0)
	{
		int newX = (x >= 0) ? x : (visual.CursorX + dx);
		int newY = (y >= 0) ? y : (visual.CursorY + dy);

		visual.MoveCursor(
			newX.Clamp(0, visual.CharacterWidth - 1),
			newY.Clamp(0, visual.CharacterHeight - 1));
	}

	[ThreadStatic]
	static byte[]? s_spaces;

	ReadOnlySpan<byte> Spaces(int count)
	{
		if ((s_spaces == null) || (s_spaces.Length < count))
		{
			s_spaces = new byte[count * 2];
			s_spaces.AsSpan().Fill((byte)' ');
		}

		return s_spaces.AsSpan().Slice(0, count);
	}

	public void WriteBlanks(int param)
	{
		Write(Spaces(param));
	}

	public void Write(byte b)
	{
		Span<byte> buffer = stackalloc byte[1];

		buffer[0] = b;

		Write(buffer);
	}

	public void Write(ReadOnlySpan<byte> bytes)
	{
		Span<char> chars = stackalloc char[bytes.Length];

		// First, the convert the bytes we're supplied to intermediate chars.
		InputDecoder.Convert(
			bytes,
			chars,
			flush: false,
			out var bytesUsed,
			out var charsUsed,
			out bool completed);

		if (charsUsed > 0)
		{
			chars = chars.Slice(0, charsUsed);

			// Then, convert the characters to bytes in the output encoding.
			var outputEncoding = OutputEncodings[OutputEncodingIndex];

			int numOutputBytes = outputEncoding.GetByteCount(chars);

			Span<byte> outputBytes = stackalloc byte[numOutputBytes];

			numOutputBytes = outputEncoding.GetBytes(chars, outputBytes);

			outputBytes = outputBytes.Slice(0, numOutputBytes);

			// Finally, send those to the target.
			visual.WriteText(outputBytes);
		}
	}

	class SavedCursorScope(VisualLibrary visual) : IDisposable
	{
		public int CursorX { get; } = visual.CursorX;
		public int CursorY { get; } = visual.CursorY;

		bool _isDisposed;

		public void Dispose()
		{
			if (!_isDisposed)
			{
				if ((visual.CursorX != CursorX)
				 || (visual.CursorY != CursorY))
					visual.MoveCursor(CursorX, CursorY);

				_isDisposed = true;
			}
		}
	}

	class SavedCharacterLineWindowScope(VisualLibrary visual) : IDisposable
	{
		public int CharacterLineWindowStart { get; } = visual.CharacterLineWindowStart;
		public int CharacterLineWindowEnd { get; } = visual.CharacterLineWindowEnd;

		bool _isDisposed;

		public void Dispose()
		{
			if (!_isDisposed)
			{
				visual.UpdateCharacterLineWindow(CharacterLineWindowStart, CharacterLineWindowEnd);
				_isDisposed = true;
			}
		}
	}

	public void Clear()
	{
		visual.Clear();
	}

	public void ClearAboveCursor()
	{
		// We need to clear all characters that precede the cursor treating the buffer
		// as a linear array. VisualLibrary doesn't support this. What it does support
		// is:
		//
		// - Limiting the active region to a range of lines. This constrains Clear.
		// - Writing characters at a particular offset.
		//
		// TextLibrary supports a clipping rectangle as well, but this isn't a feature
		// of the common VisualLibrary interface.

		using (var saved = new SavedCursorScope(visual))
		{
			// Step 1: Clear all lines above the cursor.
			if (saved.CursorY > 0)
			{
				using (new SavedCharacterLineWindowScope(visual))
				{
					visual.UpdateCharacterLineWindow(windowStart: 0, windowEnd: saved.CursorY - 1);
					visual.Clear();
				}
			}

			// Step 2: Clear characters to the left of the cursor.
			visual.WriteTextAt(0, saved.CursorY, Spaces(saved.CursorX + 1));
		}
	}

	public void ClearBelowCursor()
	{
		// We need to clear all characters that precede the cursor treating the buffer
		// as a linear array. VisualLibrary doesn't support this. What it does support
		// is:
		//
		// - Limiting the active region to a range of lines. This constrains Clear.
		// - Writing characters at a particular offset.
		//
		// TextLibrary supports a clipping rectangle as well, but this isn't a feature
		// of the common VisualLibrary interface.

		using (var saved = new SavedCursorScope(visual))
		{
			// Step 1: Clear characters to the right of the cursor.
			visual.WriteText(Spaces(visual.CharacterWidth - saved.CursorX));

			// Step 2: Clear all lines below the cursor.
			if (visual.CursorY + 1 < visual.CharacterHeight)
			{
				using (new SavedCharacterLineWindowScope(visual))
				{
					visual.UpdateCharacterLineWindow(windowStart: saved.CursorY + 1, windowEnd: visual.CharacterHeight - 1);
					visual.Clear();
				}
			}
		}
	}

	public void ClearRightOfCursor()
	{
		using (var saved = new SavedCursorScope(visual))
		{
			int remainingChars = visual.CharacterWidth - saved.CursorX;

			Write(Spaces(remainingChars));
		}
	}

	public void ClearLeftOfCursor()
	{
		using (var saved = new SavedCursorScope(visual))
		{
			int leadingChars = saved.CursorX + 1;

			visual.WriteTextAt(0, saved.CursorY, Spaces(leadingChars));
		}
	}

	public void ClearCurrentLine()
	{
		using (var saved = new SavedCursorScope(visual))
		{
			visual.WriteTextAt(0, saved.CursorY, Spaces(visual.CharacterWidth));
		}
	}

	public void InsertLines(int numLines)
	{
		if (numLines <= 0)
			return;

		using (var saved = new SavedCursorScope(visual))
		{
			byte fillAttribute =
				(visual is TextLibrary)
				? visual.CurrentAttributeByte
				: (byte)0;

			visual.ScrollTextWindow(
				0, visual.CursorY,
				visual.CharacterWidth - 1, visual.CharacterHeight - 1,
				numLines,
				fillAttribute);
		}
	}

	public void DeleteLines(int numLines)
	{
		if (numLines <= 0)
			return;

		using (var saved = new SavedCursorScope(visual))
		{
			byte fillAttribute =
				(visual is TextLibrary)
				? visual.CurrentAttributeByte
				: (byte)0;

			visual.ScrollTextWindow(
				0, visual.CursorY,
				visual.CharacterWidth - 1, visual.CharacterHeight - 1,
				-numLines,
				fillAttribute);
		}
	}

	public void DeleteCharacters(int numChars)
	{
		// VisualLibrary does not expose this operation at all.
		//
		// - For TextLibrary, we can read the characters and write them earlier in the line.
		// - For GraphicsLibrary, we need to capture the characters to be moved as a sprite
		//   and paint it in the new location.

		if (numChars <= 0)
			return;

		using (var saved = new SavedCursorScope(visual))
		{
			int remainingChars = visual.CharacterWidth - saved.CursorX;

			if (visual is TextLibrary textLibrary)
			{
				Span<byte> chars = stackalloc byte[remainingChars];

				for (int x = numChars; x < remainingChars; x++)
					chars[x - numChars] = textLibrary.GetCharacter(saved.CursorX + x, saved.CursorY);

				if (numChars <= remainingChars)
					chars.Slice(remainingChars - numChars).Fill((byte)' ');

				textLibrary.WriteTextAt(saved.CursorX, saved.CursorY, chars);
			}
			else if (visual is GraphicsLibrary graphicsLibrary)
			{
				int shiftWidth = remainingChars * 8;
				int shiftHeight = graphicsLibrary.CharacterScans;

				int cursorPixelX = saved.CursorX * 8;
				int cursorPixelY = saved.CursorY * graphicsLibrary.CharacterScans;

				int clearWidth = (remainingChars - numChars) * 8;
				int clearPixelX = cursorPixelX + shiftWidth;

				int shiftFromX = cursorPixelX + numChars * 8;

				int spriteBytesNeeded = graphicsLibrary.GetSpriteBufferSize(shiftWidth, shiftHeight);

				byte[]? rentedArray = null;

				if (spriteBytesNeeded >= 8192)
					rentedArray = ArrayPool<byte>.Shared.Rent(spriteBytesNeeded);

				try
				{
					Span<byte> spriteBytes =
						rentedArray == null
						? stackalloc byte[spriteBytesNeeded]
						: rentedArray;

					graphicsLibrary.GetSprite(
						shiftFromX, cursorPixelY,
						shiftFromX + shiftWidth - 1, cursorPixelY + shiftHeight - 1,
						spriteBytes);

					graphicsLibrary.PutSprite(
						spriteBytes,
						PutSpriteAction.PixelSet,
						cursorPixelX, cursorPixelY);

					graphicsLibrary.FillBox(
						clearPixelX, cursorPixelY,
						clearPixelX + clearWidth - 1, cursorPixelY + shiftHeight - 1,
						attribute: 0);
				}
				catch { }
				finally
				{
					if (rentedArray != null)
						ArrayPool<byte>.Shared.Return(rentedArray);
				}
			}
		}
	}

	public void ClearCharacters(int param)
	{
		int savedX = visual.CursorX;
		int savedY = visual.CursorY;

		while (param > 0)
		{
			visual.WriteText(' ');

			if (visual.CursorX == 0)
				savedY--;

			param--;
		}

		if (savedY < 0)
			savedY = 0;

		visual.MoveCursor(savedX, savedY);
	}

	public void SetCharacterLineWindow(int windowStart, int windowEnd)
	{
		visual.UpdateCharacterLineWindow(windowStart, windowEnd);
	}
}
