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
		int characterBoxHeight =
			_characterRows switch
			{
				50 => 8,
				43 => 14,
				25 => 16,

				_ => 16,
			};

		bool wasCompatibleTextMode =
			(Machine.GraphicsArray.Graphics.DisableText == false) &&
			(Machine.GraphicsArray.CRTController.CharacterHeight == characterBoxHeight);

		Machine.VideoFirmware.SetMode(3);

		Machine.VideoFirmware.SetCharacterRows(_characterRows);

		Machine.VideoFirmware.DisableBlink();

		if (Machine.GraphicsArray.Sequencer.CharacterWidth == 9)
			Machine.VideoFirmware.SetCharacterWidth(8);

		// We use our own dedicated TextLibrary.
		Machine.VideoFirmware.VisualLibrary.DetachMouseEvents();

		// If the running view is in a compatible text mode, use its font.
		if (wasCompatibleTextMode)
		{
			const int PlaneSize = 65536;

			var savedPlane2 = _savedOutput.AsSpan().Slice(2 * PlaneSize, PlaneSize);
			var idePlane2 = Machine.GraphicsArray.VRAM.AsSpan().Slice(2 * PlaneSize, PlaneSize);

			savedPlane2.CopyTo(idePlane2);
		}
	}

	void ResetOutput()
	{
		Machine.VideoFirmware.SetMode(3);
		Machine.VideoFirmware.SetCharacterRows(25);

		if (Machine.VideoFirmware.VisualLibrary is TextLibrary textLibrary)
			textLibrary.ShowCursor();

		SaveOutput();
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
