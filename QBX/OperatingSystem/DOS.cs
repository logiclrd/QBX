using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

using QBX.ExecutionEngine.Execution;
using QBX.Hardware;
using QBX.OperatingSystem.Breaks;
using QBX.OperatingSystem.FileDescriptors;
using QBX.OperatingSystem.FileStructures;
using QBX.OperatingSystem.Memory;

using FileMode = QBX.OperatingSystem.FileStructures.FileMode;
using FileAccess = QBX.OperatingSystem.FileStructures.FileAccess;
using FileAttributes = QBX.OperatingSystem.FileStructures.FileAttributes;

using SystemFileMode = System.IO.FileMode;
using SystemFileAccess = System.IO.FileAccess;
using SystemFileShare = System.IO.FileShare;
using SystemFileAttributes = System.IO.FileAttributes;

namespace QBX.OperatingSystem;

public partial class DOS
{
	public bool IsTerminated = false;

	public event Action? Break;

	public DOSError LastError = DOSError.None;

	public const int ManagedMemoryStart = 0x10000;

	public MemoryManager MemoryManager;

	public ushort CurrentPSPSegment;

	public CultureInfo CurrentCulture = CultureInfo.CurrentCulture;

	public ushort DataTransferAddressSegment;
	public ushort DataTransferAddressOffset;

	public int DataTransferAddress => DataTransferAddressSegment * 0x10 + DataTransferAddressOffset;

	public const int StandardInput = 0;
	public const int StandardOutput = 1;

	public SegmentedAddress InDOSFlagAddress;
	public SegmentedAddress FirstDriveParameterBlockAddress;

	public List<FileDescriptor?> Files = new List<FileDescriptor?>();

	public bool VerifyWrites = false;

	public Devices Devices;

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

		InDOSFlagAddress = MemoryManager.AllocateMemory(length: 4, MemoryManager.RootPSPSegment);

		GenerateDiskParameterBlocks();

		InitializeDevices();
	}

	private void GenerateDiskParameterBlocks()
	{
		var drives = DriveInfo.GetDrives();

		int bytesNeeded = drives.Length * DriveParameterBlock.Size;

		int linearAddress = MemoryManager.AllocateMemory(bytesNeeded, MemoryManager.RootPSPSegment);

		FirstDriveParameterBlockAddress = new SegmentedAddress(linearAddress);

		var nextDPBAddress = FirstDriveParameterBlockAddress;

		foreach (var drive in drives)
		{
			ref DriveParameterBlock dpb = ref DriveParameterBlock.CreateReference(_machine.SystemMemory, nextDPBAddress.ToLinearAddress());

			// This is all nonsense data, because modern drives don't
			// fit into the fields described by a DPB at all. :-P
			dpb.DriveIdentifier = (byte)(char.ToUpperInvariant(drive.RootDirectory.FullName[0]) - 'A');
			dpb.SectorSize = 512;
			dpb.ClusterMask = 255; // ClusterMask == (1 << ClusterShift) - 1
			dpb.ClusterShift = 8;
			dpb.FirstFAT = 1;
			dpb.FATCount = 2;
			dpb.RootEntries = 2;
			dpb.FirstSector = 256;
			dpb.MaxCluster = 0xFFFF;
			dpb.SectorsPerFAT = 0xFFFF;
			dpb.DirectorySector = 256;
			dpb.DeviceDriverAddress = 0xFFFFFFFF;
			dpb.MediaDescriptor = drive.DriveType == DriveType.Removable ? MediaDescriptor.FloppyDisk : MediaDescriptor.FixedDisk;
			dpb.FirstAccess = 0;
			dpb.NextFreeCluster = 0xFFFF;
			dpb.FreeClusterCount = 0xFFFF;

			nextDPBAddress.Offset += DriveParameterBlock.Size;

			if (nextDPBAddress.Offset >= bytesNeeded)
				nextDPBAddress = 0;

			dpb.NextDPBAddress = nextDPBAddress;
		}
	}

	delegate bool EnumerateDPBCallback(SegmentedAddress segmentedAddress, ref DriveParameterBlock dpb);

	void EnumerateDriveParameterBlocks(EnumerateDPBCallback callback)
	{
		var address = FirstDriveParameterBlockAddress;

		while (address != 0)
		{
			ref var dpb = ref DriveParameterBlock.CreateReference(_machine.SystemMemory, address.ToLinearAddress());

			bool @continue = callback(address, ref dpb);

			if (!@continue)
				break;

			address = dpb.NextDPBAddress;
		}
	}

	class InDOSScope : IDisposable
	{
		DOS _owner;

		ref int InDOSFlag => ref MemoryMarshal.Cast<byte, int>(
			_owner.Machine.SystemMemory.AsSpan().Slice(
				_owner.InDOSFlagAddress.ToLinearAddress(), sizeof(int)))[0];

		public InDOSScope(DOS owner)
		{
			_owner = owner;

			Interlocked.Increment(ref InDOSFlag);
		}

		public void Dispose()
		{
			Interlocked.Decrement(ref InDOSFlag);
		}
	}

	public IDisposable InDOS() => new InDOSScope(this);

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
			InitializeDevices();

			// TODO: reset memory allocator
		});
	}

	[MemberNotNull(nameof(Devices))]
	void InitializeDevices()
	{
		Devices = new Devices(this);

		while (Files.Count < 2)
			Files.Add(null);

		Files[0] = Files[1] = Devices.Console;
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
			fileDescriptor.WouldHaveBlocked = false;

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

			if (VerifyWrites)
				fileDescriptor.FlushWriteBuffer(true);
		});
	}

	public int Read(int fileHandle, IMemory systemMemory, int address, int count)
	{
		if ((fileHandle < 0) || (fileHandle >= Files.Count)
			|| (Files[fileHandle] is not FileDescriptor fileDescriptor))
		{
			LastError = DOSError.InvalidHandle;
			throw new ArgumentException("Invalid file descriptor");
		}

		int numRead = 0;

		try
		{
			while (numRead < count)
			{
				systemMemory[address++] = fileDescriptor.ReadByte();
				numRead++;
			}
		}
		catch (EndOfStreamException) { }

		return numRead;
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

			if (VerifyWrites)
				fileDescriptor.FlushWriteBuffer(true);
		}
	}

	public int Write(int fileHandle, IMemory systemMemory, int address, int count)
	{
		byte b = default;

		if ((fileHandle < 0) || (fileHandle >= Files.Count)
			|| (Files[fileHandle] is not FileDescriptor fileDescriptor))
		{
			LastError = DOSError.InvalidHandle;
			throw new ArgumentException("Invalid file descriptor");
		}

		int numWritten = 0;

		while (numWritten < count)
		{
			WriteByte(fileHandle, systemMemory[address++], out b);
			count--;
		}

		if (VerifyWrites)
			fileDescriptor.FlushWriteBuffer(true);

		return numWritten;
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

	public SegmentedAddress GetDriveParameterBlock(int driveIdentifier)
	{
		return TranslateError(() =>
		{
			if (!TryGetDriveParameterBlock(driveIdentifier, out var address))
				throw new DOSException(DOSError.InvalidDrive);

			return address;
		});
	}

	public bool TryGetDriveParameterBlock(int driveIdentifier, out SegmentedAddress address)
	{
		SegmentedAddress ret = 0;

		EnumerateDriveParameterBlocks(
			(address, ref dpb) =>
			{
				if (dpb.DriveIdentifier == driveIdentifier)
				{
					ret = address;
					return false;
				}

				return true;
			});

		address = ret;

		return true;
	}

	public SegmentedAddress GetDefaultDriveParameterBlock()
	{
		int defaultDrive = char.ToUpperInvariant(Environment.CurrentDirectory[0]) - 'A';

		return GetDriveParameterBlock(defaultDrive);
	}

	public void CreateDirectory(StringValue directoryName)
		=> CreateDirectory(directoryName.ToString());

	public void CreateDirectory(string directoryName)
	{
		TranslateError(() =>
		{
			Directory.CreateDirectory(directoryName);
		});
	}

	public void RemoveDirectory(StringValue directoryName)
		=> RemoveDirectory(directoryName.ToString());

	public void RemoveDirectory(string directoryName)
	{
		TranslateError(() =>
		{
			Directory.Delete(directoryName, recursive: false);
		});
	}

	public void ChangeCurrentDirectory(StringValue directoryName)
		=> ChangeCurrentDirectory(directoryName.ToString());

	public void ChangeCurrentDirectory(string directoryName)
	{
		TranslateError(() =>
		{
			Environment.CurrentDirectory = directoryName;
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

	int AllocateFileHandleForOpenFile(string path, FileStream stream)
	{
		return AllocateFileHandleForFileDescriptor(new RegularFileDescriptor(path, stream));
	}

	int AllocateFileHandleForFileDescriptor(FileDescriptor fileDescriptor)
	{
		int fileHandle = GetFreeFileHandle();

		if (fileHandle < 0)
			return fileHandle;

		Files[fileHandle] = fileDescriptor;

		return fileHandle;
	}

	public bool PopulateFileInfo(FileControlBlock fcb)
	{
		return TranslateError(() =>
		{
			string fileName = fcb.GetFileName();

			if (!File.Exists(fileName))
			{
				LastError = DOSError.FileNotFound;
				return false;
			}

			var info = new FileInfo(fileName);

			if (info.Length > uint.MaxValue)
			{
				LastError = DOSError.InvalidFunction;
				return false;
			}

			var numRecords = info.Length / fcb.RecordSize;

			fcb.FileSize = unchecked((uint)info.Length);
			fcb.RandomRecordNumber = unchecked((uint)numRecords);
			fcb.DateStamp.Set(info.LastWriteTime);
			fcb.TimeStamp.Set(info.LastWriteTime);

			return true;
		});
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

					if (!ShortFileNames.TryMap(fileName, out var shortPath))
						return -1;

					if (Devices.TryGetDeviceByName(Path.GetFileNameWithoutExtension(fileName), out var device))
					{
						fcb.FileSize = 0;
						fcb.DateStamp = default;
						fcb.TimeStamp = default;
						fcb.RecordSize = 128;
						fcb.FileHandle = AllocateFileHandleForFileDescriptor(device);

						return fcb.FileHandle;
					}
					else
					{
						var fileInfo = new FileInfo(fileName);

						if (fileInfo.Length <= uint.MaxValue)
						{
							var dateTime = fileInfo.LastWriteTime;

							fcb.DriveIdentifier = (byte)(fileInfo.FullName[0] - 64);
							fcb.FileSize = (uint)fileInfo.Length;
							fcb.DateStamp.Set(dateTime.Year, dateTime.Month, dateTime.Day);
							fcb.TimeStamp.Set(dateTime.Hour, dateTime.Minute, dateTime.Second);
							fcb.RecordSize = 128;

							var stream = fileInfo.Open(openMode.ToSystemFileMode());

							fcb.FileHandle = AllocateFileHandleForOpenFile(shortPath, stream);

							return fcb.FileHandle;
						}
					}
				}
				catch { }
			}

			return -1;
		});
	}

	public int OpenFile(string fileName, FileMode openMode, FileAccess accessModes)
	{
		try
		{
			return TranslateError(() =>
			{
				fileName = ShortFileNames.Unmap(fileName);

				if (!ShortFileNames.TryMap(fileName, out var shortPath))
					return -1;

				var systemFileMode = openMode.ToSystemFileMode();
				var systemFileAccess = default(SystemFileAccess);
				var systemFileShare = default(SystemFileShare);

				switch (accessModes & FileAccess.AccessMask)
				{
					case FileAccess.Access_ReadWrite: systemFileAccess = SystemFileAccess.ReadWrite; break;
					case FileAccess.Access_WriteOnly: systemFileAccess = SystemFileAccess.Read; break;
					case FileAccess.Access_ReadOnly: systemFileAccess = SystemFileAccess.Write; break;
				}

				switch (accessModes & FileAccess.ShareMask)
				{
					case FileAccess.Share_DenyReadWrite: systemFileShare = SystemFileShare.None; break;
					case FileAccess.Share_DenyRead: systemFileShare = SystemFileShare.Write; break;
					case FileAccess.Share_DenyWrite: systemFileShare = SystemFileShare.Read; break;
				}

				if ((accessModes & FileAccess.Flags_NoInherit) == 0)
					systemFileShare |= SystemFileShare.Inheritable;

				if (Devices.TryGetDeviceByName(Path.GetFileNameWithoutExtension(fileName), out var device))
					return AllocateFileHandleForFileDescriptor(device);
				else
				{
					var stream = new FileStream(fileName, openMode.ToSystemFileMode(), systemFileAccess, systemFileShare);

					return AllocateFileHandleForOpenFile(shortPath, stream);
				}
			});
		}
		catch
		{
			return -1;
		}
	}

	public FileAttributes GetFileAttributes(string relativePath)
	{
		return TranslateError(() =>
		{
			relativePath = ShortFileNames.Unmap(relativePath);

			return File.GetAttributes(relativePath).ToDOSFileAttributes();
		});
	}

	public void SetFileAttributes(string relativePath, FileAttributes attributes)
	{
		TranslateError(() =>
		{
			relativePath = ShortFileNames.Unmap(relativePath);

			File.SetAttributes(relativePath, attributes.ToSystemFileAttributes());
		});
	}

	public void SetFileAttributes(int fileHandle, FileAttributes attributes)
	{
		if ((fileHandle < 0) || (fileHandle >= Files.Count)
		 || (Files[fileHandle] is not FileDescriptor fileDescriptor))
		{
			LastError = DOSError.InvalidHandle;
			return;
		}

		TranslateError(() =>
		{
			fileDescriptor.SetAttributes(attributes);
		});
	}

	void Advance(FileControlBlock fcb)
	{
		fcb.CurrentRecordNumber++;

		if (fcb.CurrentRecordNumber == 128)
		{
			fcb.CurrentRecordNumber = 0;
			fcb.CurrentBlockNumber = unchecked((ushort)(fcb.CurrentBlockNumber + 1));
		}
	}

	public int ReadRecord(FileControlBlock fcb, bool advance = false)
		=> ReadRecords(fcb, recordCount: 1, advance, updateRandomRecordNumber: false);

	public int ReadRecords(FileControlBlock fcb, int recordCount)
		=> ReadRecords(fcb, recordCount, advance: true, updateRandomRecordNumber: true);

	public int ReadRecords(FileControlBlock fcb, int recordCount, bool advance, bool updateRandomRecordNumber)
	{
		return TranslateError(() =>
		{
			fcb.CurrentRecordNumber &= 127;

			uint offset = fcb.RecordPointer * fcb.RecordSize;

			if (offset >= fcb.FileSize)
				return 0;
			else
			{
				int readSize = fcb.RecordSize * recordCount;

				if (0x10000 - DataTransferAddressOffset < readSize)
					throw new OperationCanceledException("DTA overlaps segment boundary");

				if (Files[fcb.FileHandle] is not FileDescriptor fileDescriptor)
				{
					LastError = DOSError.InvalidHandle;
					return -1;
				}

				if (offset + readSize > fcb.FileSize)
					readSize = (int)(fcb.FileSize - offset);

				fileDescriptor.Seek(offset, MoveMethod.FromBeginning);

				if (fileDescriptor.ReadExactly(readSize, Machine.MemoryBus, DataTransferAddress))
				{
					if (advance)
					{
						Advance(fcb);

						if (updateRandomRecordNumber)
							fcb.RandomRecordNumber = fcb.RecordPointer;
					}
				}

				return readSize;
			}
		});
	}

	public int WriteRecord(FileControlBlock fcb, bool advance = false)
		=> WriteRecords(fcb, recordCount: 1, advance, updateRandomRecordNumber: false);

	public int WriteRecords(FileControlBlock fcb, int recordCount)
		=> WriteRecords(fcb, recordCount, advance: true, updateRandomRecordNumber: true);

	public int WriteRecords(FileControlBlock fcb, int recordCount, bool advance, bool updateRandomRecordNumber)
	{
		return TranslateError(() =>
		{
			uint offset = fcb.RecordPointer * fcb.RecordSize;

			if (offset >= fcb.FileSize)
				return 0;
			else
			{
				int writeSize = fcb.RecordSize * recordCount;

				if (0x10000 - DataTransferAddressOffset < writeSize)
					throw new OperationCanceledException("DTA overlaps segment boundary");

				if (Files[fcb.FileHandle] is not FileDescriptor fileDescriptor)
				{
					LastError = DOSError.InvalidHandle;
					return -1;
				}

				fileDescriptor.Seek(offset, MoveMethod.FromBeginning);

				for (int i = 0; i < recordCount; i++)
				{
					fileDescriptor.Write(fcb.RecordSize, Machine.MemoryBus, DataTransferAddress);

					if (advance)
					{
						Advance(fcb);

						if (updateRandomRecordNumber)
							fcb.RandomRecordNumber = fcb.RecordPointer;
					}
				}

				if (VerifyWrites)
					fileDescriptor.FlushWriteBuffer(true);

				return writeSize;
			}
		});
	}

	public uint SeekFile(int fileHandle, int offset, MoveMethod moveMethod)
	{
		if ((fileHandle < 0) || (fileHandle >= Files.Count)
		 || (Files[fileHandle] is not FileDescriptor fileDescriptor))
		{
			LastError = DOSError.InvalidHandle;
			return uint.MaxValue;
		}

		return TranslateError(() =>
		{
			return fileDescriptor.Seek(offset, moveMethod);
		});
	}

	public uint SeekFile(int fileHandle, uint offset, MoveMethod moveMethod)
	{
		if ((fileHandle < 0) || (fileHandle >= Files.Count)
		 || (Files[fileHandle] is not FileDescriptor fileDescriptor))
		{
			LastError = DOSError.InvalidHandle;
			return uint.MaxValue;
		}

		return TranslateError(() =>
		{
			return fileDescriptor.Seek(offset, moveMethod);
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

			ShortFileNames.Forget(fileName);
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
