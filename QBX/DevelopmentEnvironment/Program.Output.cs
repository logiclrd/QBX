using System;
using System.Collections.Generic;

using QBX.Firmware;
using QBX.Hardware;

namespace QBX.DevelopmentEnvironment;

public partial class Program
{
	byte[] _savedOutput = new byte[262144];
	byte[] _savedVideoFirmwareState = Array.Empty<byte>();

	VisualLibrary? _savedVisualLibrary;
	int _savedActivePageNumber;
	int _savedCursorX, _savedCursorY;
	int _savedCharacterLineWindowStart, _savedCharacterLineWindowEnd;

	void SetIDEVideoMode()
	{
		Machine.VideoFirmware.SetMode(3);

		Machine.VideoFirmware.DisableBlink();

		if (Machine.GraphicsArray.Sequencer.CharacterWidth == 9)
			Machine.VideoFirmware.SetCharacterWidth(8);

		// We use our own dedicated TextLibrary.
		Machine.VideoFirmware.VisualLibrary.DetachMouseEvents();
	}

	void SaveOutput()
	{
		Machine.GraphicsArray.VRAM.CopyTo(_savedOutput);

		_savedVideoFirmwareState = new byte[Machine.VideoFirmware.GetStateBufferLength()];

		Machine.VideoFirmware.SaveState(_savedVideoFirmwareState);

		_savedVisualLibrary = Machine.VideoFirmware.VisualLibrary;

		_savedActivePageNumber = _savedVisualLibrary.ActivePageNumber;

		_savedCursorX = _savedVisualLibrary.CursorX;
		_savedCursorY = _savedVisualLibrary.CursorY;

		_savedCharacterLineWindowStart = _savedVisualLibrary.CharacterLineWindowStart;
		_savedCharacterLineWindowEnd = _savedVisualLibrary.CharacterLineWindowEnd;
	}

	void RestoreOutput()
	{
		if (_savedVideoFirmwareState.Length > 0)
		{
			Machine.VideoFirmware.RestoreState(_savedVideoFirmwareState);

			_savedOutput.CopyTo(Machine.GraphicsArray.VRAM);

			// Just in case.
			_savedVisualLibrary ??= Machine.VideoFirmware.VisualLibrary;

			_savedVisualLibrary.ActivePageNumber = _savedActivePageNumber;
			_savedVisualLibrary.RefreshParameters();

			_savedVisualLibrary.UpdateCharacterLineWindow(_savedCharacterLineWindowStart, _savedCharacterLineWindowEnd);
			_savedVisualLibrary.MoveCursor(_savedCursorX, _savedCursorY);

			Machine.VideoFirmware.SetVisualLibrary(_savedVisualLibrary);
		}
	}
}
