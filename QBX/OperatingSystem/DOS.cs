using System;
using System.Collections.Generic;

using QBX.Hardware;

namespace QBX.OperatingSystem;

public class DOS
{
	public bool IsTerminated = false;

	public List<FileDescriptor> Files = new List<FileDescriptor>();

	Machine _machine;

	public DOS(Machine machine)
	{
		_machine = machine;

		Files.Add(new FileDescriptor(ReadStandardInput, null));
		Files.Add(new FileDescriptor(null, WriteStandardOutput) { WriteThrough = true });
	}

	void ReadStandardInput(FileBuffer readBuffer)
	{
		while (true)
		{
			_machine.Keyboard.WaitForInput();

			var evt = _machine.Keyboard.GetNextEvent();

			if ((evt != null) && !evt.IsEphemeral)
			{
				if (evt.IsNormalText)
					readBuffer.Push(unchecked((byte)evt.TextCharacter));
				else
				{
					readBuffer.Push(0);
					readBuffer.Push(unchecked((byte)evt.ScanCode));
				}
			}
		}
	}

	int WriteStandardOutput(ReadOnlySpan<byte> buffer)
	{
		var visual = _machine.VideoFirmware.VisualLibrary;

		bool savedControlCharacterFlag = visual.ProcessControlCharacters;

		try
		{
			visual.ProcessControlCharacters = true;
			visual.WriteText(buffer);

			return buffer.Length;
		}
		finally
		{
			visual.ProcessControlCharacters = savedControlCharacterFlag;
		}
	}

	public void TerminateProgram()
	{
		IsTerminated = true;
	}

	public byte ReadByte(int fd, bool echo = false)
	{
		if ((fd < 0) || (fd >= Files.Count))
			throw new ArgumentException("Invalid file descriptor");

		var fileDescriptor = Files[fd];

		byte b = fileDescriptor.ReadByte();

		if (b == 13)
			fileDescriptor.Column = 0;
		else
			fileDescriptor.Column++;

		if (echo)
			_machine.VideoFirmware.VisualLibrary.WriteText(b);

		return b;
	}

	public void WriteByte(int fd, byte b, out byte lastByteWritten)
	{
		if ((fd < 0) || (fd >= Files.Count))
			throw new ArgumentException("Invalid file descriptor");

		var fileDescriptor = Files[fd];

		lastByteWritten = b;

		if (b != 9)
		{
			fileDescriptor.WriteByte(b);
			if (b == 13)
				fileDescriptor.Column = 0;
			else
				fileDescriptor.Column++;
		}
		else
		{
			lastByteWritten = 32;

			do
			{
				fileDescriptor.WriteByte(32);
				fileDescriptor.Column++;
			} while ((fileDescriptor.Column & 7) != 0);
		}
	}

	readonly byte[] ControlCharacters = [9, 13];

	public void Write(int fd, ReadOnlySpan<byte> bytes, out byte lastByteWritten)
	{
		if ((fd < 0) || (fd >= Files.Count))
			throw new ArgumentException("Invalid file descriptor");

		var fileDescriptor = Files[fd];

		lastByteWritten = 0;

		while (bytes.Length > 0)
		{
			int controlCharacterIndex = bytes.IndexOfAny(ControlCharacters);

			if (controlCharacterIndex < 0)
				controlCharacterIndex = bytes.Length;

			if (controlCharacterIndex > 0)
			{
				fileDescriptor.Write(bytes.Slice(0, controlCharacterIndex));
				fileDescriptor.Column += controlCharacterIndex;

				bytes = bytes.Slice(controlCharacterIndex);
			}

			if (bytes.Length > 0)
			{
				WriteByte(fd, bytes[0], out lastByteWritten);
				bytes = bytes.Slice(1);
			}
		}
	}
}
