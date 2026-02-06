using QBX.ExecutionEngine.Execution.Variables;
using System.IO;

using static QBX.Hardware.GraphicsArray;

namespace QBX.Firmware;

public partial class Video
{
	public int GetStateBufferLength()
	{
		var dummy = new MemoryStream();

		var writer = new BinaryWriter(dummy);

		SaveState(writer);

		writer.Flush();

		return (int)dummy.Length;
	}

	const int GraphicsRegisterCount = 9;
	const int SequencerRegisterCount = 5;
	const int CRTControllerRegisterCount = 25;
	const int AttributeControllerRegisterCount = 21;

	public void SaveState(byte[] buffer)
	{
		using (var stream = new MemoryStream(buffer))
		using (var writer = new BinaryWriter(stream))
			SaveState(writer);
	}

	public void SaveState(BinaryWriter writer)
	{
		SaveRegisters(
			writer,
			GraphicsRegisterCount,
			GraphicsRegisters.IndexPort,
			GraphicsRegisters.DataPort);

		SaveRegisters(
			writer,
			SequencerRegisterCount,
			SequencerRegisters.IndexPort,
			SequencerRegisters.DataPort);

		writer.Write(machine.GraphicsArray.InPort(DACRegisters.MaskPort));

		SavePalette(writer);

		writer.Write(machine.GraphicsArray.InPort(MiscellaneousOutputRegisters.ReadPort));

		SaveRegisters(
			writer,
			CRTControllerRegisterCount,
			CRTControllerRegisters.IndexPort,
			CRTControllerRegisters.DataPort);

		// Attribute controller just has to be weird.
		for (int i = 0; i < AttributeControllerRegisterCount; i++)
		{
			machine.GraphicsArray.InPort(InputStatusRegisters.InputStatus1Port);
			machine.GraphicsArray.OutPort(AttributeControllerRegisters.IndexAndDataWritePort, (byte)i);
			writer.Write(machine.GraphicsArray.InPort(AttributeControllerRegisters.DataReadPort));
		}

		machine.GraphicsArray.InPort(InputStatusRegisters.InputStatus1Port);
		machine.GraphicsArray.OutPort(AttributeControllerRegisters.IndexAndDataWritePort, AttributeControllerRegisters.Index_PaletteAddressSourceBit);
	}

	public void RestoreState(byte[] buffer)
	{
		using (var stream = new MemoryStream(buffer))
		using (var reader = new BinaryReader(stream))
			RestoreState(reader);
	}

	public void RestoreState(BinaryReader reader)
	{
		RestoreRegisters(
			reader,
			GraphicsRegisterCount,
			GraphicsRegisters.IndexPort,
			GraphicsRegisters.DataPort);

		RestoreRegisters(
			reader,
			SequencerRegisterCount,
			SequencerRegisters.IndexPort,
			SequencerRegisters.DataPort);

		machine.GraphicsArray.OutPort(
			DACRegisters.MaskPort,
			reader.ReadByte());

		RestorePalette(reader);

		machine.GraphicsArray.OutPort(
			MiscellaneousOutputRegisters.WritePort,
			reader.ReadByte());

		RestoreRegisters(
			reader,
			CRTControllerRegisterCount,
			CRTControllerRegisters.IndexPort,
			CRTControllerRegisters.DataPort);

		// Attribute controller just has to be weird.
		machine.GraphicsArray.InPort(InputStatusRegisters.InputStatus1Port);

		for (int i = 0; i < AttributeControllerRegisterCount; i++)
		{
			machine.GraphicsArray.OutPort(AttributeControllerRegisters.IndexAndDataWritePort, (byte)i);
			machine.GraphicsArray.OutPort(AttributeControllerRegisters.IndexAndDataWritePort, reader.ReadByte());
		}

		machine.GraphicsArray.OutPort(AttributeControllerRegisters.IndexAndDataWritePort, AttributeControllerRegisters.Index_PaletteAddressSourceBit);
	}

	void SaveRegisters(BinaryWriter storage, int registerCount, int indexPort, int dataPort)
	{
		for (int i = 0; i < registerCount; i++)
		{
			machine.GraphicsArray.OutPort(indexPort, (byte)i);
			storage.Write(machine.GraphicsArray.InPort(dataPort));
		}
	}

	void RestoreRegisters(BinaryReader storage, int registerCount, int indexPort, int dataPort)
	{
		for (int i = 0; i < registerCount; i++)
		{
			machine.GraphicsArray.OutPort(indexPort, (byte)i);
			machine.GraphicsArray.OutPort(dataPort, storage.ReadByte());
		}
	}

	void SavePalette(BinaryWriter writer)
	{
		machine.GraphicsArray.OutPort(DACRegisters.ReadIndexPort, 0);

		for (int i = 0; i < 768; i++)
			writer.Write(machine.GraphicsArray.InPort(DACRegisters.DataPort));
	}

	void RestorePalette(BinaryReader reader)
	{
		machine.GraphicsArray.OutPort(DACRegisters.WriteIndexPort, 0);

		for (int i = 0; i < 768; i++)
			machine.GraphicsArray.OutPort(DACRegisters.DataPort, reader.ReadByte());

		machine.GraphicsArray.DAC.RebuildBGRA();
	}
}
