using System;
using System.Collections.Generic;

using QBX.Hardware;

using static QBX.Hardware.GraphicsArray;

namespace QBX.DevelopmentEnvironment;

public partial class Program
{
	byte[] _savedOutput = new byte[262144];
	byte[] _savedGraphicsRegisters = new byte[9];
	byte[] _savedSequencerRegisters = new byte[5];
	byte _savedDACMask;
	byte[] _savedDACPalette = new byte[768];
	byte _savedMiscellaneousOutputRegister;
	byte[] _savedCRTControllerRegisters = new byte[25];
	byte[] _savedAttributeControllerRegisters = new byte[21];

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

		SaveRegisters(
			_savedGraphicsRegisters,
			GraphicsRegisters.IndexPort,
			GraphicsRegisters.DataPort);

		SaveRegisters(
			_savedSequencerRegisters,
			SequencerRegisters.IndexPort,
			SequencerRegisters.DataPort);

		_savedDACMask = Machine.GraphicsArray.InPort(
			DACRegisters.MaskPort);

		SavePalette(_savedDACPalette);

		_savedMiscellaneousOutputRegister = Machine.GraphicsArray.InPort(
			MiscellaneousOutputRegisters.ReadPort);

		SaveRegisters(
			_savedCRTControllerRegisters,
			CRTControllerRegisters.IndexPort,
			CRTControllerRegisters.DataPort);

		// Attribute controller just has to be weird.
		for (int i = 0; i < _savedAttributeControllerRegisters.Length; i++)
		{
			Machine.GraphicsArray.InPort(InputStatusRegisters.InputStatus1Port);
			Machine.GraphicsArray.OutPort(AttributeControllerRegisters.IndexAndDataWritePort, (byte)i);
			_savedAttributeControllerRegisters[i] = Machine.GraphicsArray.InPort(AttributeControllerRegisters.DataReadPort);
		}
	}

	void RestoreOutput()
	{
		RestoreRegisters(
			_savedGraphicsRegisters,
			GraphicsRegisters.IndexPort,
			GraphicsRegisters.DataPort);

		RestoreRegisters(
			_savedSequencerRegisters,
			SequencerRegisters.IndexPort,
			SequencerRegisters.DataPort);

		Machine.GraphicsArray.OutPort(
			DACRegisters.MaskPort,
			_savedDACMask);

		RestorePalette(_savedDACPalette);

		Machine.GraphicsArray.OutPort(
			MiscellaneousOutputRegisters.WritePort,
			_savedMiscellaneousOutputRegister);

		RestoreRegisters(
			_savedCRTControllerRegisters,
			CRTControllerRegisters.IndexPort,
			CRTControllerRegisters.DataPort);

		// Attribute controller just has to be weird.
		Machine.GraphicsArray.InPort(InputStatusRegisters.InputStatus1Port);

		for (int i = 0; i < _savedAttributeControllerRegisters.Length; i++)
		{
			Machine.GraphicsArray.OutPort(AttributeControllerRegisters.IndexAndDataWritePort, (byte)i);
			Machine.GraphicsArray.OutPort(AttributeControllerRegisters.IndexAndDataWritePort, _savedAttributeControllerRegisters[i]);
		}

		_savedOutput.CopyTo(Machine.GraphicsArray.VRAM);
	}

	void SaveRegisters(byte[] storage, int indexPort, int dataPort)
	{
		for (int i = 0; i < storage.Length; i++)
		{
			Machine.GraphicsArray.OutPort(indexPort, (byte)i);
			storage[i] = Machine.GraphicsArray.InPort(dataPort);
		}
	}

	void RestoreRegisters(byte[] storage, int indexPort, int dataPort)
	{
		for (int i = 0; i < storage.Length; i++)
		{
			Machine.GraphicsArray.OutPort(indexPort, (byte)i);
			Machine.GraphicsArray.OutPort(dataPort, storage[i]);
		}
	}

	void SavePalette(byte[] storage)
	{
		Machine.GraphicsArray.OutPort(DACRegisters.ReadIndexPort, 0);

		for (int i = 0; i < 768; i++)
			storage[i] = Machine.GraphicsArray.InPort(DACRegisters.DataPort);
	}

	void RestorePalette(byte[] storage)
	{
		Machine.GraphicsArray.OutPort(DACRegisters.WriteIndexPort, 0);

		for (int i = 0; i < 768; i++)
			Machine.GraphicsArray.OutPort(DACRegisters.DataPort, storage[i]);

		Machine.GraphicsArray.DAC.RebuildBGRA();
	}
}
