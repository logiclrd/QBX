using System;
using System.Collections.Generic;
using System.Threading;

using QBX.Hardware;

namespace QBX.OperatingSystem;

public class DOS
{
	public bool IsTerminated = false;

	public event Action? Break;

	public List<FileDescriptor?> Files = new List<FileDescriptor?>();

	Machine _machine;

	public const int StandardInput = 0;
	public const int StandardOutput = 1;

	public DOS(Machine machine)
	{
		_machine = machine;

		_machine.Keyboard.Break += OnBreak;

		InitializeStandardInputAndOutput();
	}

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

	class CancellationScope : IDisposable
	{
		DOS _owner;
		CancellationTokenSource _cancellationTokenSource;
		bool _breakReceived;

		public CancellationToken Token => _cancellationTokenSource.Token;
		public bool BreakReceived => _breakReceived;

		public CancellationScope(DOS owner)
		{
			_cancellationTokenSource = new CancellationTokenSource();

			_owner = owner;
			_owner.Break += owner_Break;
		}

		public void Dispose()
		{
			_owner.Break -= owner_Break;
		}

		private void owner_Break()
		{
			_breakReceived = true;

			try
			{
				_cancellationTokenSource.Cancel();
			}
			catch { }
		}
	}

	CancellationScope CancelOnBreak() => new CancellationScope(this);

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

	public void Beep()
	{
		_machine.Speaker.ChangeSound(true, false, frequency: 1000, false, hold: TimeSpan.FromMilliseconds(200));
		_machine.Speaker.ChangeSound(false, false, frequency: 1000, false);
	}

	bool _blockingInput = true;
	bool _wouldHaveBlocked = false;

	class NonBlockingScope(DOS owner) : IDisposable
	{
		bool saved = Interlocked.Exchange(ref owner._blockingInput, false);
		public void Dispose() => owner._blockingInput = saved;
	}

	IDisposable StandardInputNonBlocking() => new NonBlockingScope(this);

	void ReadStandardInput(FileBuffer readBuffer)
	{
		do
		{
			if (_blockingInput)
			{
				if (!_enableBreak)
					_machine.Keyboard.WaitForInput();
				else
				{
					using (var scope = CancelOnBreak())
					{
						_machine.Keyboard.WaitForInput(scope.Token);

						if (scope.BreakReceived)
							throw new Break();
					}
				}
			}

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

				return;
			}
		} while (_blockingInput);

		_wouldHaveBlocked = true;
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

		_wouldHaveBlocked = false;

		using (StandardInputNonBlocking())
			b = fileDescriptor.ReadByte();

		if (_wouldHaveBlocked)
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
}
