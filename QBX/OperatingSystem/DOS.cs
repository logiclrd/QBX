using System;
using System.Collections.Generic;
using System.Linq;
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
		//Files.Add(new FileDescriptor(null, WriteStandardOutput));
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

		if (echo)
			_machine.VideoFirmware.VisualLibrary.WriteText(b);

		return b;
	}
}
