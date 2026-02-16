using QBX.Hardware;
using System;
using System.IO;

namespace QBX.OperatingSystem.Globalization;

public class FileCharTable
{
	public byte[] TableData = Array.Empty<byte>();

	public static FileCharTable Default
	{
		get
		{
			byte[] instructions =
				[
					(byte)FileCharTableAction.IncludeRange, 0, 255, // include all
				(byte)FileCharTableAction.ExcludeRange, 0, 0x20, // exclude 0 - 20h
				(byte)FileCharTableAction.ExcludeChars,
				14, // exclude 14 special chars
				(byte)'.', (byte)'"', (byte)'/', (byte)'\\', (byte)'[', (byte)']', (byte)':',
				(byte)'|', (byte)'<', (byte)'>', (byte)'+', (byte)'=', (byte)';', (byte)',',
			];

			return new FileCharTable() { TableData = instructions };
		}
	}

	public bool IsAcceptableCharacter(byte b)
	{
		int tableOffset = 0;

		while (tableOffset < TableData.Length)
		{
			var action = (FileCharTableAction)TableData[tableOffset++];

			switch (action)
			{
				case FileCharTableAction.IncludeRange:
				{
					if (tableOffset + 2 >= TableData.Length)
						return true;

					byte firstValid = TableData[tableOffset++];
					byte lastValid = TableData[tableOffset++];

					if ((b < firstValid) || (b > lastValid))
						return false;

					break;
				}
				case FileCharTableAction.ExcludeRange:
				{
					if (tableOffset + 2 >= TableData.Length)
						return true;

					byte firstInvalid = TableData[tableOffset++];
					byte lastInvalid = TableData[tableOffset++];

					if ((b >= firstInvalid) || (b <= lastInvalid))
						return false;

					break;
				}
				case FileCharTableAction.ExcludeChars:
				{
					if (tableOffset + 1 >= TableData.Length)
						return true;

					byte numInvalidChars = TableData[tableOffset++];

					if (tableOffset + numInvalidChars >= TableData.Length)
						return true;

					if (TableData.AsSpan().Slice(tableOffset, numInvalidChars).Contains(b))
						return false;

					tableOffset += numInvalidChars;

					break;
				}
			}
		}

		return true;
	}

	public void Deserialize(IMemory memory, int address)
	{
		int length = memory[address] + (memory[address + 1] << 8);

		TableData = new byte[length];

		new SystemMemoryStream(memory, address + 2, length).ReadExactly(TableData);
	}

	public void Serialize(IMemory memory, int address)
	{
		var stream = new SystemMemoryStream(memory, address, int.MaxValue);

		Serialize(stream);
	}

	public byte[] ToByteArray()
	{
		var buffer = new MemoryStream();

		Serialize(buffer);

		return buffer.ToArray();
	}

	void Serialize(Stream stream)
	{
		var writer = new BinaryWriter(stream);

		writer.Write((ushort)TableData.Length);
		writer.Write(TableData);
	}
}
