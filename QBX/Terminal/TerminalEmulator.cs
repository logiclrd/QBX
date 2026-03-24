using System;
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

	internal void Clear()
	{
		visual.Clear();
	}

	internal void ClearAboveCursor()
	{
		throw new NotImplementedException();
	}

	internal void ClearBelowCursor()
	{
		throw new NotImplementedException();
	}

	public void ClearRightOfCursor()
	{
		int savedX = visual.CursorX;
		int savedY = visual.CursorY;

		int remainingChars = visual.CharacterWidth - visual.CursorX;

		Write(Spaces(remainingChars));

		MoveCursor(savedX, savedY);
	}

	internal void ClearLeftOfCursor()
	{
		throw new NotImplementedException();
	}

	internal void ClearCurrentLine()
	{
		throw new NotImplementedException();
	}

	internal void InsertLines(int param)
	{
		throw new NotImplementedException();
	}

	internal void DeleteLines(int param)
	{
		throw new NotImplementedException();
	}

	internal void DeleteCharacters(int param)
	{
		throw new NotImplementedException();
	}

	internal void ClearCharacters(int param)
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

	internal void SetCharacterLineWindow(int v1, int v2)
	{
		throw new NotImplementedException();
	}
}
