using System;
using System.Collections.Generic;

using QBX.Hardware;

using static QBX.Hardware.GraphicsArray;

namespace QBX.DevelopmentEnvironment;

public partial class Program
{
	byte[] _savedOutput = new byte[262144];
	byte[] _savedVideoFirmwareState = Array.Empty<byte>();

	void SetIDEVideoMode()
	{
		Machine.VideoFirmware.SetMode(3);

		Machine.VideoFirmware.DisableBlink();

		if (Machine.GraphicsArray.Sequencer.CharacterWidth == 9)
			Machine.VideoFirmware.SetCharacterWidth(8);
	}

	void SaveOutput()
	{
		Machine.GraphicsArray.VRAM.CopyTo(_savedOutput);

		_savedVideoFirmwareState = new byte[Machine.VideoFirmware.GetStateBufferLength()];

		Machine.VideoFirmware.SaveState(_savedVideoFirmwareState);
	}

	void RestoreOutput()
	{
		if (_savedVideoFirmwareState.Length > 0)
		{
			Machine.VideoFirmware.RestoreState(_savedVideoFirmwareState);

			_savedOutput.CopyTo(Machine.GraphicsArray.VRAM);
		}
	}
}
