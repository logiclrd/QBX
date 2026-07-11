using QBX.Firmware;
using QBX.Hardware;

namespace QBX.Tests.Firmware;

public class TextLibraryTests
{
	[TestCase(1, 40*25)]
	[TestCase(3, 80*25)]
	public void Clear_should_clear_default_text_mode(int modeNumber, int expectedCharactersCleared)
	{
		// Arrange
		var machine = new Machine();

		var array = machine.GraphicsArray;

		machine.VideoFirmware.SetMode(modeNumber);

		var vram = array.VRAM.AsSpan();

		const byte Semaphore = 0xFF;

		vram.Fill(Semaphore);

		Assume.That(machine.VideoFirmware.VisualLibrary is TextLibrary);

		// Act
		machine.VideoFirmware.VisualLibrary.Clear();

		// Assert
		var plane0 = vram.Slice(0, 65536);
		var plane1 = vram.Slice(65536, 65536);

		for (int i = 0; i < expectedCharactersCleared; i++)
		{
			plane0[i * 2].Should().Be(32); // space
			plane1[i * 2].Should().Be(7); // light gray on black
		}

		plane0.Slice(expectedCharactersCleared * 2).ContainsAnyExcept(Semaphore).Should().BeFalse();
		plane1.Slice(expectedCharactersCleared * 2).ContainsAnyExcept(Semaphore).Should().BeFalse();
	}

	[TestCase(1, 43, 40*43)]
	[TestCase(1, 50, 40*50)]
	[TestCase(3, 43, 80*43)]
	[TestCase(3, 50, 80*50)]
	public void Clear_should_clear_alternate_rows_text_mode(int modeNumber, int newRowCount, int expectedCharactersCleared)
	{
		// Arrange
		var machine = new Machine();

		var array = machine.GraphicsArray;

		machine.VideoFirmware.SetMode(modeNumber);
		machine.VideoFirmware.SetCharacterRows(newRowCount);

		machine.VideoFirmware.VisualLibrary.RefreshParameters();

		var vram = array.VRAM.AsSpan();

		const byte Semaphore = 0xFF;

		vram.Fill(Semaphore);

		Assume.That(machine.VideoFirmware.VisualLibrary is TextLibrary);

		// Act
		machine.VideoFirmware.VisualLibrary.Clear();

		// Assert
		var plane0 = vram.Slice(0, 65536);
		var plane1 = vram.Slice(65536, 65536);

		for (int i = 0; i < expectedCharactersCleared; i++)
		{
			plane0[i * 2].Should().Be(32); // space
			plane1[i * 2].Should().Be(7); // light gray on black
		}

		plane0.Slice(expectedCharactersCleared * 2).ContainsAnyExcept(Semaphore).Should().BeFalse();
		plane1.Slice(expectedCharactersCleared * 2).ContainsAnyExcept(Semaphore).Should().BeFalse();
	}

	[TestCase(25, 79, 1, "a", 0, 2)]
	public void WriteText_that_ends_in_last_column_should_leave_physical_cursor_overtop_of_last_letter_but_internally_track_new_cursor_position(
		int characterRows, int startCursorX, int startCursorY, string text, int expectedCursorX, int expectedCursorY)
	{
		// Arrange
		var machine = new Machine();

		var array = machine.GraphicsArray;

		machine.VideoFirmware.SetMode(3);
		machine.VideoFirmware.SetCharacterRows(characterRows);

		machine.VideoFirmware.VisualLibrary.RefreshParameters();

		Assume.That(machine.VideoFirmware.VisualLibrary is TextLibrary);

		var sut = (TextLibrary)machine.VideoFirmware.VisualLibrary;

		sut.MoveCursor(startCursorX, startCursorY);

		// Act
		sut.WriteText(text);

		// Assert
		int physicalCursorAddress = array.CRTController.CursorAddress;

		int physicalCursorX = physicalCursorAddress % 80;
		int physicalCursorY = physicalCursorAddress / 80;

		sut.CursorXAfterPassiveNewLine.Should().Be(expectedCursorX);
		sut.CursorYAfterPassiveNewLine.Should().Be(expectedCursorY);

		sut.CursorX.Should().Be(79);
		sut.CursorY.Should().Be(startCursorY);

		physicalCursorX.Should().Be(79);
		physicalCursorY.Should().Be(startCursorY);
	}
}
