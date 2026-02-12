using System;
using System.Globalization;
using System.IO;

using QBX.ExecutionEngine.Execution;
using QBX.Hardware;
using QBX.OperatingSystem;
using QBX.OperatingSystem.Breaks;
using QBX.OperatingSystem.FileStructures;
using QBX.OperatingSystem.Globalization;
using QBX.OperatingSystem.Memory;

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
	}

	public enum Function33 : byte
	{
		GetCtrlCCheckFlag = 0x00,
		SetCtrlCCheckFlag = 0x01, // not implemented
		GetStartupDrive = 0x05,
		GetMSDOSVersion = 0x06,
	}

	public override Registers Execute(Registers input)
	{
		using (machine.DOS.InDOS())
		{
			byte ah = unchecked((byte)(input.AX >> 8));
			byte al = unchecked((byte)input.AX);

			var function = (Function)ah;

			var result = input.AsRegistersEx();

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
					int o = input.AsRegistersEx().DS * 0x10 + result.DX;

					while (true)
					{
						byte b = machine.SystemMemory[o++];

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
					int o = input.AsRegistersEx().DS * 0x10 + result.DX;

					int numBytesDesired = machine.SystemMemory[o];

					o += 2;

					int numBytesRead = 0;

					while (numBytesRead < numBytesDesired)
					{
						byte b = machine.DOS.ReadByte(DOS.StandardInput, echo: false);

						if ((numBytesRead + 1 == numBytesDesired) && (b != 13))
						{
							machine.DOS.Beep();
							continue;
						}

						machine.DOS.WriteByte(DOS.StandardOutput, b, out _);

						machine.SystemMemory[o + numBytesRead] = b;
						numBytesRead++;

						if (b == 13)
							break;
					}

					machine.SystemMemory[o - 1] = (byte)(numBytesRead - 1);

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
					int offset = input.AsRegistersEx().DS * 0x10 + input.DX;

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
					int offset = input.AsRegistersEx().DS * 0x10 + input.DX;

					var fcb = FileControlBlock.Deserialize(machine.MemoryBus, offset);

					result.AX &= 0xFF00;

					if (!machine.DOS.CloseFile(fcb.FileHandle))
						result.AX |= 0xFF;

					break;
				}
				case Function.FindFirstFileWithFCB:
				{
					int offset = input.AsRegistersEx().DS * 0x10 + input.DX;

					var fcb = FileControlBlock.Deserialize(machine.MemoryBus, offset);

					result.AX &= 0xFF00;

					if (!machine.DOS.FindFirst(fcb))
						result.AX |= 0xFF;

					break;
				}
				case Function.FindNextFileWithFCB:
				{
					int offset = input.AsRegistersEx().DS * 0x10 + input.DX;

					var fcb = FileControlBlock.Deserialize(machine.MemoryBus, offset);

					result.AX &= 0xFF00;

					if (!machine.DOS.FindNext(fcb))
						result.AX |= 0xFF;

					break;
				}
				case Function.DeleteFileWithFCB:
				{
					int offset = input.AsRegistersEx().DS * 0x10 + input.DX;

					var fcb = FileControlBlock.Deserialize(machine.MemoryBus, offset);

					result.AX &= 0xFF00;

					machine.DOS.DeleteFile(fcb);

					if (machine.DOS.LastError != DOSError.None)
						result.AX |= 0xFF;

					break;
				}
				case Function.SequentialRead:
				{
					int offset = input.AsRegistersEx().DS * 0x10 + input.DX;

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
					int offset = input.AsRegistersEx().DS * 0x10 + input.DX;

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
					int offset = input.AsRegistersEx().DS * 0x10 + input.DX;

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
					int offset = input.AsRegistersEx().DS * 0x10 + input.DX;

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
						int defaultDrive = char.ToUpperInvariant(Environment.CurrentDirectory[0]) - 'A';

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
				case Function.GetDefaultDriveData:
				{
					input.DX = 0;
					goto case Function.GetDriveData;
				}
				case Function.GetDriveData:
				{
					try
					{
						char driveLetter = (char)((input.DX & 0xFF) + 64);

						if (driveLetter == '@')
							driveLetter = char.ToUpperInvariant(Environment.CurrentDirectory[0]);

						var driveInfo = new DriveInfo(driveLetter.ToString());

						// No way to sensibly map modern numbers into the return from this
						// call, so we just fake a 100MB hard drive or a 1.44MB floppy.

						if (driveInfo.DriveType == DriveType.Removable)
						{
							// TODO: use memory allocator to properly reserve permanent space for this function
							machine.SystemMemory[0x20000 + (driveLetter - 'A')] = 0xF0;

							result.AX = 1; // sectors per cluster
							result.CX = 512; // bytes per sector
							result.DX = 2880; // number of clusters

							result.DS = 0x2000;
							result.BX = (ushort)(driveLetter - 'A');
						}
						else
						{
							// TODO: use memory allocator to properly reserve permanent space for this function
							machine.SystemMemory[0x20000 + (driveLetter - 'A')] = 0xF8;

							result.AX = 4; // sectors per cluster
							result.CX = 512; // bytes per sector
							result.DX = 51200; // number of clusters

							result.DS = 0x2000;
							result.BX = (ushort)(driveLetter - 'A');
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
					machine.DOS.DataTransferAddressSegment = result.DS;
					machine.DOS.DataTransferAddressOffset = result.DX;
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
					int offset = input.AsRegistersEx().DS * 0x10 + input.DX;

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
					int offset = input.AsRegistersEx().DS * 0x10 + input.DX;

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
					int offset = input.AsRegistersEx().DS * 0x10 + input.DX;

					var fcb = FileControlBlock.Deserialize(machine.MemoryBus, offset);

					result.AX &= 0xFF00;

					if (!machine.DOS.PopulateFileInfo(fcb))
						result.AX |= 0xFF;

					fcb.Serialize(machine.MemoryBus);

					break;
				}
				case Function.SetRandomRecordNumber:
				{
					int offset = input.AsRegistersEx().DS * 0x10 + input.DX;

					var fcb = FileControlBlock.Deserialize(machine.MemoryBus, offset);

					result.AX &= 0xFF00;

					if ((fcb.FileHandle == 0) || (fcb.RecordPointer != 0))
						result.AX |= 0xFF;
					else
						fcb.RandomRecordNumber = unchecked((uint)fcb.RecordPointer);

					fcb.Serialize(machine.MemoryBus);

					break;
				}
				case Function.RandomBlockRead:
				{
					int offset = input.AsRegistersEx().DS * 0x10 + input.DX;

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
					int offset = input.AsRegistersEx().DS * 0x10 + input.DX;

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
					int offset = input.AsRegistersEx().ES * 0x10 + input.DI;

					var fcb = FileControlBlock.Deserialize(machine.MemoryBus, offset);

					var flags = unchecked((ParseFlags)input.AX);

					var inputPointer = new SegmentedAddress(result.DS, result.SI);
					int inputLinearAddress = inputPointer.ToLinearAddress();

					FileControlBlock.ParseFileName(
						readInputChar: (idx) => machine.MemoryBus[inputLinearAddress + idx],
						lengthIsAtLeast: (testLength) => (65536 - inputPointer.Offset >= testLength),
						advanceInput: (numBytes) => inputPointer.Offset += (ushort)numBytes,
						ref fcb.DriveIdentifier, fcb.FileNameBytes,
						out bool containsWildcards,
						out bool invalidDriveLetter,
						flags);

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
					result.ES = machine.DOS.DataTransferAddressSegment;
					result.BX = machine.DOS.DataTransferAddressOffset;
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

					if (driveNumber > 26)
						result.AX |= 0xFF;
					else
					{
						var address = machine.DOS.GetDriveParameterBlock(driveNumber);

						result.DS = address.Segment;
						result.BX = address.Offset;
					}

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
								result.DX = 2; // C:\, if something goes wrong
							}

							break;
						}
						case Function33.GetMSDOSVersion:
						{
							result.BX = 5; // MS-DOS version 5.0, which is the MS-DOS Programmer's Reference this code was based on
							result.DX = 0x800; // revision 0, version flag 8: DOS is running from ROM

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

					var dpbAddress = machine.DOS.GetDriveParameterBlock(driveIdentifier);

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
						int offset = input.AsRegistersEx().DS * 0x10 + input.DX;

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
					int address = input.AsRegistersEx().DS * 0x10 + input.DX;

					var directoryName = ReadStringZ(machine.MemoryBus, address);

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
			}

			return result;
		}
	}

	const int MaximumNullTerminatedStringLength = 2048;

	StringValue ReadStringZ(IMemory memory, int address)
	{
		var ret = new StringValue();

		while (memory[address] != 0)
		{
			if (ret.Length == MaximumNullTerminatedStringLength)
				throw new DOSException(DOSError.InvalidData);

			ret.Append(memory[address]);
			address++;
		}

		return ret;
	}
	/*
TODO:

Int 21/AH=3Ch - DOS 2+ - CREAT - CREATE OR TRUNCATE FILE
Int 21/AH=3Dh - DOS 2+ - OPEN - OPEN EXISTING FILE
Int 21/AH=3Eh - DOS 2+ - CLOSE - CLOSE FILE
Int 21/AH=3Fh - DOS 2+ - READ - READ FROM FILE OR DEVICE
Int 21/AH=41h - DOS 2+ - UNLINK - DELETE FILE
Int 21/AH=42h - DOS 2+ - LSEEK - SET CURRENT FILE POSITION
Int 21/AX=4301h - DOS 2+ - CHMOD - SET FILE ATTRIBUTES
Int 21/AX=4302h - MS-DOS 7 - GET COMPRESSED FILE SIZE
Int 21/AX=43FFh/BP=5053h - MS-DOS 7.20 (Win98) - EXTENDED-LENGTH FILENAME OPERATIONS
Int 21/AX=4401h - DOS 2+ - IOCTL - SET DEVICE INFORMATION
Int 21/AX=4403h - DOS 2+ - IOCTL - WRITE TO CHARACTER DEVICE CONTROL CHANNEL
Int 21/AX=4404h - DOS 2+ - IOCTL - READ FROM BLOCK DEVICE CONTROL CHANNEL
Int 21/AX=4405h - DOS 2+ - IOCTL - WRITE TO BLOCK DEVICE CONTROL CHANNEL
Int 21/AX=4406h - DOS 2+ - IOCTL - GET INPUT STATUS
Int 21/AX=4407h - DOS 2+ - IOCTL - GET OUTPUT STATUS
Int 21/AX=4408h - DOS 3.0+ - IOCTL - CHECK IF BLOCK DEVICE REMOVABLE
Int 21/AX=4409h - DOS 3.1+ - IOCTL - CHECK IF BLOCK DEVICE REMOTE
Int 21/AX=440Ah - DOS 3.1+ - IOCTL - CHECK IF HANDLE IS REMOTE
Int 21/AX=440Bh - DOS 3.1+ - IOCTL - SET SHARING RETRY COUNT
Int 21/AX=440Ch - DOS 3.2+ - IOCTL - GENERIC CHARACTER DEVICE REQUEST
Int 21/AX=440Dh - DOS 3.2+ - IOCTL - GENERIC BLOCK DEVICE REQUEST
Int 21/AX=440Dh/CX=084Ah - MS-DOS 7.0+ - GENERIC IOCTL - LOCK LOGICAL VOLUME
Int 21/AX=440Dh/CX=084Bh - MS-DOS 7.0+ - GENERIC IOCTL - LOCK PHYSICAL VOLUME
Int 21/AX=440Dh/CX=086Ah - MS-DOS 7.0+ - GENERIC IOCTL - UNLOCK LOGICAL VOLUME
Int 21/AX=440Dh/CX=086Bh - MS-DOS 7.0+ - GENERIC IOCTL - UNLOCK PHYSICAL VOLUME
Int 21/AX=440Dh/CX=086Ch - MS-DOS 7.0+ - GENERIC IOCTL - GET LOCK FLAG STATE
Int 21/AX=440Dh/CX=086Dh - MS-DOS 7.0+ - GENERIC IOCTL - ENUMERATE OPEN FILES
Int 21/AX=440Dh/CX=086Eh - MS-DOS 7.0+ - GENERIC IOCTL - FIND SWAP FILE
Int 21/AX=440Dh/CX=0870h - MS-DOS 7.0+ - GENERIC IOCTL - GET CURRENT LOCK STATE
Int 21/AX=440Dh/CX=0871h - MS-DOS 7.0+ - GENERIC IOCTL - GET FIRST CLUSTER
Int 21/AX=440Eh - DOS 3.2+ - IOCTL - GET LOGICAL DRIVE MAP
Int 21/AX=440Fh - DOS 3.2+ - IOCTL - SET LOGICAL DRIVE MAP
Int 21/AX=4410h - DOS 5+ - IOCTL - QUERY GENERIC IOCTL CAPABILITY (HANDLE)
Int 21/AX=4411h - DOS 5+ - IOCTL - QUERY GENERIC IOCTL CAPABILITY (DRIVE)
Int 21/AH=45h - DOS 2+ - DUP - DUPLICATE FILE HANDLE
Int 21/AH=46h - DOS 2+ - DUP2, FORCEDUP - FORCE DUPLICATE FILE HANDLE
Int 21/AH=47h - DOS 2+ - CWD - GET CURRENT DIRECTORY
Int 21/AH=48h - DOS 2+ - ALLOCATE MEMORY
Int 21/AH=49h - DOS 2+ - FREE MEMORY
Int 21/AH=4Ah - DOS 2+ - RESIZE MEMORY BLOCK
Int 21/AH=4Bh - DOS 2+ - EXEC - LOAD AND/OR EXECUTE PROGRAM
Int 21/AX=4B05h - DOS 5+ - SET EXECUTION STATE
Int 21/AH=4Ch - DOS 2+ - EXIT - TERMINATE WITH RETURN CODE
Int 21/AH=4Dh - DOS 2+ - GET RETURN CODE (ERRORLEVEL)
Int 21/AH=4Eh - DOS 2+ - FINDFIRST - FIND FIRST MATCHING FILE
Int 21/AH=4Fh - DOS 2+ - FINDNEXT - FIND NEXT MATCHING FILE
Int 21/AH=54h - DOS 2+ - GET VERIFY FLAG
Int 21/AH=56h - DOS 2+ - RENAME - RENAME FILE
Int 21/AX=5700h - DOS 2+ - GET FILE'S LAST-WRITTEN DATE AND TIME
Int 21/AX=5701h - DOS 2+ - SET FILE'S LAST-WRITTEN DATE AND TIME
Int 21/AX=5702h - DOS 4.x only - GET EXTENDED ATTRIBUTES FOR FILE
Int 21/AX=5703h - DOS 4.x only - GET EXTENDED ATTRIBUTE PROPERTIES
Int 21/AX=5704h - DOS 4.x only - SET EXTENDED ATTRIBUTES
Int 21/AX=5704h - MS-DOS 7/Windows95 - GET LAST ACCESS DATE AND TIME
Int 21/AX=5705h - MS-DOS 7/Windows95 - SET LAST ACCESS DATE AND TIME
Int 21/AX=5706h - MS-DOS 7/Windows95 - GET CREATION DATE AND TIME
Int 21/AX=5707h - MS-DOS 7/Windows95 - SET CREATION DATE AND TIME
Int 21/AH=58h - DOS 2.11+ - GET OR SET MEMORY ALLOCATION STRATEGY
Int 21/AH=58h - DOS 5+ - GET OR SET UMB LINK STATE
Int 21/AH=59h/BX=0000h - DOS 3.0+ - GET EXTENDED ERROR INFORMATION
Int 21/AH=5Ah - DOS 3.0+ - CREATE TEMPORARY FILE
Int 21/AH=5Bh - DOS 3.0+ - CREATE NEW FILE
Int 21/AH=5Ch - DOS 3.0+ - FLOCK - RECORD LOCKING
Int 21/AX=5D0Ah - DOS 3.1+ - SET EXTENDED ERROR INFORMATION
Int 21/AX=5F07h - DOS 5+ - ENABLE DRIVE
Int 21/AX=5F08h - DOS 5+ - DISABLE DRIVE
Int 21/AH=60h - DOS 3.0+ - TRUENAME - CANONICALIZE FILENAME OR PATH
Int 21/AH=62h - DOS 3.0+ - GET CURRENT PSP ADDRESS
Int 21/AH=64h - DOS 3.2+ internal - SET DEVICE DRIVER LOOKAHEAD FLAG
Int 21/AX=6500h - Windows95 (OSR2) - SET GENERAL INTERNATIONALIZATION INFO
Int 21/AH=65h - DOS 3.3+ - GET EXTENDED COUNTRY INFORMATION
Int 21/AH=65h - DOS 4.0+ - COUNTRY-DEPENDENT CHARACTER CAPITALIZATION
Int 21/AX=6523h - DOS 4.0+ - DETERMINE IF CHARACTER REPRESENTS YES/NO RESPONSE
Int 21/AX=6601h - DOS 3.3+ - GET GLOBAL CODE PAGE TABLE
Int 21/AX=6602h - DOS 3.3+ - SET GLOBAL CODE PAGE TABLE
Int 21/AH=67h - DOS 3.3+ - SET HANDLE COUNT
Int 21/AH=68h - DOS 3.3+ - FFLUSH - COMMIT FILE
Int 21/AH=69h - DOS 4.0+ internal - GET/SET DISK SERIAL NUMBER
Int 21/AH=6Ah - DOS 4.0+ - COMMIT FILE
Int 21/AH=6Bh - DOS 5+ - NULL FUNCTION
Int 21/AX=6C00h - DOS 4.0+ - EXTENDED OPEN/CREATE
Int 21/AH=6Dh - DOS 5+ ROM - FIND FIRST ROM PROGRAM
Int 21/AH=6Eh - DOS 5+ ROM - FIND NEXT ROM PROGRAM
Int 21/AX=6F00h - DOS 5+ ROM - GET ROM SCAN START ADDRESS
Int 21/AX=6F01h - DOS 5+ ROM - SET ROM SCAN START ADDRESS
Int 21/AX=6F02h - DOS 5+ ROM - GET EXCLUSION REGION LIST
Int 21/AX=6F03h - DOS 5+ ROM - SET EXCLUSION REGION LIST
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
