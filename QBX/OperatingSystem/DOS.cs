using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

using QBX.Hardware;
using QBX.OperatingSystem.Breaks;
using QBX.OperatingSystem.FileDescriptors;
using QBX.OperatingSystem.Memory;

namespace QBX.OperatingSystem;

public partial class DOS
{
	public bool IsTerminated = false;

	public event Action? Break;

	public DOSError LastError = DOSError.None;

	public const int ManagedMemoryStart = 0x10000;

	public MemoryManager MemoryManager;

	public ushort CurrentPSPSegment;

	public ushort DataTransferAddressSegment;
	public ushort DataTransferAddressOffset;

	public int DataTransferAddress => DataTransferAddressSegment * 0x10 + DataTransferAddressOffset;

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

		MemoryManager = new MemoryManager(
			machine.SystemMemory,
			ManagedMemoryStart,
			machine.SystemMemory.Length - ManagedMemoryStart);

		CurrentPSPSegment = MemoryManager.RootPSPSegment;

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

	public void ClearLastError()
	{
		LastError = DOSError.None;
	}

	bool _suppressExceptions = false;

	class SuppressExceptionsScope(DOS owner) : IDisposable
	{
		bool saved = Interlocked.Exchange(ref owner._suppressExceptions, true);
		public void Dispose() { owner._suppressExceptions = saved; }
	}

	public IDisposable SuppressExceptionsInScope() => new SuppressExceptionsScope(this);

	void TranslateError(Action action)
	{
		try
		{
			ClearLastError();

			action();
		}
		catch (Exception e)
		{
			LastError = e.ToDOSError();
			if (!_suppressExceptions)
				throw;
		}
	}

	TReturn TranslateError<TReturn>(Func<TReturn> action)
	{
		try
		{
			ClearLastError();

			return action();
		}
		catch (Exception e)
		{
			LastError = e.ToDOSError();
			if (!_suppressExceptions)
				throw;

			return default!;
		}
	}

	public void TerminateProgram()
	{
		TranslateError(() =>
		{
			Reset();
			IsTerminated = true;
		});
	}

	public void Reset()
	{
		TranslateError(() =>
		{
			CloseAllFiles(keepStandardHandles: false);
			InitializeStandardInputAndOutput();

			// TODO: reset memory allocator
		});
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
		TranslateError(() =>
		{
			int targetCount = keepStandardHandles ? 2 : 0;

			while (Files.Count > targetCount)
			{
				int fileHandle = Files.Count - 1;

				if (Files[fileHandle] is FileDescriptor fileDescriptor)
					fileDescriptor.Close();

				Files.RemoveAt(Files.Count - 1);
			}
		});
	}

	public void FlushAllBuffers()
	{
		TranslateError(() =>
		{
			foreach (var file in Files.OfType<FileDescriptor>())
				file.FlushWriteBuffer();
		});
	}

	public void Beep()
	{
		_machine.Speaker.ChangeSound(true, false, frequency: 1000, false, hold: TimeSpan.FromMilliseconds(200));
		_machine.Speaker.ChangeSound(false, false, frequency: 1000, false);
	}

	public void FlushStandardInput()
	{
		TranslateError(() =>
		{
			if (Files[StandardInput] is FileDescriptor fileDescriptor)
				fileDescriptor.ReadBuffer.Free(fileDescriptor.ReadBuffer.NumUsed);

			_machine.Keyboard.DiscardQueueudInput();
		});
	}

	public byte ReadByte(int fileHandle, bool echo = false)
	{
		if ((fileHandle < 0) || (fileHandle >= Files.Count)
			|| (Files[fileHandle] is not FileDescriptor fileDescriptor))
		{
			LastError = DOSError.InvalidHandle;
			throw new ArgumentException("Invalid file descriptor");
		}

		return TranslateError(() =>
		{
			byte b = fileDescriptor.ReadByte();

			if (_enableBreak && (fileHandle == StandardInput) && (b == 3))
				throw new Break();
			else if (b == 13)
				fileDescriptor.Column = 0;
			else
				fileDescriptor.Column++;

			if (echo)
				WriteByte(StandardOutput, b, out _);

			return b;
		});
	}

	public bool TryReadByte(int fileHandle, out byte byteValue)
	{
		byte b = default;

		if ((fileHandle < 0) || (fileHandle >= Files.Count)
			|| (Files[fileHandle] is not FileDescriptor fileDescriptor))
		{
			LastError = DOSError.InvalidHandle;
			throw new ArgumentException("Invalid file descriptor");
		}

		bool ret = TranslateError(() =>
		{
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
		});

		byteValue = b;

		return ret;
	}

	public void WriteByte(int fileHandle, byte b, out byte lastByteWritten)
	{
		lastByteWritten = (b == 9) ? (byte)32 : b; // Tabs get expanded to spaces.

		if ((fileHandle < 0) || (fileHandle >= Files.Count)
			|| (Files[fileHandle] is not FileDescriptor fileDescriptor))
		{
			LastError = DOSError.InvalidHandle;
			throw new ArgumentException("Invalid file descriptor");
		}

		TranslateError(() =>
		{
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
				do
				{
					fileDescriptor.WriteByte(32);
					fileDescriptor.Column++;
				} while ((fileDescriptor.Column & 7) != 0);
			}
		});
	}

	readonly byte[] ControlCharacters = [9, 13];

	public void Write(int fileHandle, ReadOnlySpan<byte> bytes, out byte lastByteWritten)
	{
		byte b = default;

		if ((fileHandle < 0) || (fileHandle >= Files.Count)
			|| (Files[fileHandle] is not FileDescriptor fileDescriptor))
		{
			LastError = DOSError.InvalidHandle;
			throw new ArgumentException("Invalid file descriptor");
		}

		lastByteWritten = 0;

		while (bytes.Length > 0)
		{
			int controlCharacterIndex = bytes.IndexOfAny(ControlCharacters);

			if (controlCharacterIndex < 0)
				controlCharacterIndex = bytes.Length;

			if (controlCharacterIndex > 0)
			{
				try
				{
					fileDescriptor.Write(bytes.Slice(0, controlCharacterIndex));
					fileDescriptor.Column += controlCharacterIndex;

					bytes = bytes.Slice(controlCharacterIndex);
				}
				catch (Exception e)
				{
					LastError = e.ToDOSError();
				}
			}

			if (bytes.Length > 0)
			{
				WriteByte(fileHandle, bytes[0], out b);
				bytes = bytes.Slice(1);
			}
		}
	}

	public void SetDefaultDrive(char driveLetter)
	{
		TranslateError(() =>
		{
			Directory.SetCurrentDirectory(driveLetter + ":");
		});
	}

	public int GetLogicalDriveCount()
	{
		return TranslateError(() =>
		{
			return DriveInfo.GetDrives().Length;
		});
	}

	int GetFreeFileHandle()
	{
		for (int i = 2; i < Files.Count; i++)
			if (Files[i] is null)
				return i;

		Files.Add(null);

		return Files.Count - 1;
	}

	int AllocateFileHandleForOpenFile(FileStream stream)
	{
		int fileHandle = GetFreeFileHandle();

		if (fileHandle < 0)
			return fileHandle;

		Files[fileHandle] = new RegularFileDescriptor(stream);

		return fileHandle;
	}

	public int OpenFile(FileControlBlock fcb, FileMode openMode)
	{
		return TranslateError(() =>
		{
			string fileName = fcb.GetFileName();

			if (!string.IsNullOrWhiteSpace(fileName))
			{
				if (fcb.DriveIdentifier != 0)
					fileName = fcb.GetDriveLetter() + fileName;

				try
				{
					fileName = ShortFileNames.Unmap(Path.GetFullPath(fileName));

					var fileInfo = new FileInfo(fileName);

					if (fileInfo.Length <= uint.MaxValue)
					{
						var dateTime = fileInfo.LastWriteTime;

						fcb.DriveIdentifier = (byte)(fileInfo.FullName[0] - 64);
						fcb.FileSize = (uint)fileInfo.Length;
						fcb.DateStamp.Set(dateTime.Year, dateTime.Month, dateTime.Day);
						fcb.TimeStamp.Set(dateTime.Hour, dateTime.Minute, dateTime.Second);
						fcb.RecordSize = 128;

						var stream = fileInfo.Open(openMode);

						return AllocateFileHandleForOpenFile(stream);
					}
				}
				catch { }
			}

			return -1;
		});
	}

	public int OpenFile(string fileName, FileMode openMode)
	{
		try
		{
			return TranslateError(() =>
			{
				fileName = ShortFileNames.Unmap(fileName);

				var stream = new FileStream(fileName, openMode, FileAccess.ReadWrite, FileShare.ReadWrite);

				return AllocateFileHandleForOpenFile(stream);
			});
		}
		catch
		{
			return -1;
		}
	}

	public int ReadRecord(FileControlBlock fcb)
	{
		return TranslateError(() =>
		{
			int offset = fcb.RecordPointer * fcb.RecordSize;

			if (offset >= fcb.FileSize)
				return 0;
			else
			{
				if (0x10000 - DataTransferAddressOffset < fcb.RecordSize)
					throw new OperationCanceledException("DTA overlaps segment boundary");

				if (Files[fcb.FileHandle] is not FileDescriptor fileDescriptor)
				{
					LastError = DOSError.InvalidHandle;
					return -1;
				}

				int readSize = fcb.RecordSize;

				if (offset + readSize > fcb.FileSize)
					readSize = (int)(fcb.FileSize - offset);

				fileDescriptor.Seek(offset);

				if (fileDescriptor.ReadExactly(readSize, Machine.SystemMemory, DataTransferAddress))
				{
					fcb.CurrentRecordNumber++;

					if (fcb.CurrentRecordNumber == 128)
					{
						fcb.CurrentRecordNumber = 0;
						fcb.CurrentBlockNumber++;
					}
				}

				return readSize;
			}
		});
	}


	public int WriteRecord(FileControlBlock fcb)
	{
		return TranslateError(() =>
		{
			int offset = fcb.RecordPointer * fcb.RecordSize;

			if (offset >= fcb.FileSize)
				return 0;
			else
			{
				if (0x10000 - DataTransferAddressOffset < fcb.RecordSize)
					throw new OperationCanceledException("DTA overlaps segment boundary");

				if (Files[fcb.FileHandle] is not FileDescriptor fileDescriptor)
				{
					LastError = DOSError.InvalidHandle;
					return -1;
				}

				fileDescriptor.Seek(offset);

				fileDescriptor.Write(fcb.RecordSize, Machine.SystemMemory, DataTransferAddress);

				fcb.CurrentRecordNumber++;

				if (fcb.CurrentRecordNumber == 128)
				{
					fcb.CurrentRecordNumber = 0;
					fcb.CurrentBlockNumber++;
				}

				return fcb.RecordSize;
			}
		});
	}

	public bool CloseFile(FileControlBlock fcb)
	{
		return TranslateError(() =>
		{
			if (fcb.FileHandle != 0)
			{
				bool result = CloseFile(fcb.FileHandle);

				fcb.FileHandle = 0;

				return result;
			}

			return false;
		});
	}

	public bool CloseFile(int fileHandle)
	{
		if ((fileHandle < 0) || (fileHandle >= Files.Count)
		 || (Files[fileHandle] is not FileDescriptor fileDescriptor))
		{
			LastError = DOSError.InvalidHandle;
			return false;
		}

		return TranslateError(() =>
		{
			fileDescriptor.Close();

			Files[fileHandle] = null;

			return true;
		});
	}

	public void DeleteFile(FileControlBlock fcb)
	{
		string fileName = fcb.GetFileName();

		if (!string.IsNullOrWhiteSpace(fileName))
		{
			if (fcb.DriveIdentifier != 0)
				fileName = fcb.GetDriveLetter() + fileName;

			DeleteFile(Path.GetFullPath(fileName));
		}
	}

	public void DeleteFile(string fileName)
	{
		TranslateError(() =>
		{
			fileName = ShortFileNames.Unmap(fileName);

			File.Delete(fileName);
		});
	}

	public void RenameFiles(RenameFileControlBlock rfcb)
	{
		TranslateError(() =>
		{
			string fileNamePattern = rfcb.GetOldFileName();

			string fileNamePart = Path.GetFileNameWithoutExtension(fileNamePattern);
			string extensionPart = Path.GetExtension(fileNamePattern);

			string collapsedFileNamePart = NormalizeFileSearchPattern(ref fileNamePart, 8);
			string collapsedExtensionPart = NormalizeFileSearchPattern(ref extensionPart, 3);

			fileNamePattern = fileNamePart + "." + extensionPart;

			rfcb.SetOldFileName(fileNamePattern);

			string collapsedFileNamePattern = collapsedFileNamePart + "." + collapsedExtensionPart;

			RenameFileControlBlock rename = new RenameFileControlBlock();

			void PerformRename(FileSystemInfo fileSystemInfo, string shortName, byte searchAttributeByte)
			{
				rename.SetOldFileName(shortName);

				for (int i = 0; i < 11; i++)
				{
					if (rfcb.NewFileNameBytes[i] == '?')
						rename.NewFileNameBytes[i] = rename.OldFileNameBytes[i];
					else
						rename.NewFileNameBytes[i] = rfcb.NewFileNameBytes[i];
				}

				string oldFullPath = fileSystemInfo.FullName;

				ShortFileNames.Forget(oldFullPath);

				string newFullPath = Path.Combine(
					Path.GetDirectoryName(oldFullPath) ?? ".",
					rename.GetNewFileName());

				if (fileSystemInfo is FileInfo fileInfo)
					fileInfo.MoveTo(newFullPath);
				else if (fileSystemInfo is DirectoryInfo dirInfo)
					dirInfo.MoveTo(newFullPath);
				else
					throw new FileNotFoundException();
			}

			bool success = FindFirst(
				collapsedFileNamePattern,
				default,
				PerformRename,
				out var search);

			if (success)
			{
				while (FindNext(search, default, PerformRename))
					;
			}
		});
	}
}
