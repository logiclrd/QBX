using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using QBX.Hardware;

namespace QBX.OperatingSystem;

public class FileControlBlock
{
	public byte DriveIdentifier;
	public byte[] FileName = new byte[11]; // FILENAMEEXT, the dot is implicit
	public ushort CurrentBlockNumber;
	public ushort RecordSize = 128;
	public uint FileSize;
	public FileDate DateStamp;
	public FileTime TimeStamp;
	public byte CurrentRecordNumber;
	public uint RandomRecordNumber;

	public int RecordPointer => CurrentBlockNumber * RecordSize + RecordPointer;

	public int MemoryAddress;

	[ThreadStatic]
	static byte[]? s_data;

	public void Serialize(Machine machine)
	{
		s_data ??= new byte[36];

		var dataSpan = s_data.AsSpan();

		dataSpan[0] = DriveIdentifier;
		dataSpan = dataSpan.Slice(1);

		FileName.CopyTo(dataSpan);
		dataSpan = dataSpan.Slice(11);

		var ushortSpan = MemoryMarshal.Cast<byte, ushort>(dataSpan);

		ushortSpan[0] = CurrentBlockNumber;
		ushortSpan[1] = RecordSize;
		ushortSpan = ushortSpan.Slice(2);

		var uintSpan = MemoryMarshal.Cast<ushort, uint>(ushortSpan);

		uintSpan[0] = FileSize;
		uintSpan = uintSpan.Slice(1);

		ushortSpan = MemoryMarshal.Cast<uint, ushort>(uintSpan);

		ushortSpan[0] = DateStamp.Raw;
		ushortSpan[1] = TimeStamp.Raw;
		ushortSpan = ushortSpan.Slice(2);

		dataSpan = MemoryMarshal.Cast<ushort, byte>(ushortSpan);

		dataSpan = dataSpan.Slice(8); // Reserved

		dataSpan[0] = CurrentRecordNumber;
		dataSpan[1] = unchecked((byte)RandomRecordNumber);
		dataSpan[2] = unchecked((byte)(RandomRecordNumber >> 8));
		dataSpan[3] = unchecked((byte)(RandomRecordNumber >> 16));

		for (int i = 0; i < 36; i++)
			machine.SystemMemory[MemoryAddress + i] = dataSpan[i];
	}

	public void Deserialize(Machine machine)
	{
		s_data ??= new byte[36];

		var dataSpan = s_data.AsSpan();

		for (int i = 0; i < 36; i++)
			dataSpan[i] = machine.SystemMemory[MemoryAddress + i];

		DriveIdentifier = dataSpan[0];
		dataSpan = dataSpan.Slice(1);

		dataSpan.Slice(0, 11).CopyTo(FileName);
		dataSpan = dataSpan.Slice(11);

		var ushortSpan = MemoryMarshal.Cast<byte, ushort>(dataSpan);

		CurrentBlockNumber = ushortSpan[0];
		RecordSize = ushortSpan[1];
		ushortSpan = ushortSpan.Slice(2);

		var uintSpan = MemoryMarshal.Cast<ushort, uint>(ushortSpan);

		FileSize = uintSpan[0];
		uintSpan = uintSpan.Slice(1);

		ushortSpan = MemoryMarshal.Cast<uint, ushort>(uintSpan);

		DateStamp.Raw = ushortSpan[0];
		TimeStamp.Raw = ushortSpan[1];
		ushortSpan = ushortSpan.Slice(2);

		dataSpan = MemoryMarshal.Cast<ushort, byte>(ushortSpan);

		dataSpan = dataSpan.Slice(8); // Reserved

		uintSpan = MemoryMarshal.Cast<byte, uint>(dataSpan);

		RandomRecordNumber = uintSpan[0];

		// Unpack final fields, separating a byte and a 24-bit integer.
		CurrentRecordNumber = unchecked((byte)RandomRecordNumber);
		RandomRecordNumber >>= 8;
	}
}
