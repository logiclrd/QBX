using System;
using System.Globalization;

using QBX.ExecutionEngine.Execution;
using QBX.Firmware.Fonts;
using QBX.Hardware;
using QBX.OperatingSystem;
using QBX.OperatingSystem.Breaks;
using QBX.OperatingSystem.FileDescriptors;
using QBX.OperatingSystem.FileStructures;
using QBX.OperatingSystem.Globalization;
using QBX.OperatingSystem.Memory;
using QBX.OperatingSystem.Processes;

using Path = System.IO.Path;
using File = System.IO.File;
using BinaryReader = System.IO.BinaryReader;
using BinaryWriter = System.IO.BinaryWriter;

namespace QBX.Interrupts;

// Based on the MS-DOS Programmer's Reference manual for MS-DOS 5.0
// with gaps filled in by Ralf Brown's Interrupt List: https://www.ctyme.com/intr/int-21.htm

public class Interrupt0x21(Machine machine) : InterruptHandler
{
	public enum Function : byte
	{
		TerminateProgram = 0x00,
		ReadKeyboardWithEcho = 0x01,
		DisplayCharacter = 0x02,
		AuxiliaryInput = 0x03,
		AuxiliaryOutput = 0x04,
		PrintCharacter = 0x05,
		DirectConsoleIO = 0x06,
		DirectConsoleInput = 0x07,
		ReadKeyboardWithoutEcho = 0x08,
		DisplayString = 0x09,
		BufferedKeyboardInput = 0x0A,
		CheckKeyboardStatus = 0x0B,
		FlushBufferReadKeyboard = 0x0C,
		ResetDrive = 0x0D, // does not reset any drives, but does flush all write buffers
		SetDefaultDrive = 0x0E,
		OpenFileWithFCB = 0x0F,
		CloseFileWithFCB = 0x10,
		FindFirstFileWithFCB = 0x11,
		FindNextFileWithFCB = 0x12,
		DeleteFileWithFCB = 0x13,
		SequentialRead = 0x14,
		SequentialWrite = 0x15,
		CreateFileWithFCB = 0x16,
		RenameFileWithFCB = 0x17,
		GetDefaultDrive = 0x19,
		SetDiskTransferAddress = 0x1A,
		GetDefaultDriveData = 0x1B,
		GetDriveData = 0x1C,
		GetDefaultDPB = 0x1F,
		RandomRead = 0x21,
		RandomWrite = 0x22,
		GetFileSize = 0x23,
		SetRandomRecordNumber = 0x24,
		SetInterruptVector = 0x25, // not implemented
		CreateNewPSP = 0x26, // not implemented
		RandomBlockRead = 0x27,
		RandomBlockWrite = 0x28,
		ParseFilename = 0x29,
		GetDate = 0x2A,
		SetDate = 0x2B,
		GetTime = 0x2C,
		SetTime = 0x2D,
		SetResetVerifyFlag = 0x2E,
		GetDiskTransferAddress = 0x2F,
		GetVersionNumber = 0x30,
		KeepProgram = 0x31,
		GetDPB = 0x32,
		Function33 = 0x33,
		GetInDOSFlagAddress = 0x34,
		GetInterruptVector = 0x35, // not implemented
		GetDiskFreeSpace = 0x36,
		GetSetCountryInformation = 0x38,
		CreateDirectory = 0x39,
		RemoveDirectory = 0x3A,
		ChangeCurrentDirectory = 0x3B,
		CreateFileWithHandle = 0x3C,
		OpenFileWithHandle = 0x3D,
		CloseFileWithHandle = 0x3E,
		ReadFileOrDevice = 0x3F,
		WriteFileOrDevice = 0x40,
		DeleteFile = 0x41,
		MoveFilePointer = 0x42,
		Function43 = 0x43,
		Function44 = 0x44,
		DuplicateFileHandle = 0x45,
		ForceDuplicateFileHandle = 0x46,
		GetCurrentDirectory = 0x47,
		AllocateMemory = 0x48,
		FreeAllocatedMemory = 0x49,
		SetMemoryBlockSize = 0x4A,
		Function4B = 0x4B,
		EndProgram = 0x4C,
		GetChildProgramReturnValue = 0x4D,
		FindFirstFile = 0x4E,
		FindNextFile = 0x4F,
		SetPSPAddress = 0x50,
		GetPSPAddress = 0x51,
		GetVerifyState = 0x52,
		RenameFile = 0x53,
		Function57 = 0x57,
		Function58 = 0x58,
		GetExtendedError = 0x59,
		CreateTemporaryFile = 0x5A,
		CreateNewFile = 0x5B,
		LockUnlockFile = 0x5C,
		SetExtendedError = 0x5D,
		Function5E = 0x5E,
		Function5F = 0x5F, // network operations -- not supported
		TrueName = 0x60, // undocumented
		GetCurrentPSPAddress = 0x62,
		Function65 = 0x65,
		Function66 = 0x66,
		SetMaximumHandleCount = 0x67,
		CommitFile = 0x68,
		CommitFile2 = 0x6A,
		NullFunction = 0x6B,
		ExtendedOpenCreate = 0x6C,
	}

	public enum Function33 : byte
	{
		GetCtrlCCheckFlag = 0x00,
		SetCtrlCCheckFlag = 0x01, // not implemented
		GetStartupDrive = 0x05,
		GetMSDOSVersion = 0x06,
	}

	public enum Function43 : byte
	{
		GetFileAttributes = 0x00,
		SetFileAttributes = 0x01,
		ExtendedLengthFileNameOperations = 0xFF, // per Ralf Brown's Interrupt List
	}

	public enum Function44 : byte
	{
		GetDeviceData = 0x00,
		SetDeviceData = 0x01,
		ReceiveControlDataFromCharacterDevice = 0x02, // not implemented
		SendControlDataToCharacterDevice = 0x03, // not implemented
		ReceiveControlDataFromBlockDevice = 0x04, // not implemented
		SendControlDataToBlockDevice = 0x05, // not implemented
		CheckDeviceInputStatus = 0x06,
		CheckDeviceOutputStatus = 0x07,
		DoesDeviceUseRemovableMedia = 0x08,
		IsDriveRemote = 0x09,
		IsFileOrDeviceRemote = 0x0A,
		SetSharingRetryCount = 0x0B, // not implemented
		Function440C = 0x0C,
		Function440D = 0x0D,
		GetLogicalDriveMap = 0x0E, // not implemented
		SetLogicalDriveMap = 0x0F, // not implemented
		QueryIOCTLHandle = 0x10, // not implemented
		QueryIOCTLDevice = 0x11, // not implemented
	}

	public enum Function440CMinorCode : byte
	{
		SetIterationCount = 0x45, // not implemented
		SelectCodePage = 0x4A, // not implemented
		StartCodePagePrepare = 0x4C, // not implemented
		EndCodePagePrepare = 0x4D, // not implemented
		SetDisplayMode = 0x5F, // not implemented
		GetIterationCount = 0x65, // not implemented
		QuerySelectedCodePage = 0x6A, // not implemented
		QueryCodePagePrepareList = 0x6B, // not implemented
		GetDisplayMode = 0x7F, // not implemented
	}

	public enum Function440DMinorCode : byte
	{
		SetDeviceParameters = 0x40, // not implemented
		WriteTrackOnLogicalDrive = 0x41, // not implemented
		FormatTrackOnLogicalDrive = 0x42, // not implemented
		SetMediaID = 0x46, // not implemented
		GetDeviceParameters = 0x60, // not implemented
		ReadTrackOnLogicalDrive = 0x61, // not implemented
		VerifyTrackOnLogicalDrive = 0x62, // not implemented
		GetMediaID = 0x66, // not implemented
		SenseMediaType = 0x68, // not implemented
	}

	public enum Function4B : byte
	{
		LoadAndExecuteProgram = 0x00, // not implemented
		LoadProgram = 0x01, // not implemented
		LoadOverlay = 0x03, // not implemented
		SetExecutionState = 0x05, // not implemented
	}

	public enum Function57 : byte
	{
		GetFileDateAndTime = 0x00,
		SetFileDateAndTime = 0x01,
		GetFileLastAccessDateAndTime = 0x04,
		SetFileLastAccessDateAndTime = 0x05,
		GetFileCreationDateAndTime = 0x06,
		SetFileCreationDateAndTime = 0x07,
	}

	public enum Function58 : byte
	{
		GetAllocationStrategy = 0x00,
		SetAllocationStrategy = 0x01,
		GetUpperMemoryLink = 0x02,
		SetUpperMemoryLink = 0x03,
	}

	public enum Function5E : byte
	{
		GetMachineName = 0x00,
		SetPrinterSetup = 0x02,
		GetPrinterSetup = 0x03,
	}

	public enum Function65 : byte
	{
		GetExtendedCountryInformation = 0x01,
		GetUppercaseTable = 0x02,
		GetFilenameUppercaseTable = 0x04,
		GetFilenameCharacterTable = 0x05,
		GetCollateSequenceTable = 0x06,
		GetDoubleByteCharacterSet = 0x07,
		ConvertCharacter = 0x20,
		ConvertString = 0x21,
		ConvertASCIIZString = 0x22,
	}

	public enum Function66 : byte
	{
		GetGlobalCodePage = 0x01,
		SetGlobalCodePage = 0x02,
	}

	public override Registers Execute(Registers input)
	{
		using (machine.DOS.InDOS())
		{
			byte ah = unchecked((byte)(input.AX >> 8));
			byte al = unchecked((byte)input.AX);

			var function = (Function)ah;

			var inputEx = input.AsRegistersEx();

			var result = inputEx;

			result.FLAGS &= ~Flags.Carry;

			using var suppressionScope = machine.DOS.SuppressExceptionsInScope();

			switch (function)
			{
				case Function.TerminateProgram:
				case Function.KeepProgram:
				{
					machine.DOS.TerminateProgram();
					throw new TerminatedException();
				}
				case Function.ReadKeyboardWithEcho:
				{
					using (machine.DOS.EnableBreak())
					{
						try
						{
							byte b = machine.DOS.ReadByte(DOS.StandardInput, echo: true);

							result.AX &= 0xFF00;
							result.AX |= b;
						}
						catch (Break) { }
					}
					break;
				}
				case Function.DisplayCharacter:
				{
					byte ch = unchecked((byte)input.DX);

					machine.DOS.WriteByte(DOS.StandardOutput, ch, out byte lastCharacterWritten);

					result.AX &= 0xFF00;
					result.AX |= lastCharacterWritten;

					break;
				}
				case Function.AuxiliaryInput:
				case Function.AuxiliaryOutput:
				case Function.PrintCharacter:
				{
					// If you don't have a serial port/printer, it appears DOS simply hangs. I'm not going to do that. :-)
					result.AX &= 0xFF00;
					break;
				}
				case Function.DirectConsoleIO:
				{
					byte dl = unchecked((byte)input.DX);

					if (dl != 0xFF)
						machine.DOS.WriteByte(DOS.StandardOutput, dl, out _);
					else
					{
						result.AX &= 0xFF00;

						if (machine.DOS.TryReadByte(DOS.StandardInput, out byte b))
						{
							result.AX |= b;
							result.FLAGS &= ~Flags.Zero;
						}
						else
							result.FLAGS |= Flags.Zero;
					}

					break;
				}
				case Function.DirectConsoleInput:
				{
					result.AX &= 0xFF00;
					result.AX |= machine.DOS.ReadByte(DOS.StandardInput, echo: false);
					break;
				}
				case Function.ReadKeyboardWithoutEcho:
				{
					using (machine.DOS.EnableBreak())
					{
						try
						{
							result.AX &= 0xFF00;
							result.AX |= machine.DOS.ReadByte(DOS.StandardInput, echo: false);
						}
						catch (Break) { }
					}
					break;
				}
				case Function.DisplayString:
				{
					int o = inputEx.DS * 0x10 + result.DX;

					while (true)
					{
						byte b = machine.MemoryBus[o++];

						if (b == (byte)'$')
							break;

						machine.DOS.WriteByte(DOS.StandardOutput, b, out _);
					}

					result.AX &= 0xFF00;
					result.AX |= (byte)'$';

					break;
				}
				case Function.BufferedKeyboardInput:
				{
					int o = inputEx.DS * 0x10 + result.DX;

					int numBytesDesired = machine.MemoryBus[o];

					o += 2;

					int numBytesRead = 0;

					while (numBytesRead < numBytesDesired)
					{
						byte b = machine.DOS.ReadByte(DOS.StandardInput, echo: false);

						if ((numBytesRead + 1 == numBytesDesired) && (b != 13))
						{
							// Emit BEL character.
							machine.DOS.WriteByte(DOS.StandardOutput, 7, out _);
							continue;
						}

						machine.DOS.WriteByte(DOS.StandardOutput, b, out _);

						machine.MemoryBus[o + numBytesRead] = b;
						numBytesRead++;

						if (b == 13)
							break;
					}

					machine.MemoryBus[o - 1] = (byte)(numBytesRead - 1);

					break;
				}
				case Function.CheckKeyboardStatus:
				{
					result.AX &= 0xFF00;

					if ((machine.DOS.Files[0]?.ReadBuffer.IsEmpty == false)
					 || machine.Keyboard.HasQueuedTangibleInput)
						result.AX |= 0xFF;

					break;
				}
				case Function.FlushBufferReadKeyboard:
				{
					machine.DOS.FlushStandardInput();

					result.AX &= 0xFF00;

					if (al == 1)
						goto case Function.ReadKeyboardWithEcho;
					else if ((al == 6) && ((input.DX & 0xFF) == 0xFF))
						goto case Function.DirectConsoleIO;
					else if (al == 7)
						goto case Function.DirectConsoleInput;
					else if (al == 8)
						goto case Function.ReadKeyboardWithoutEcho;

					break;
				}
				case Function.ResetDrive: // does not reset any drives, but does flush all write buffers
				{
					machine.DOS.FlushAllBuffers();
					break;
				}
				case Function.SetDefaultDrive:
				{
					char drive = (char)('A' + (input.DX & 0xFF));

					if (char.IsAsciiLetterUpper(drive))
						machine.DOS.SetDefaultDrive(drive);

					result.AX &= 0xFF00;
					result.AX |= (byte)machine.DOS.GetLogicalDriveCount();

					break;
				}
				case Function.OpenFileWithFCB:
				{
					int offset = inputEx.DS * 0x10 + input.DX;

					var fcb = FileControlBlock.Deserialize(machine.MemoryBus, offset);

					int fd = machine.DOS.OpenFile(fcb, FileMode.Open);

					fcb.Serialize(machine.MemoryBus);

					result.AX &= 0xFF00;

					if (fd < 0)
						result.AX |= 0xFF;

					break;
				}
				case Function.CloseFileWithFCB:
				{
					int offset = inputEx.DS * 0x10 + input.DX;

					var fcb = FileControlBlock.Deserialize(machine.MemoryBus, offset);

					result.AX &= 0xFF00;

					if (!machine.DOS.CloseFile(fcb.FileHandle))
						result.AX |= 0xFF;

					break;
				}
				case Function.FindFirstFileWithFCB:
				{
					int offset = inputEx.DS * 0x10 + input.DX;

					var fcb = FileControlBlock.Deserialize(machine.MemoryBus, offset);

					result.AX &= 0xFF00;

					if (!machine.DOS.FindFirst(fcb))
						result.AX |= 0xFF;

					fcb.Serialize(machine.MemoryBus);

					break;
				}
				case Function.FindNextFileWithFCB:
				{
					int offset = inputEx.DS * 0x10 + input.DX;

					var fcb = FileControlBlock.Deserialize(machine.MemoryBus, offset);

					result.AX &= 0xFF00;

					if (!machine.DOS.FindNext(fcb))
						result.AX |= 0xFF;

					break;
				}
				case Function.DeleteFileWithFCB:
				{
					int offset = inputEx.DS * 0x10 + input.DX;

					var fcb = FileControlBlock.Deserialize(machine.MemoryBus, offset);

					result.AX &= 0xFF00;

					machine.DOS.DeleteFile(fcb);

					if (machine.DOS.LastError != DOSError.None)
						result.AX |= 0xFF;

					break;
				}
				case Function.SequentialRead:
				{
					int offset = inputEx.DS * 0x10 + input.DX;

					var fcb = FileControlBlock.Deserialize(machine.MemoryBus, offset);

					result.AX &= 0xFF00;

					try
					{
						machine.DOS.ReadRecord(fcb, advance: true);

						if (machine.DOS.LastError != DOSError.None)
							result.AX |= 0xFF;
					}
					catch (OperationCanceledException)
					{
						result.AX |= 0x02;
					}

					fcb.Serialize(machine.MemoryBus);

					break;
				}
				case Function.SequentialWrite:
				{
					int offset = inputEx.DS * 0x10 + input.DX;

					var fcb = FileControlBlock.Deserialize(machine.MemoryBus, offset);

					result.AX &= 0xFF00;

					try
					{
						machine.DOS.WriteRecord(fcb, advance: true);

						if (machine.DOS.LastError != DOSError.None)
						{
							if (machine.DOS.LastError == DOSError.HandleDiskFull)
								result.AX |= 0x01;
							else
								result.AX |= 0xFF;
						}
					}
					catch (OperationCanceledException)
					{
						result.AX |= 0x02;
					}

					fcb.Serialize(machine.MemoryBus);

					break;
				}
				case Function.CreateFileWithFCB:
				{
					int offset = inputEx.DS * 0x10 + input.DX;

					var fcb = FileControlBlock.Deserialize(machine.MemoryBus, offset);

					int fd = machine.DOS.OpenFile(fcb, FileMode.Create);

					fcb.Serialize(machine.MemoryBus);

					result.AX &= 0xFF00;

					if (fd < 0)
						result.AX |= 0xFF;

					break;
				}
				case Function.RenameFileWithFCB:
				{
					int offset = inputEx.DS * 0x10 + input.DX;

					var rfcb = RenameFileControlBlock.Deserialize(machine.MemoryBus, offset);

					machine.DOS.RenameFiles(rfcb);

					result.AX &= 0xFF00;

					if (machine.DOS.LastError != DOSError.None)
						result.AX |= 0xFF;

					break;
				}
				case Function.GetDefaultDrive:
				{
					try
					{
						int defaultDrive = machine.DOS.GetDefaultDrive();

						if ((defaultDrive >= 0) && (defaultDrive <= 25))
						{
							result.AX &= 0xFF00;
							result.AX |= (byte)defaultDrive;
						}
					}
					catch
					{
						result.AX |= 0xFF;
					}

					break;
				}
				case Function.SetDiskTransferAddress:
				{
					machine.DOS.DiskTransferAddressSegment = result.DS;
					machine.DOS.DiskTransferAddressOffset = result.DX;
					break;
				}
				case Function.GetDefaultDriveData:
				{
					input.DX = 0;
					goto case Function.GetDriveData;
				}
				case Function.GetDriveData:
				{
					try
					{
						int driveIndicator = input.DX & 0xFF;

						var dpbAddress = (driveIndicator != 0)
							? machine.DOS.GetDriveParameterBlock(driveIndicator - 1)
							: machine.DOS.GetDefaultDriveParameterBlock();

						if (dpbAddress != 0)
						{
							ref var dpb = ref DriveParameterBlock.CreateReference(machine.SystemMemory, dpbAddress.ToLinearAddress());

							// No way to sensibly map modern numbers into the return from this
							// call, so we just fake a 100MB hard drive or a 1.44MB floppy.

							result.DS = dpbAddress.Segment;
							result.BX = (ushort)(dpbAddress.Offset + DriveParameterBlock.MediaDescriptorFieldOffset);

							if (dpb.MediaDescriptor == MediaDescriptor.FloppyDisk)
							{
								result.AX = 1; // sectors per cluster
								result.CX = 512; // bytes per sector
								result.DX = 2880; // number of clusters
							}
							else
							{
								result.AX = 4; // sectors per cluster
								result.CX = 512; // bytes per sector
								result.DX = 51200; // number of clusters
							}
						}
					}
					catch
					{
						machine.DOS.SetLastError(DOSError.InvalidFunction);
					}

					if (machine.DOS.LastError != DOSError.None)
						result.AX |= 0xFF;

					break;
				}
				case Function.GetDefaultDPB:
				{
					var address = machine.DOS.GetDefaultDriveParameterBlock();

					result.DS = address.Segment;
					result.BX = address.Offset;

					break;
				}
				case Function.RandomRead:
				{
					int offset = inputEx.DS * 0x10 + input.DX;

					var fcb = FileControlBlock.Deserialize(machine.MemoryBus, offset);

					if (fcb.RandomRecordNumber > 8388607) // maximum addressible record number
						result.AX |= 0xFF;
					else
					{
						result.AX &= 0xFF00;

						try
						{
							unchecked
							{
								fcb.CurrentBlockNumber = (ushort)(fcb.RandomRecordNumber / 128);
								fcb.CurrentRecordNumber = (byte)(fcb.RandomRecordNumber & 127);
							}

							machine.DOS.ReadRecord(fcb, advance: false);

							if (machine.DOS.LastError != DOSError.None)
								result.AX |= 0xFF;
						}
						catch (OperationCanceledException)
						{
							result.AX |= 0x02;
						}

						fcb.Serialize(machine.MemoryBus);
					}

					break;
				}
				case Function.RandomWrite:
				{
					int offset = inputEx.DS * 0x10 + input.DX;

					var fcb = FileControlBlock.Deserialize(machine.MemoryBus, offset);

					if (fcb.RandomRecordNumber > 8388607) // maximum addressible record number
						result.AX |= 0xFF;
					else
					{
						result.AX &= 0xFF00;

						try
						{
							unchecked
							{
								fcb.CurrentBlockNumber = (ushort)(fcb.RandomRecordNumber / 128);
								fcb.CurrentRecordNumber = (byte)(fcb.RandomRecordNumber & 127);
							}

							machine.DOS.WriteRecord(fcb, advance: false);

							if (machine.DOS.LastError != DOSError.None)
							{
								if (machine.DOS.LastError == DOSError.HandleDiskFull)
									result.AX |= 0x01;
								else
									result.AX |= 0xFF;
							}
						}
						catch (OperationCanceledException)
						{
							result.AX |= 0x02;
						}

						fcb.Serialize(machine.MemoryBus);
					}

					break;
				}
				case Function.GetFileSize:
				{
					int offset = inputEx.DS * 0x10 + input.DX;

					var fcb = FileControlBlock.Deserialize(machine.MemoryBus, offset);

					result.AX &= 0xFF00;

					if (!machine.DOS.PopulateFileInfo(fcb))
						result.AX |= 0xFF;

					fcb.Serialize(machine.MemoryBus);

					break;
				}
				case Function.SetRandomRecordNumber:
				{
					int offset = inputEx.DS * 0x10 + input.DX;

					var fcb = FileControlBlock.Deserialize(machine.MemoryBus, offset);

					result.AX &= 0xFF00;

					if ((fcb.FileHandle == 0) || (fcb.RandomRecordNumber != 0))
						result.AX |= 0xFF;
					else
						fcb.RandomRecordNumber = unchecked((uint)fcb.RecordPointer);

					fcb.Serialize(machine.MemoryBus);

					break;
				}
				case Function.RandomBlockRead:
				{
					int offset = inputEx.DS * 0x10 + input.DX;

					var fcb = FileControlBlock.Deserialize(machine.MemoryBus, offset);

					if (fcb.RandomRecordNumber > 8388607) // maximum addressible record number
						result.AX |= 0xFF;
					else
					{
						result.AX &= 0xFF00;

						try
						{
							unchecked
							{
								fcb.CurrentBlockNumber = (ushort)(fcb.RandomRecordNumber / 128);
								fcb.CurrentRecordNumber = (byte)(fcb.RandomRecordNumber & 127);
							}

							int recordCount = input.CX;

							machine.DOS.ReadRecords(fcb, recordCount);

							if (machine.DOS.LastError != DOSError.None)
								result.AX |= 0xFF;
						}
						catch (OperationCanceledException)
						{
							result.AX |= 0x02;
						}

						fcb.Serialize(machine.MemoryBus);
					}

					break;
				}
				case Function.RandomBlockWrite:
				{
					int offset = inputEx.DS * 0x10 + input.DX;

					var fcb = FileControlBlock.Deserialize(machine.MemoryBus, offset);

					if (fcb.RandomRecordNumber > 8388607) // maximum addressible record number
						result.AX |= 0xFF;
					else
					{
						result.AX &= 0xFF00;

						try
						{
							unchecked
							{
								fcb.CurrentBlockNumber = (ushort)(fcb.RandomRecordNumber / 128);
								fcb.CurrentRecordNumber = (byte)(fcb.RandomRecordNumber & 127);
							}

							int recordCount = input.CX;

							machine.DOS.WriteRecords(fcb, recordCount);

							if (machine.DOS.LastError != DOSError.None)
							{
								if (machine.DOS.LastError == DOSError.HandleDiskFull)
									result.AX |= 0x01;
								else
									result.AX |= 0xFF;
							}
						}
						catch (OperationCanceledException)
						{
							result.AX |= 0x02;
						}

						fcb.Serialize(machine.MemoryBus);
					}

					break;
				}
				case Function.ParseFilename:
				{
					int offset = inputEx.ES * 0x10 + input.DI;

					var fcb = FileControlBlock.Deserialize(machine.MemoryBus, offset);

					var flags = unchecked((ParseFlags)input.AX);

					var inputPointer = new SegmentedAddress(result.DS, result.SI);
					int inputLinearAddress = inputPointer.ToLinearAddress();

					FileControlBlock.ParseFileName(
						readInputChar: (idx) => machine.MemoryBus[inputLinearAddress + idx],
						lengthIsAtLeast: (testLength) => (65536 - inputPointer.Offset >= testLength),
						advanceInput:
							(numBytes) =>
							{
								inputPointer.Offset += (ushort)numBytes;
								inputLinearAddress = inputPointer.ToLinearAddress();
							},
						ref fcb.DriveIdentifier, fcb.FileNameBytes,
						out bool containsWildcards,
						out bool invalidDriveLetter,
						flags);

					fcb.Serialize(machine.MemoryBus);

					result.AX &= 0xFF00;

					if (containsWildcards)
						result.AX |= 0x0001;

					result.DS = inputPointer.Segment;
					result.SI = inputPointer.Offset;

					break;
				}
				case Function.GetDate:
				{
					var date = machine.SystemClock.Now;

					result.AX &= 0xFF00;
					result.AX |= (byte)date.DayOfWeek;

					result.CX = (ushort)date.Year;

					result.DX = unchecked((ushort)((date.Month << 8) | date.Day));

					break;
				}
				case Function.SetDate:
				{
					var now = new DateTime(
						year: input.CX,
						month: input.DX >> 8,
						day: input.DX & 0xFF);

					now += machine.SystemClock.Now.TimeOfDay;

					machine.SystemClock.SetCurrentTime(now);

					break;
				}
				case Function.GetTime:
				{
					var time = machine.SystemClock.Now;

					result.CX = unchecked((ushort)((time.Hour << 8) | time.Minute));
					result.DX = unchecked((ushort)((time.Second << 8) | (time.Millisecond / 10)));

					break;
				}
				case Function.SetTime:
				{
					var timeOfDay = new TimeSpan(
						days: 0,
						hours: input.CX >> 8,
						minutes: input.CX & 0xFF,
						seconds: input.DX >> 8,
						milliseconds: (input.DX & 0xFF) * 10);

					var now = machine.SystemClock.Now.Date + timeOfDay;

					machine.SystemClock.SetCurrentTime(now);

					break;
				}
				case Function.SetResetVerifyFlag:
				{
					machine.DOS.VerifyWrites = (input.AX & 0xFF) != 0;
					break;
				}
				case Function.GetDiskTransferAddress:
				{
					result.ES = machine.DOS.DiskTransferAddressSegment;
					result.BX = machine.DOS.DiskTransferAddressOffset;
					break;
				}
				case Function.GetVersionNumber:
				{
					bool oemNumber = (input.AX & 0xFF) == 0;

					result.AX = 5; // version 5.0, which is the MS-DOS Programmer's Reference this code was based on
					result.BX = oemNumber ? (ushort)0 : (ushort)0x800; // DOS version flag 8: DOS is running from ROM
					result.CX = 0; // BL:CX: serial number (0 if not used)

					break;
				}
				case Function.GetDPB:
				{
					int driveNumber = input.DX & 0xFF;

					if (driveNumber == 0)
						goto case Function.GetDefaultDPB;

					result.AX &= 0xFF00;

					var address = machine.DOS.GetDriveParameterBlock(driveNumber);

					result.DS = address.Segment;
					result.BX = address.Offset;

					if (machine.DOS.LastError != DOSError.None)
						result.AX |= 0xFF;

					break;
				}
				case Function.Function33:
				{
					var subfunction = (Function33)al;

					switch (subfunction)
					{
						case Function33.GetCtrlCCheckFlag:
						{
							result.DX = 0; // always off
							break;
						}
						case Function33.GetStartupDrive:
						{
							try
							{
								string binaryPath = Path.GetFullPath(typeof(DOS).Assembly.Location);

								result.DX = (ushort)(char.ToUpper(binaryPath[0]) - 'A' + 1);
							}
							catch
							{
								result.DX = 3; // C:\, if something goes wrong
							}

							break;
						}
						case Function33.GetMSDOSVersion:
						{
							result.BX = 5; // MS-DOS version 5.0, which is the MS-DOS Programmer's Reference this code was based on
							result.DX = 0x800; // revision 0, version flag 8: DOS is running from ROM

							break;
						}
						default:
						{
							machine.DOS.SetLastError(DOSError.InvalidFunction);

							result.FLAGS |= Flags.Carry;
							result.AX = (ushort)machine.DOS.LastError;

							break;
						}
					}

					break;
				}
				case Function.GetInDOSFlagAddress:
				{
					result.ES = machine.DOS.InDOSFlagAddress.Segment;
					result.BX = machine.DOS.InDOSFlagAddress.Offset;

					break;
				}
				case Function.GetDiskFreeSpace:
				{
					int driveIdentifier = input.DX & 0xFF;

					var dpbAddress =
						driveIdentifier == 0
						? machine.DOS.GetDefaultDriveParameterBlock()
						: machine.DOS.GetDriveParameterBlock(driveIdentifier - 1);

					if (dpbAddress == 0)
						result.AX = 0xFFFF;
					else
					{
						ref DriveParameterBlock dpb = ref DriveParameterBlock.CreateReference(machine.SystemMemory, dpbAddress.ToLinearAddress());

						result.AX = unchecked((ushort)(65536 / dpb.SectorSize));
						result.BX = 0xFFFF;
						result.CX = dpb.SectorSize;
						result.DX = 0xFFFF;
					}

					break;
				}
				case Function.GetSetCountryInformation:
				{
					if (input.DX != 0xFFFF)
					{
						// Get
						int offset = inputEx.DS * 0x10 + input.DX;

						var countryInfo = new CountryInfo();

						countryInfo.Import(machine.DOS.CurrentCulture);

						countryInfo.Serialize(machine.MemoryBus, offset);
					}
					else
					{
						// Set
						int countryCodeValue = input.AX & 0xFF;

						if (countryCodeValue == 0xFF)
							countryCodeValue = input.BX;

						var countryCode = (CountryCode)countryCodeValue;

						machine.DOS.CurrentCulture = CultureInfo.GetCultureInfo(countryCode.ToCultureName());
					}

					break;
				}
				case Function.CreateDirectory:
				case Function.RemoveDirectory:
				case Function.ChangeCurrentDirectory:
				{
					int address = inputEx.DS * 0x10 + input.DX;

					var directoryName = machine.DOS.ReadStringZ(machine.MemoryBus, address);

					if (machine.DOS.LastError != DOSError.None)
					{
						result.AX |= 0xFF;
						break;
					}

					result.AX &= 0xFF00;

					switch (function)
					{
						case Function.CreateDirectory: machine.DOS.CreateDirectory(directoryName); break;
						case Function.RemoveDirectory: machine.DOS.RemoveDirectory(directoryName); break;
						case Function.ChangeCurrentDirectory: machine.DOS.ChangeCurrentDirectory(directoryName); break;
					}

					if (machine.DOS.LastError != DOSError.None)
						result.AX |= 0xFF;

					break;
				}
				case Function.CreateFileWithHandle:
				{
					int address = inputEx.DS * 0x10 + input.DX;

					var relativePath = machine.DOS.ReadStringZ(machine.MemoryBus, address);

					if (machine.DOS.LastError != DOSError.None)
					{
						result.FLAGS |= Flags.Carry;
						result.AX = (ushort)machine.DOS.LastError;
						break;
					}

					var attributes = (FileAttributes)input.CX;

					if ((attributes & FileAttributes.Directory) != 0)
						machine.DOS.SetLastError(DOSError.InvalidParameter);
					else
					{
						int fileHandle = machine.DOS.OpenFile(relativePath.ToString(), FileMode.Create, default);

						result.AX = (ushort)fileHandle;

						if (machine.DOS.LastError == DOSError.None)
							machine.DOS.SetFileAttributes(fileHandle, attributes);
					}

					if (machine.DOS.LastError != DOSError.None)
					{
						result.FLAGS |= Flags.Carry;
						result.AX = (ushort)machine.DOS.LastError;
					}

					break;
				}
				case Function.OpenFileWithHandle:
				{
					int address = inputEx.DS * 0x10 + input.DX;

					var relativePath = machine.DOS.ReadStringZ(machine.MemoryBus, address);

					if (machine.DOS.LastError != DOSError.None)
					{
						result.FLAGS |= Flags.Carry;
						result.AX = (ushort)machine.DOS.LastError;
						break;
					}

					var accessModes = (OpenMode)(input.AX & 0xFF);

					int fileHandle = machine.DOS.OpenFile(relativePath.ToString(), FileMode.Open, accessModes);

					result.AX = (ushort)fileHandle;

					if (machine.DOS.LastError != DOSError.None)
					{
						result.FLAGS |= Flags.Carry;
						result.AX = (ushort)machine.DOS.LastError;
					}

					break;
				}
				case Function.CloseFileWithHandle:
				{
					machine.DOS.CloseFile(fileHandle: input.BX);

					if (machine.DOS.LastError != DOSError.None)
					{
						result.FLAGS |= Flags.Carry;
						result.AX = (ushort)machine.DOS.LastError;
					}

					break;
				}
				case Function.ReadFileOrDevice:
				{
					int fileHandle = input.BX;
					int numBytes = input.CX;
					var bufferAddress = new SegmentedAddress(inputEx.DS, input.DX);

					result.AX = (ushort)machine.DOS.Read(fileHandle, machine.MemoryBus, bufferAddress.ToLinearAddress(), numBytes);

					if (machine.DOS.LastError != DOSError.None)
					{
						result.FLAGS |= Flags.Carry;
						result.AX = (ushort)machine.DOS.LastError;
					}

					break;
				}
				case Function.WriteFileOrDevice:
				{
					int fileHandle = input.BX;
					int numBytes = input.CX;
					var bufferAddress = new SegmentedAddress(inputEx.DS, input.DX);

					result.AX = (ushort)machine.DOS.Write(fileHandle, machine.MemoryBus, bufferAddress.ToLinearAddress(), numBytes);

					if (machine.DOS.LastError != DOSError.None)
					{
						result.FLAGS |= Flags.Carry;
						result.AX = (ushort)machine.DOS.LastError;
					}

					break;
				}
				case Function.DeleteFile:
				{
					int address = inputEx.DS * 0x10 + input.DX;

					var relativePath = machine.DOS.ReadStringZ(machine.MemoryBus, address);

					if (machine.DOS.LastError != DOSError.None)
					{
						result.FLAGS |= Flags.Carry;
						result.AX = (ushort)machine.DOS.LastError;
						break;
					}

					result.FLAGS &= Flags.Carry;

					machine.DOS.DeleteFile(relativePath.ToString());

					if (machine.DOS.LastError != DOSError.None)
					{
						result.FLAGS |= Flags.Carry;
						result.AX = (ushort)machine.DOS.LastError;
					}

					break;
				}
				case Function.MoveFilePointer:
				{
					int fileHandle = input.BX;
					int offset = (input.CX << 16) | input.DX;
					MoveMethod moveMethod = (MoveMethod)(input.AX & 0xFF);

					uint newPosition = (moveMethod == MoveMethod.FromBeginning)
						? machine.DOS.SeekFile(fileHandle, unchecked((uint)offset), moveMethod)
						: machine.DOS.SeekFile(fileHandle, offset, moveMethod);

					if (machine.DOS.LastError == DOSError.None)
					{
						result.DX = unchecked((ushort)(newPosition >> 16));
						result.AX = unchecked((ushort)newPosition);
					}
					else
					{
						result.FLAGS |= Flags.Carry;
						result.AX = (ushort)machine.DOS.LastError;
					}

					break;
				}
				case Function.Function43:
				{
					var subfunction = (Function43)al;

					if (subfunction == Function43.ExtendedLengthFileNameOperations)
					{
						input.AX = input.CX;
						input.AX <<= 8;

						switch (input.AX)
						{
							case 0x3900:
							case 0x5600:
								return Execute(input);
						}
					}

					switch (subfunction)
					{
						case Function43.GetFileAttributes:
						case Function43.SetFileAttributes:
						{
							int address = inputEx.DS * 0x10 + input.DX;

							var relativePath = machine.DOS.ReadStringZ(machine.MemoryBus, address);

							if (machine.DOS.LastError != DOSError.None)
							{
								result.FLAGS |= Flags.Carry;
								result.AX = (ushort)machine.DOS.LastError;
								break;
							}

							if (subfunction == Function43.GetFileAttributes)
								result.CX = (ushort)machine.DOS.GetFileAttributes(relativePath.ToString());
							else
								machine.DOS.SetFileAttributes(relativePath.ToString(), (FileAttributes)input.CX);

							if (machine.DOS.LastError != DOSError.None)
							{
								result.FLAGS |= Flags.Carry;
								result.AX = (ushort)machine.DOS.LastError;
							}

							break;
						}
						default:
						{
							machine.DOS.SetLastError(DOSError.InvalidFunction);

							result.FLAGS |= Flags.Carry;
							result.AX = (ushort)machine.DOS.LastError;

							break;
						}
					}

					break;
				}
				case Function.Function44:
				{
					var subfunction = (Function44)al;

					switch (subfunction)
					{
						case Function44.GetDeviceData:
						{
							int fileHandle = input.BX;

							machine.DOS.ClearLastError();

							if ((fileHandle < 0) || (fileHandle >= machine.DOS.Files.Count)
							 || (machine.DOS.Files[fileHandle] is not FileDescriptor fileDescriptor))
								machine.DOS.SetLastError(DOSError.InvalidHandle);
							else if (fileDescriptor is RegularFileDescriptor regularFile)
							{
								result.DX = unchecked((ushort)(
									(char.ToUpperInvariant(regularFile.Path[0]) - 'A') |
									(regularFile.IsPristine ? 64 : 0)));
							}
							else
							{
								result.DX = 128;

								switch (fileDescriptor)
								{
									case ConsoleFileDescriptor: result.DX |= 3; break;
									case NullFileDescriptor: result.DX |= 4; break;
									case ClockFileDescriptor: result.DX |= 8; break;
								}

								if (fileDescriptor.IOMode == IOMode.Binary)
									result.DX |= 32;

								if (fileDescriptor.CanRead && !fileDescriptor.AtSoftEOF)
									result.DX |= 1 << 6;
							}

							if (machine.DOS.LastError != DOSError.None)
							{
								result.FLAGS |= Flags.Carry;
								result.AX = (ushort)machine.DOS.LastError;
							}

							break;
						}
						case Function44.SetDeviceData:
						{
							int fileHandle = input.BX;

							machine.DOS.ClearLastError();

							if ((fileHandle < 0) || (fileHandle >= machine.DOS.Files.Count)
							 || (machine.DOS.Files[fileHandle] is not FileDescriptor fileDescriptor))
								machine.DOS.SetLastError(DOSError.InvalidHandle);
							else if ((fileDescriptor is RegularFileDescriptor) || ((input.DX & 128) == 0))
								machine.DOS.SetLastError(DOSError.InvalidFunction);
							else
							{
								bool isConsole = (fileDescriptor is ConsoleFileDescriptor);
								bool isNull = (fileDescriptor is NullFileDescriptor);
								bool isClock = (fileDescriptor is ClockFileDescriptor);

								int consoleBits = isConsole ? 3 : 0;
								int nullBits = isNull ? 4 : 0;
								int clockBits = isClock ? 8 : 0;

								if (((input.DX & 3) != consoleBits)
								 || ((input.DX & 4) != nullBits)
								 || ((input.DX & 8) != clockBits)
								 || ((input.DX & 16) != 0)) // "special device"
									machine.DOS.SetLastError(DOSError.InvalidFunction);
								else
								{
									fileDescriptor.SetIOMode(
										((input.DX & 32) == 0)
										? IOMode.ASCII
										: IOMode.Binary);

									fileDescriptor.AtSoftEOF = ((input.DX & 64) != 0);
								}
							}

							if (machine.DOS.LastError != DOSError.None)
							{
								result.FLAGS |= Flags.Carry;
								result.AX = (ushort)machine.DOS.LastError;
							}

							break;
						}
						case Function44.ReceiveControlDataFromCharacterDevice:
						case Function44.SendControlDataToCharacterDevice:
						case Function44.ReceiveControlDataFromBlockDevice:
						case Function44.SendControlDataToBlockDevice:
						{
							machine.DOS.SetLastError(DOSError.NotSupported);

							result.FLAGS |= Flags.Carry;
							result.AX = (ushort)machine.DOS.LastError;
							break;
						}
						case Function44.CheckDeviceInputStatus:
						{
							int fileHandle = input.BX;

							machine.DOS.ClearLastError();

							if ((fileHandle < 0) || (fileHandle >= machine.DOS.Files.Count)
							 || (machine.DOS.Files[fileHandle] is not FileDescriptor fileDescriptor))
								machine.DOS.SetLastError(DOSError.InvalidHandle);
							else if (!fileDescriptor.CanRead)
								machine.DOS.SetLastError(DOSError.AccessDenied);
							else
							{
								result.AX &= 0xFF00;

								if (fileDescriptor.ReadyToRead)
									result.AX |= 0xFF;
							}

							if (machine.DOS.LastError != DOSError.None)
							{
								result.FLAGS |= Flags.Carry;
								result.AX = (ushort)machine.DOS.LastError;
							}

							break;
						}
						case Function44.CheckDeviceOutputStatus:
						{
							int fileHandle = input.BX;

							machine.DOS.ClearLastError();

							if ((fileHandle < 0) || (fileHandle >= machine.DOS.Files.Count)
							 || (machine.DOS.Files[fileHandle] is not FileDescriptor fileDescriptor))
								machine.DOS.SetLastError(DOSError.InvalidHandle);
							else if (!fileDescriptor.CanWrite)
								machine.DOS.SetLastError(DOSError.AccessDenied);
							else
							{
								result.AX &= 0xFF00;

								if (fileDescriptor.ReadyToWrite)
									result.AX |= 0xFF;
							}

							if (machine.DOS.LastError != DOSError.None)
							{
								result.FLAGS |= Flags.Carry;
								result.AX = (ushort)machine.DOS.LastError;
							}

							break;
						}
						case Function44.DoesDeviceUseRemovableMedia:
						{
							try
							{
								int driveIndicator = input.DX & 0xFF;

								var dpbAddress = (driveIndicator != 0)
									? machine.DOS.GetDriveParameterBlock(driveIndicator)
									: machine.DOS.GetDefaultDriveParameterBlock();

								if (dpbAddress != 0)
								{
									ref var dpb = ref DriveParameterBlock.CreateReference(machine.SystemMemory, dpbAddress.ToLinearAddress());

									if (dpb.MediaDescriptor == MediaDescriptor.FloppyDisk)
										result.AX = 0x0000;
									else
										result.AX = 0x0001;
								}
							}
							catch
							{
								if (machine.DOS.LastError == DOSError.None)
									machine.DOS.SetLastError(DOSError.InvalidFunction);
							}

							if (machine.DOS.LastError != DOSError.None)
							{
								result.FLAGS |= Flags.Carry;
								result.AX |= (ushort)machine.DOS.LastError;
							}

							break;
						}
						case Function44.IsDriveRemote:
						{
							try
							{
								int driveIndicator = input.DX & 0xFF;

								string path = driveIndicator > 0 ? ((char)(driveIndicator + 'A' - 1)).ToString() : "";

								// TODO: SUBST drives

								if (machine.DOS.IsRemoteDrive(path))
									result.DX = 0b0001000000000000; // bit 12: is network drive
								else
									result.DX = 0b0000100000000000; // bit 11: can query whether drive uses removable media
							}
							catch
							{
								if (machine.DOS.LastError == DOSError.None)
									machine.DOS.SetLastError(DOSError.InvalidFunction);
							}

							if (machine.DOS.LastError != DOSError.None)
							{
								result.FLAGS |= Flags.Carry;
								result.AX = (ushort)machine.DOS.LastError;
							}

							break;
						}
						case Function44.IsFileOrDeviceRemote:
						{
							try
							{
								int fileHandle = input.BX;

								machine.DOS.ClearLastError();

								if ((fileHandle < 0) || (fileHandle >= machine.DOS.Files.Count)
									|| (machine.DOS.Files[fileHandle] is not FileDescriptor fileDescriptor))
									machine.DOS.SetLastError(DOSError.InvalidHandle);
								else
								{
									// TODO: SUBST drives
									bool isRemote = machine.DOS.IsRemoteDrive(fileDescriptor.Path);

									if (fileDescriptor is RegularFileDescriptor regularFileDescriptor)
									{
										result.DX = unchecked((ushort)(
											(char.ToUpperInvariant(fileDescriptor.Path[0]) - 'A') |
											(regularFileDescriptor.IsPristine ? (1 << 6) : 0) |
											(1 << 12) | /* no inherit */
											(isRemote ? (1 << 15) : 0)));
									}
									else
									{
										result.DX = 128;

										switch (fileDescriptor)
										{
											case ConsoleFileDescriptor: result.DX |= 3; break;
											case NullFileDescriptor: result.DX |= 4; break;
											case ClockFileDescriptor: result.DX |= 8; break;
										}

										if (fileDescriptor.IOMode == IOMode.Binary)
											result.DX |= 32;

										if (fileDescriptor.CanRead && !fileDescriptor.AtSoftEOF)
											result.DX |= 1 << 6;

										result.DX |= 1 << 12; // no inherit

										if (isRemote)
											result.DX |= 1 << 15;
									}
								}

								if (machine.DOS.LastError != DOSError.None)
								{
									result.FLAGS |= Flags.Carry;
									result.AX = (ushort)machine.DOS.LastError;
								}
							}
							catch
							{
								if (machine.DOS.LastError == DOSError.None)
									machine.DOS.SetLastError(DOSError.InvalidFunction);
							}

							if (machine.DOS.LastError != DOSError.None)
							{
								result.FLAGS |= Flags.Carry;
								result.AX = (ushort)machine.DOS.LastError;
							}

							break;
						}
						default:
						{
							machine.DOS.SetLastError(DOSError.InvalidFunction);

							result.FLAGS |= Flags.Carry;
							result.AX = (ushort)machine.DOS.LastError;

							break;
						}
					}

					break;
				}
				case Function.DuplicateFileHandle:
				{
					int fileHandle = input.BX;

					result.AX = (ushort)machine.DOS.DuplicateHandle(fileHandle);

					if (machine.DOS.LastError != DOSError.None)
					{
						result.FLAGS |= Flags.Carry;
						result.AX = (ushort)machine.DOS.LastError;
					}

					break;
				}
				case Function.ForceDuplicateFileHandle:
				{
					int fileHandle = input.BX;
					int toFileHandle = input.CX;

					result.AX = (ushort)machine.DOS.DuplicateHandle(fileHandle, toFileHandle );

					if (machine.DOS.LastError != DOSError.None)
					{
						result.FLAGS |= Flags.Carry;
						result.AX = (ushort)machine.DOS.LastError;
					}

					break;
				}
				case Function.GetCurrentDirectory:
				{
					var buffer = new SegmentedAddress(inputEx.DS, input.SI);

					int driveIdentifier = input.DX & 0xFF;

					string path = machine.DOS.GetCurrentDirectoryUnrooted(driveIdentifier);

					if (path.Length > 63)
						machine.DOS.SetLastError(DOSError.InvalidFunction);

					int address = buffer.ToLinearAddress();
					int i;

					for (i = 0; path[i] != 0; i++)
						machine.MemoryBus[address + i] = CP437Encoding.GetByteSemantic(path[i]);
					machine.MemoryBus[address + i] = 0;

					if (machine.DOS.LastError != DOSError.None)
					{
						result.FLAGS |= Flags.Carry;
						result.AX = (ushort)machine.DOS.LastError;
					}

					break;
				}
				case Function.AllocateMemory:
				{
					int requestedParagraphs = input.BX;

					int requestedBytes = requestedParagraphs * MemoryManager.ParagraphSize;

					int largestBlockSize = 0;

					var address =
						machine.DOS.TranslateError(() =>
						{
							return machine.DOS.MemoryManager.AllocateMemory(requestedBytes, machine.DOS.CurrentPSPSegment, out largestBlockSize);
						});

					result.BX = unchecked((ushort)(largestBlockSize / MemoryManager.ParagraphSize));

					if (machine.DOS.LastError == DOSError.None)
						result.AX = unchecked((ushort)(address / MemoryManager.ParagraphSize));
					else
					{
						result.FLAGS |= Flags.Carry;
						result.AX = (ushort)machine.DOS.LastError;
					}

					break;
				}
				case Function.FreeAllocatedMemory:
				{
					int address = inputEx.ES * 0x10;

					machine.DOS.TranslateError(() =>
					{
						machine.DOS.MemoryManager.FreeMemory(address);
					});

					if (machine.DOS.LastError != DOSError.None)
					{
						result.FLAGS |= Flags.Carry;
						result.AX = (ushort)machine.DOS.LastError;
					}

					break;
				}
				case Function.SetMemoryBlockSize:
				{
					int address = inputEx.ES * 0x10;
					int newSize = input.BX * MemoryManager.ParagraphSize;

					int largestBlockSize = 0;

					machine.DOS.TranslateError(() =>
					{
						machine.DOS.MemoryManager.ResizeAllocation(address, newSize, out largestBlockSize);
					});

					if (machine.DOS.LastError != DOSError.None)
					{
						result.FLAGS |= Flags.Carry;
						result.AX = (ushort)machine.DOS.LastError;
					}

					break;
				}
				case Function.Function4B:
				{
					var subfunction = (Function4B)al;

					switch (subfunction)
					{
						case Function4B.LoadAndExecuteProgram:
							var fileNameAddress = new SegmentedAddress(inputEx.DS, input.DX);
							var argsAddress = new SegmentedAddress(inputEx.ES, input.BX);

							string fileName = machine.DOS.ReadStringZ(machine.MemoryBus, fileNameAddress.ToLinearAddress()).ToString();
							var parameters = LoadExec.Deserialize(machine.MemoryBus, argsAddress.ToLinearAddress(), machine.DOS);

							if (machine.DOS.LastError != DOSError.None)
								break;

							machine.DOS.ExecuteChildProcess(fileName, parameters);

							break;

						default:
							machine.DOS.SetLastError(DOSError.NotSupported);
							break;
					}

					if (machine.DOS.LastError != DOSError.None)
					{
						result.FLAGS |= Flags.Carry;
						result.AX = (ushort)machine.DOS.LastError;
					}

					break;
				}
				case Function.EndProgram:
				{
					machine.ExitCode = result.AX & 0xFF;
					machine.KeepRunning = false;
					break;
				}
				case Function.GetChildProgramReturnValue:
				{
					// AH is exit reason, but the DOS reasons don't really apply, so leave at 0x00.
					result.AX = unchecked((ushort)(machine.DOS.LastChildProcessExitCode & 0xFF));
					break;
				}
				case Function.FindFirstFile:
				{
					var searchPatternAddress = new SegmentedAddress(inputEx.DS, input.DX);

					var searchPattern = machine.DOS.ReadStringZ(machine.MemoryBus, searchPatternAddress.ToLinearAddress()).ToString();

					FileAttributes attributes = (FileAttributes)input.CX;

					bool success = machine.DOS.FindFirst(searchPattern, attributes);

					if (!success)
					{
						result.FLAGS |= Flags.Carry;
						result.AX = (ushort)machine.DOS.LastError;
					}

					break;
				}
				case Function.FindNextFile:
				{
					bool success = machine.DOS.FindNext();

					if (!success)
					{
						result.FLAGS |= Flags.Carry;
						result.AX = (ushort)machine.DOS.LastError;
					}

					break;
				}
				case Function.SetPSPAddress:
				{
					machine.DOS.CurrentPSPSegment = input.BX;
					break;
				}
				case Function.GetPSPAddress:
				case Function.GetCurrentPSPAddress:
				{
					result.BX = machine.DOS.CurrentPSPSegment;
					break;
				}
				case Function.GetVerifyState:
				{
					result.AX &= 0xFF00;

					if (machine.DOS.VerifyWrites)
						result.AX |= 0x01;

					break;
				}
				case Function.RenameFile:
				{
					var oldNameAddress = new SegmentedAddress(inputEx.DS, input.DX);
					var newNameAddress = new SegmentedAddress(inputEx.ES, input.DI);

					var oldName = machine.DOS.ReadStringZ(machine.MemoryBus, oldNameAddress.ToLinearAddress());
					var newName = machine.DOS.ReadStringZ(machine.MemoryBus, newNameAddress.ToLinearAddress());

					machine.DOS.RenameFile(oldName, newName);

					if (machine.DOS.LastError != DOSError.None)
					{
						result.FLAGS |= Flags.Carry;
						result.AX = (ushort)machine.DOS.LastError;
					}

					break;
				}
				case Function.Function57:
				{
					int fileHandle = input.BX;

					machine.DOS.ClearLastError();

					if ((fileHandle < 0) || (fileHandle >= machine.DOS.Files.Count)
					 || (machine.DOS.Files[fileHandle] is not FileDescriptor fileDescriptor))
						machine.DOS.SetLastError(DOSError.InvalidHandle);
					else if (fileDescriptor is not RegularFileDescriptor regularFileDescriptor)
						machine.DOS.SetLastError(DOSError.InvalidFunction);
					else
					{
						var subfunction = (Function57)al;

						FileTime fileTime = new FileTime() { Raw = input.CX };
						FileDate fileDate = new FileDate() { Raw = input.DX };

						Lazy<DateTime> suppliedDateTime = new Lazy<DateTime>(
							new DateTime(
								fileDate.Year, fileDate.Month, fileDate.Day,
								fileTime.Hour, fileTime.Minute, fileTime.Second));

						DateTime returnDateTime = default;

						machine.DOS.TranslateError(() =>
						{
							switch (subfunction)
							{
								case Function57.GetFileDateAndTime:
									returnDateTime = File.GetLastWriteTime(regularFileDescriptor.Handle);
									break;
								case Function57.SetFileDateAndTime:
									File.SetLastWriteTime(regularFileDescriptor.Handle, suppliedDateTime.Value);
									break;

								case Function57.GetFileLastAccessDateAndTime:
									returnDateTime = File.GetLastAccessTime(regularFileDescriptor.Handle);
									break;
								case Function57.SetFileLastAccessDateAndTime:
									File.SetLastAccessTime(regularFileDescriptor.Handle, suppliedDateTime.Value);
									break;

								case Function57.GetFileCreationDateAndTime:
									returnDateTime = File.GetCreationTime(regularFileDescriptor.Handle);
									break;
								case Function57.SetFileCreationDateAndTime:
									File.SetCreationTime(regularFileDescriptor.Handle, suppliedDateTime.Value);
									break;

								default:
								{
									machine.DOS.SetLastError(DOSError.InvalidFunction);

									result.FLAGS |= Flags.Carry;
									result.AX = (ushort)machine.DOS.LastError;

									break;
								}
							}
						});

						if (machine.DOS.LastError == DOSError.None)
						{
							if (returnDateTime != default)
							{
								fileTime.Set(returnDateTime);
								fileDate.Set(returnDateTime);

								result.CX = fileTime.Raw;
								result.DX = fileDate.Raw;
							}
						}
					}

					if (machine.DOS.LastError != DOSError.None)
					{
						result.FLAGS |= Flags.Carry;
						result.AX = (ushort)machine.DOS.LastError;
					}

					break;
				}
				case Function.Function58:
				{
					var subfunction = (Function58)al;

					switch (subfunction)
					{
						case Function58.GetAllocationStrategy:
						{
							result.AX = (ushort)machine.DOS.MemoryManager.AllocationStrategy;
							break;
						}
						case Function58.SetAllocationStrategy:
						{
							var newStrategy = (MemoryAllocationStrategy)input.BX;

							if (!Enum.IsDefined(newStrategy))
								machine.DOS.SetLastError(DOSError.InvalidFunction);

							if (machine.DOS.LastError == DOSError.None)
								machine.DOS.MemoryManager.AllocationStrategy = newStrategy;
							else
							{
								result.FLAGS |= Flags.Carry;
								result.AX = (ushort)machine.DOS.LastError;
							}

							break;
						}
						case Function58.GetUpperMemoryLink:
						{
							result.AX &= 0xFF00;
							break;
						}
						case Function58.SetUpperMemoryLink:
						{
							machine.DOS.SetLastError(DOSError.NotSupported);

							result.FLAGS |= Flags.Carry;
							result.AX = (ushort)machine.DOS.LastError;

							break;
						}
						default:
						{
							machine.DOS.SetLastError(DOSError.InvalidFunction);

							result.FLAGS |= Flags.Carry;
							result.AX = (ushort)machine.DOS.LastError;

							break;
						}
					}

					break;
				}
				case Function.GetExtendedError:
				{
					result.AX = (ushort)machine.DOS.LastError;
					result.BX = unchecked((ushort)(
						((int)machine.DOS.LastErrorClass << 8) |
						((int)machine.DOS.LastErrorAction)));
					result.CX = (ushort)((int)machine.DOS.LastErrorLocation << 8);

					break;
				}
				case Function.CreateTemporaryFile:
				{
					var pathBufferAddress = new SegmentedAddress(inputEx.DS, input.DX);

					var attributes = (FileAttributes)input.CX;

					var directory = machine.DOS.ReadStringZ(machine.MemoryBus, pathBufferAddress.ToLinearAddress());

					if (machine.DOS.LastError == DOSError.None)
					{
						if (!directory.EndsWith((byte)'\\'))
							machine.DOS.SetLastError(DOSError.InvalidParameter);
						else
						{
							int fileHandle = machine.DOS.CreateTemporaryFile(directory.ToString(), attributes);

							result.AX = (ushort)fileHandle;

							if (machine.DOS.Files[fileHandle] is RegularFileDescriptor regularFile)
							{
								// Append filename onto the caller-supplied buffer.

								string fileName = Path.GetFileName(regularFile.Path);

								int offset = pathBufferAddress.ToLinearAddress() + directory.Length;

								for (int i = 0; i < fileName.Length; i++)
									machine.MemoryBus[offset++] = CP437Encoding.GetByteSemantic(fileName[i]);

								machine.MemoryBus[offset] = 0;
							}
						}
					}

					if (machine.DOS.LastError != DOSError.None)
					{
						result.FLAGS |= Flags.Carry;
						result.AX = (ushort)machine.DOS.LastError;
					}

					break;
				}
				case Function.CreateNewFile:
				{
					int address = inputEx.DS * 0x10 + input.DX;

					var relativePath = machine.DOS.ReadStringZ(machine.MemoryBus, address);

					if (machine.DOS.LastError != DOSError.None)
					{
						result.FLAGS |= Flags.Carry;
						result.AX = (ushort)machine.DOS.LastError;
						break;
					}

					var attributes = (FileAttributes)input.CX;

					int fileHandle = machine.DOS.OpenFile(relativePath.ToString(), FileMode.CreateNew, OpenMode.Access_ReadWrite | OpenMode.Share_Compatibility);

					result.AX = (ushort)fileHandle;

					if (machine.DOS.LastError == DOSError.None)
						machine.DOS.SetFileAttributes(fileHandle, attributes);
					else
					{
						result.FLAGS |= Flags.Carry;
						result.AX = (ushort)machine.DOS.LastError;
					}

					break;
				}
				case Function.LockUnlockFile:
				{
					int fileHandle = input.BX;

					uint offset = unchecked((uint)((input.CX << 16) | input.DX));
					uint length = unchecked((uint)((input.SI << 16) | input.DI));

					bool @lock = (input.AX & 0xFF) == 0;

					if (@lock)
						machine.DOS.LockFile(fileHandle, offset, length);
					else
						machine.DOS.UnlockFile(fileHandle, offset, length);

					if (machine.DOS.LastError != DOSError.None)
					{
						result.FLAGS |= Flags.Carry;
						result.AX = (ushort)machine.DOS.LastError;
					}

					break;
				}
				case Function.SetExtendedError:
				{
					if (al != 0x0A)
						machine.DOS.SetLastError(DOSError.InvalidFunction);
					else
					{
						// This points at an ERROR struct, which is actually a register dump. We only
						// need the AX, BX and CX values, the actual parameters to this function.

						var address = new SegmentedAddress(inputEx.DS, input.SI);

						var stream = new SystemMemoryStream(machine.MemoryBus, address.ToLinearAddress(), 6);
						var reader = new BinaryReader(stream);

						var error = (DOSError)reader.ReadUInt16(); // AX
						var action = (DOSErrorAction)reader.ReadByte(); // BL
						var @class = (DOSErrorClass)reader.ReadByte(); // BH
						var location = (DOSErrorLocation)reader.ReadUInt16(); // CX

						machine.DOS.SetExtendedError(error, @class, action, location);
					}

					break;
				}
				case Function.Function5E:
				{
					var subfunction = (Function5E)al;

					switch (subfunction)
					{
						case Function5E.GetMachineName:
						{
							var address = new SegmentedAddress(inputEx.DS, input.DX);

							var machineName = Environment.MachineName.AsSpan();

							int dot = machineName.IndexOf('.');

							if (dot >= 0)
								machineName = machineName.Slice(0, dot);

							if (machineName.Length > 15)
								machineName = machineName.Slice(0, 15);

							int offset = address.ToLinearAddress();

							for (int i=0; i < machineName.Length; i++)
								machine.MemoryBus[offset++] = CP437Encoding.GetByteSemantic(machineName[i]);
							machine.MemoryBus[offset] = 0;

							break;
						}
						default:
						{
							machine.DOS.SetLastError(DOSError.InvalidFunction);

							result.FLAGS |= Flags.Carry;
							result.AX = (ushort)machine.DOS.LastError;

							break;
						}
					}

					break;
				}
				case Function.Function5F:
				{
					machine.DOS.SetLastError(DOSError.NotSupported);

					result.FLAGS |= Flags.Carry;
					result.AX = (ushort)machine.DOS.LastError;

					break;
				}
				case Function.TrueName:
				{
					var inputAddress = new SegmentedAddress(inputEx.DS, input.SI);

					var inputPath = machine.DOS.ReadStringZ(machine.MemoryBus, inputAddress.ToLinearAddress());

					if (machine.DOS.LastError == DOSError.None)
					{
						var outputPath = machine.DOS.GetCanonicalName(inputPath);

						var outputAddress = new SegmentedAddress(inputEx.ES, input.SI);

						int address = outputAddress.ToLinearAddress();

						var outputSpan = outputPath.AsSpan();

						if (outputSpan.Length > 127)
							outputSpan = outputSpan.Slice(0, 127);

						for (int i = 0; i < outputSpan.Length; i++)
							machine.MemoryBus[address + i] = outputSpan[i];

						machine.MemoryBus[address + outputSpan.Length] = 0;
					}

					if (machine.DOS.LastError != DOSError.None)
					{
						result.FLAGS |= Flags.Carry;
						result.AX = (ushort)machine.DOS.LastError;
					}

					break;
				}
				case Function.Function65:
				{
					var subfunction = (Function65)al;

					switch (subfunction)
					{
						case Function65.GetExtendedCountryInformation:
						{
							int bufferAddress = inputEx.ES * 0x10 + input.DI;

							int codePage = input.BX;

							CountryCode countryCode = (input.DX == 0xFFFF)
								? machine.DOS.CurrentCulture.ToCountryCode()
								: (CountryCode)input.DX;

							int bufferSize = input.CX;

							var countryInfo = new ExtendedCountryInfo();

							var cultureInfo = (input.BX == 0xFFFF)
								? machine.DOS.CurrentCulture
								: CultureUtility.GetCultureInfoForCodePageAndCountry(codePage, countryCode);

							if (cultureInfo == null)
								machine.DOS.SetLastError(DOSError.FileNotFound);
							else if (bufferSize < ExtendedCountryInfo.Size)
								machine.DOS.SetLastError(DOSError.InvalidFunction);
							else
							{
								machine.DOS.TranslateError(() =>
								{
									countryInfo.Import(machine.DOS.CurrentCulture);
									countryInfo.Serialize(machine.MemoryBus, bufferAddress);
								});
							}

							if (machine.DOS.LastError != DOSError.None)
							{
								result.FLAGS |= Flags.Carry;
								result.AX = (ushort)machine.DOS.LastError;
							}

							break;
						}
						case Function65.GetUppercaseTable:
						case Function65.GetFilenameUppercaseTable:
						case Function65.GetFilenameCharacterTable:
						case Function65.GetCollateSequenceTable:
						case Function65.GetDoubleByteCharacterSet:
						{
							int bufferAddress = inputEx.ES * 0x10 + input.DI;

							int codePage = input.BX;

							CountryCode countryCode = (input.DX == 0xFFFF)
								? machine.DOS.CurrentCulture.ToCountryCode()
								: (CountryCode)input.DX;

							int bufferSize = input.CX;

							var cultureInfo = (input.BX == 0xFFFF)
								? machine.DOS.CurrentCulture
								: CultureUtility.GetCultureInfoForCodePageAndCountry(codePage, countryCode);

							if (cultureInfo == null)
								machine.DOS.SetLastError(DOSError.FileNotFound);
							else if (bufferSize < 5)
								machine.DOS.SetLastError(DOSError.InvalidFunction);
							else
							{
								machine.DOS.TranslateError(() =>
								{
									var table =
										subfunction switch
										{
											Function65.GetUppercaseTable => CharacterTables.GetUppercaseTable(cultureInfo),
											Function65.GetFilenameUppercaseTable => CharacterTables.GetFilenameUppercaseTable(cultureInfo),
											Function65.GetFilenameCharacterTable => FileCharTable.Default.ToByteArray(),
											Function65.GetCollateSequenceTable => CharacterTables.GetCollateSequenceTable(cultureInfo),
											Function65.GetDoubleByteCharacterSet => CharacterTables.GetDoubleByteCharacterSet(cultureInfo),

											_ => throw new Exception("Internal error")
										};

									var presentedTableAddress = machine.DOS.PresentData(table);

									var stream = new SystemMemoryStream(machine.MemoryBus, bufferAddress, 5);

									var writer = new BinaryWriter(stream);

									writer.Write((byte)0x02);

									writer.Write(presentedTableAddress.Offset);
									writer.Write(presentedTableAddress.Segment);
								});
							}

							if (machine.DOS.LastError != DOSError.None)
							{
								result.FLAGS |= Flags.Carry;
								result.AX = (ushort)machine.DOS.LastError;
							}

							break;
						}
						case Function65.ConvertCharacter:
						case Function65.ConvertString:
						case Function65.ConvertASCIIZString:
						{
							var uppercaseTable = CharacterTables.GetUppercaseTable(
								machine.DOS.CurrentCulture);

							byte ToUpper(byte ch)
							{
								if ((ch >= 'a') && (ch <= 'z'))
									ch &= (255 - 32);
								else if (ch >= 128)
									ch = uppercaseTable[ch - 128];

								return ch;
							}

							switch (subfunction)
							{
								case Function65.ConvertCharacter:
								{
									input.DX &= 0xFF00;

									byte ch = unchecked((byte)(input.DX & 0xFF));

									ch = ToUpper(ch);

									input.DX |= ch;

									break;
								}
								case Function65.ConvertString:
								case Function65.ConvertASCIIZString:
								{
									var bufferAddress = new SegmentedAddress(inputEx.DS, input.DX);

									int linearAddress = bufferAddress.ToLinearAddress();
									int offset = 0;

									Func<bool> atEnd =
										subfunction switch
										{
											Function65.ConvertString => () => (offset >= input.CX),
											Function65.ConvertASCIIZString => () => (machine.MemoryBus[linearAddress + offset] == 0),

											_ => throw new Exception("Internal error")
										};

									while (!atEnd())
									{
										machine.MemoryBus[linearAddress + offset] = ToUpper(
											machine.MemoryBus[linearAddress + offset]);

										offset++;
									}

									break;
								}
							}

							break;
						}
						default:
						{
							machine.DOS.SetLastError(DOSError.InvalidFunction);

							result.FLAGS |= Flags.Carry;
							result.AX = (ushort)machine.DOS.LastError;

							break;
						}
					}

					break;
				}
				case Function.Function66:
				{
					var subfunction = (Function66)al;

					switch (subfunction)
					{
						case Function66.GetGlobalCodePage:
						{
							ushort codePage = (ushort)machine.DOS.CurrentCulture.TextInfo.OEMCodePage;

							result.BX = codePage;
							result.DX = codePage;

							break;
						}
						case Function66.SetGlobalCodePage:
						{
							int codePage = input.BX;

							var country = machine.DOS.CurrentCulture.ToCountryCode();

							try
							{
								var newCulture = CultureUtility.GetCultureInfoForCodePageAndCountry(codePage, country);

								if (newCulture != null)
									machine.DOS.CurrentCulture = newCulture;
								else
									machine.DOS.SetLastError(DOSError.FileNotFound);
							}
							catch
							{
								machine.DOS.SetLastError(DOSError.FileNotFound);
							}

							if (machine.DOS.LastError != DOSError.None)
							{
								result.FLAGS |= Flags.Carry;
								result.AX = (ushort)machine.DOS.LastError;
							}

							break;
						}
						default:
						{
							machine.DOS.SetLastError(DOSError.InvalidFunction);

							result.FLAGS |= Flags.Carry;
							result.AX = (ushort)machine.DOS.LastError;

							break;
						}
					}

					break;
				}
				case Function.SetMaximumHandleCount:
				{
					int newMax = input.BX;

					if (newMax < DOS.MinimumMaxFiles)
						newMax = DOS.MinimumMaxFiles;

					machine.DOS.MaxFiles = newMax;

					break;
				}
				case Function.CommitFile:
				case Function.CommitFile2:
				{
					int fileHandle = input.BX;

					if ((fileHandle < 0) || (fileHandle >= machine.DOS.Files.Count)
					 || (machine.DOS.Files[fileHandle] is not FileDescriptor fileDescriptor))
						machine.DOS.SetLastError(DOSError.InvalidHandle);
					else
					{
						machine.DOS.TranslateError(() =>
						{
							fileDescriptor.FlushWriteBuffer(flushToDisk: true);
						});
					}

					if (machine.DOS.LastError != DOSError.None)
					{
						result.FLAGS |= Flags.Carry;
						result.AX = (ushort)machine.DOS.LastError;
					}

					break;
				}
				case Function.NullFunction:
				{
					result.AX &= 0xFF00;
					break;
				}
				case Function.ExtendedOpenCreate:
				{
					int address = inputEx.DS * 0x10 + input.SI;

					var relativePath = machine.DOS.ReadStringZ(machine.MemoryBus, address);

					if (machine.DOS.LastError != DOSError.None)
					{
						result.FLAGS |= Flags.Carry;
						result.AX = (ushort)machine.DOS.LastError;
						break;
					}

					var openMode = (OpenMode)input.BX;

					var attributes = (FileAttributes)input.CX;

					var openAction = (OpenAction)input.DX;

					var fileMode = default(FileMode);

					if ((openAction & OpenAction.Truncate) != 0)
					{
						if ((openAction & OpenAction.Create) != 0)
							fileMode = FileMode.Create; // create if doesn't exist, truncate if does
						else
							fileMode = FileMode.Truncate; // error if doesn't exist, truncate if does
					}
					else if ((openAction & OpenAction.Open) != 0)
					{
						if ((openAction & OpenAction.Create) != 0)
							fileMode = FileMode.OpenOrCreate; // create if doesn't exist, open if does
						else
							fileMode = FileMode.Open; // error if doesn't exist, open if does
					}
					else
					{
						if ((openAction & OpenAction.Create) != 0)
							fileMode = FileMode.CreateNew; // create if doesn't exist, error if does
						else
							machine.DOS.SetLastError(DOSError.InvalidParameter); // no action specified
					}

					if (machine.DOS.LastError == DOSError.None)
					{
						int fileHandle = machine.DOS.OpenFile(relativePath.ToString(), fileMode, openMode, attributes, out var actionTaken);

						result.AX = (ushort)fileHandle;
						result.CX = (ushort)actionTaken;
					}

					if (machine.DOS.LastError != DOSError.None)
					{
						result.FLAGS |= Flags.Carry;
						result.AX = (ushort)machine.DOS.LastError;
					}

					break;
				}
			}

			return result;
		}
	}

	/*
TODO: Long filename support?

Int 21/AH=70h - MS-DOS 7 (Windows95) - GET/SET INTERNATIONALIZATION INFORMATION
Int 21/AH=71h - Windows95 - LONG FILENAME FUNCTIONS
Int 21/AX=710Dh - Windows95 - RESET DRIVE
Int 21/AX=7139h - Windows95 - LONG FILENAME - MAKE DIRECTORY
Int 21/AX=713Ah - Windows95 - LONG FILENAME - REMOVE DIRECTORY
Int 21/AX=713Bh - Windows95 - LONG FILENAME - CHANGE DIRECTORY
Int 21/AX=7141h - Windows95 - LONG FILENAME - DELETE FILE
Int 21/AX=7143h - Windows95 - LONG FILENAME - EXTENDED GET/SET FILE ATTRIBUTES
Int 21/AX=7147h - Windows95 - LONG FILENAME - GET CURRENT DIRECTORY
Int 21/AX=714Eh - Windows95 - LONG FILENAME - FIND FIRST MATCHING FILE
Int 21/AX=714Fh - Windows95 - LONG FILENAME - FIND NEXT MATCHING FILE
Int 21/AX=7156h - Windows95 - LONG FILENAME - RENAME FILE
Int 21/AX=7160h/CL=00h - Windows95 - LONG FILENAME - TRUENAME - CANONICALIZE PATH
Int 21/AX=7160h/CL=01h - Windows95 - LONG FILENAME - GET SHORT (8.3) FILENAME FOR FILE
Int 21/AX=7160h/CL=02h - Windows95 - LONG FILENAME - GET CANONICAL LONG FILENAME OR PATH
Int 21/AX=716Ch - Windows95 - LONG FILENAME - CREATE OR OPEN FILE
Int 21/AX=71A0h - Windows95 - LONG FILENAME - GET VOLUME INFORMATION
Int 21/AX=71A1h - Windows95 - LONG FILENAME - FindClose - TERMINATE DIRECTORY SEARCH
Int 21/AX=71A6h - Windows95 - LONG FILENAME - GET FILE INFO BY HANDLE
Int 21/AX=71A7h/BL=00h - Windows95 - LONG FILENAME - FILE TIME TO DOS TIME
Int 21/AX=71A7h/BL=01h - Windows95 - LONG FILENAME - DOS TIME TO FILE TIME
Int 21/AX=71A8h - Windows95 - LONG FILENAME - GENERATE SHORT FILENAME
Int 21/AX=71A9h - Windows95 - LONG FILENAME - SERVER CREATE OR OPEN FILE
Int 21/AX=71AAh/BH=00h - Windows95 - LONG FILENAME - CREATE SUBST
Int 21/AX=71AAh/BH=01h - Windows95 - LONG FILENAME - TERMINATE SUBST
Int 21/AX=71AAh/BH=02h - Windows95 - LONG FILENAME - QUERY SUBST
Int 21/AH=72h - Windows95 beta - LFN-FindClose
Int 21/AH=73h - MS-DOS 7 - DRIVE LOCKING AND FLUSHING
Int 21/AX=7302h - Windows95 - FAT32 - Get_ExtDPB - GET EXTENDED DPB
Int 21/AX=7303h - Windows95 - FAT32 - GET EXTENDED FREE SPACE ON DRIVE
Int 21/AX=7304h - Windows95 - FAT32 - Set DPB TO USE FOR FORMATTING
Int 21/AX=7305h/CX=FFFFh - Windows95 - FAT32 - EXTENDED ABSOLUTE DISK READ/WRITE
Int 21/AH=F8h - DOS v2.11-2.13 - SET OEM INT 21 HANDLER
	 */
}
