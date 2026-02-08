using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading;

using QBX.Hardware;
using QBX.OperatingSystem.Breaks;
using QBX.OperatingSystem.FileDescriptors;

namespace QBX.OperatingSystem;

public partial class DOS
{
	public bool IsTerminated = false;

	public event Action? Break;

	public const int StandardInput = 0;
	public const int StandardOutput = 1;

	public List<FileDescriptor?> Files = new List<FileDescriptor?>();

	StandardInputFileDescriptor _stdin;
	StandardOutputFileDescriptor _stdout;

	public Machine Machine => _machine;

	Machine _machine;

	public DOS(Machine machine)
	{
		_machine = machine;

		_machine.Keyboard.Break += OnBreak;

		InitializeStandardInputAndOutput();
	}

	public bool BreakEnabled => _enableBreak;

	bool _enableBreak = false;

	void OnBreak()
	{
		if (_enableBreak)
			Break?.Invoke();
	}

	class BreakScope(DOS owner) : IDisposable
	{
		bool saved = Interlocked.Exchange(ref owner._enableBreak, true);
		public void Dispose() { owner._enableBreak = saved; }
	}

	public IDisposable EnableBreak() => new BreakScope(this);

	internal CancellationScope CancelOnBreak() => new CancellationScope(this);

	public void TerminateProgram()
	{
		Reset();
		IsTerminated = true;
	}

	public void Reset()
	{
		CloseAllFiles(keepStandardHandles: false);
		InitializeStandardInputAndOutput();

		// TODO: reset memory allocator
	}

	[MemberNotNull(nameof(_stdin)), MemberNotNull(nameof(_stdout))]
	void InitializeStandardInputAndOutput()
	{
		while (Files.Count < 2)
			Files.Add(null);

		Files[0] = _stdin = new StandardInputFileDescriptor(this);
		Files[1] = _stdout = new StandardOutputFileDescriptor(this);
	}

	public void CloseAllFiles(bool keepStandardHandles)
	{
		int targetCount = keepStandardHandles ? 2 : 0;

		while (Files.Count > targetCount)
		{
			int fd = Files.Count - 1;

			if (Files[fd] != null)
			{
				// TODO: CloseFile(Files[fd]);
			}

			Files.RemoveAt(Files.Count - 1);
		}
	}

	public void FlushAllBuffers()
	{
		foreach (var file in Files.OfType<FileDescriptor>())
			file.FlushWriteBuffer();
	}

	public void Beep()
	{
		_machine.Speaker.ChangeSound(true, false, frequency: 1000, false, hold: TimeSpan.FromMilliseconds(200));
		_machine.Speaker.ChangeSound(false, false, frequency: 1000, false);
	}

	public void FlushStandardInput()
	{
		if (Files[StandardInput] is FileDescriptor fileDescriptor)
			fileDescriptor.ReadBuffer.Free(fileDescriptor.ReadBuffer.NumUsed);

		_machine.Keyboard.DiscardQueueudInput();
	}

	public byte ReadByte(int fd, bool echo = false)
	{
		if ((fd < 0) || (fd >= Files.Count)
		 || (Files[fd] is not FileDescriptor fileDescriptor))
			throw new ArgumentException("Invalid file descriptor");

		byte b = fileDescriptor.ReadByte();

		if (_enableBreak && (fd == StandardInput) && (b == 3))
			throw new Break();
		else if (b == 13)
			fileDescriptor.Column = 0;
		else
			fileDescriptor.Column++;

		if (echo)
			WriteByte(StandardOutput, b, out _);

		return b;
	}

	public bool TryReadByte(int fd, out byte b)
	{
		if ((fd < 0) || (fd >= Files.Count)
		 || (Files[fd] is not FileDescriptor fileDescriptor))
			throw new ArgumentException("Invalid file descriptor");

		_stdin.WouldHaveBlocked = false;

		using (fileDescriptor.NonBlocking())
			b = fileDescriptor.ReadByte();

		if (fileDescriptor.WouldHaveBlocked)
			return false;
		else
		{
			if (b == 13)
				fileDescriptor.Column = 0;
			else
				fileDescriptor.Column++;

			return true;
		}
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

	public void SetDefaultDrive(char driveLetter)
	{
		Directory.SetCurrentDirectory(driveLetter + ":");
	}

	public int GetLogicalDriveCount()
	{
		return DriveInfo.GetDrives().Length;
	}
}
