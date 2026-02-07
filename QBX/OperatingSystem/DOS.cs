using System;
using System.Collections.Generic;

using QBX.Hardware;

namespace QBX.OperatingSystem;

public class DOS
{
	public bool IsTerminated = false;

	public List<FileDescriptor?> Files = new List<FileDescriptor?>();

	Machine _machine;

	public const int StandardInput = 0;
	public const int StandardOutput = 1;

	public DOS(Machine machine)
	{
		_machine = machine;

		InitializeStandardInputAndOutput();
	}

	void InitializeStandardInputAndOutput()
	{
		while (Files.Count < 2)
			Files.Add(null);

		Files[0] = new FileDescriptor(ReadStandardInput, null);
		Files[1] = new FileDescriptor(null, WriteStandardOutput) { WriteThrough = true };
	}

	public void Reset()
	{
		// Keep FDs 0 and 1, stdin and stdout.
		for (int i = 0; i < Files.Count; i++)
		{
			if (Files[i] != null)
			{
				// TODO: CloseFile(Files[i]);
			}
		}

		InitializeStandardInputAndOutput();

		// TODO: reset memory allocator
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
		Reset();
		IsTerminated = true;
	}

	public byte ReadByte(int fd, bool echo = false)
	{
		if ((fd < 0) || (fd >= Files.Count)
		 || (Files[fd] is not FileDescriptor fileDescriptor))
			throw new ArgumentException("Invalid file descriptor");

		byte b = fileDescriptor.ReadByte();

		if (b == 13)
			fileDescriptor.Column = 0;
		else
			fileDescriptor.Column++;

		if (echo)
			WriteByte(StandardOutput, b, out _);

		return b;
	}

	public void WriteByte(int fd, byte b, out byte lastByteWritten)
	{
		if ((fd < 0) || (fd >= Files.Count)
		 || (Files[fd] is not FileDescriptor fileDescriptor))
			throw new ArgumentException("Invalid file descriptor");

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
		if ((fd < 0) || (fd >= Files.Count)
		 || (Files[fd] is not FileDescriptor fileDescriptor))
			throw new ArgumentException("Invalid file descriptor");

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
