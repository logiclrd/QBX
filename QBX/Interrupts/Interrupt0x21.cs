using System.IO;

using QBX.ExecutionEngine.Execution;
using QBX.Hardware;
using QBX.OperatingSystem;
using QBX.OperatingSystem.Breaks;

namespace QBX.Interrupts;

// Based on Ralf Brown's Interrupt List: https://www.ctyme.com/intr/int-21.htm

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
		SetDiskTransferAddress = 0x1A,
		GetDiskTransferAddress = 0x2F,
	}

	public override Registers Execute(Registers input)
	{
		byte ah = unchecked((byte)(input.AX >> 8));
		byte al = unchecked((byte)input.AX);

		var function = (Function)ah;

		var result = input.AsRegistersEx();

		using var suppressionScope = machine.DOS.SuppressExceptionsInScope();

		switch (function)
		{
			case Function.TerminateProgram:
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

				var fcb = FileControlBlock.Deserialize(machine.SystemMemory, offset);

				int fd = machine.DOS.OpenFile(fcb, FileMode.Open);

				fcb.Serialize(machine.SystemMemory);

				result.AX &= 0xFF00;

				if (fd < 0)
					result.AX |= 0xFF;

				break;
			}
			case Function.CloseFileWithFCB:
			{
				int offset = input.AsRegistersEx().DS * 0x10 + input.DX;

				var fcb = FileControlBlock.Deserialize(machine.SystemMemory, offset);

				result.AX &= 0xFF00;

				if (!machine.DOS.CloseFile(fcb.FileHandle))
					result.AX |= 0xFF;

				break;
			}
			case Function.FindFirstFileWithFCB:
			{
				int offset = input.AsRegistersEx().DS * 0x10 + input.DX;

				var fcb = FileControlBlock.Deserialize(machine.SystemMemory, offset);

				result.AX &= 0xFF00;

				if (!machine.DOS.FindFirst(fcb))
					result.AX |= 0xFF;

				break;
			}
			case Function.FindNextFileWithFCB:
			{
				int offset = input.AsRegistersEx().DS * 0x10 + input.DX;

				var fcb = FileControlBlock.Deserialize(machine.SystemMemory, offset);

				result.AX &= 0xFF00;

				if (!machine.DOS.FindNext(fcb))
					result.AX |= 0xFF;

				break;
			}
			case Function.SetDiskTransferAddress:
			{
				machine.DOS.DataTransferAddressSegment = result.DS;
				machine.DOS.DataTransferAddressOffset = result.DX;
				break;
			}
			case Function.GetDiskTransferAddress:
			{
				result.ES = machine.DOS.DataTransferAddressSegment;
				result.BX = machine.DOS.DataTransferAddressOffset;
				break;
			}
		}

		return result;
	}
	/*
TODO:

Int 21/AH=11h - DOS 1+ - FIND FIRST MATCHING FILE USING FCB
Int 21/AH=12h - DOS 1+ - FIND NEXT MATCHING FILE USING FCB
Int 21/AH=13h - DOS 1+ - DELETE FILE USING FCB
Int 21/AH=14h - DOS 1+ - SEQUENTIAL READ FROM FCB FILE
Int 21/AH=15h - DOS 1+ - SEQUENTIAL WRITE TO FCB FILE
Int 21/AH=16h - DOS 1+ - CREATE OR TRUNCATE FILE USING FCB
Int 21/AH=17h - DOS 1+ - RENAME FILE USING FCB
Int 21/AH=18h - DOS 1+ - NULL FUNCTION FOR CP/M COMPATIBILITY
Int 21/AH=19h - DOS 1+ - GET CURRENT DEFAULT DRIVE
Int 21/AH=1Ah - DOS 1+ - SET DISK TRANSFER AREA ADDRESS
Int 21/AH=1Bh - DOS 1+ - GET ALLOCATION INFORMATION FOR DEFAULT DRIVE
Int 21/AH=1Ch - DOS 1+ - GET ALLOCATION INFORMATION FOR SPECIFIC DRIVE
Int 21/AH=1Dh - DOS 1+ - NULL FUNCTION FOR CP/M COMPATIBILITY
Int 21/AH=1Eh - DOS 1+ - NULL FUNCTION FOR CP/M COMPATIBILITY
Int 21/AH=1Fh - DOS 1+ - GET DRIVE PARAMETER BLOCK FOR DEFAULT DRIVE
Int 21/AH=20h - DOS 1+ - NULL FUNCTION FOR CP/M COMPATIBILITY
Int 21/AH=21h - DOS 1+ - READ RANDOM RECORD FROM FCB FILE
Int 21/AH=22h - DOS 1+ - WRITE RANDOM RECORD TO FCB FILE
Int 21/AH=23h - DOS 1+ - GET FILE SIZE FOR FCB
Int 21/AH=24h - DOS 1+ - SET RANDOM RECORD NUMBER FOR FCB
Int 21/AH=25h - DOS 1+ - SET INTERRUPT VECTOR
Int 21/AH=26h - DOS 1+ - CREATE NEW PROGRAM SEGMENT PREFIX
Int 21/AH=27h - DOS 1+ - RANDOM BLOCK READ FROM FCB FILE
Int 21/AH=28h - DOS 1+ - RANDOM BLOCK WRITE TO FCB FILE
Int 21/AH=29h - DOS 1+ - PARSE FILENAME INTO FCB
Int 21/AH=2Ah - DOS 1+ - GET SYSTEM DATE
Int 21/AH=2Bh - DOS 1+ - SET SYSTEM DATE
Int 21/AH=2Ch - DOS 1+ - GET SYSTEM TIME
Int 21/AH=2Dh - DOS 1+ - SET SYSTEM TIME
Int 21/AH=2Eh/DL=00h - DOS 1+ - SET VERIFY FLAG
Int 21/AH=2Fh - DOS 2+ - GET DISK TRANSFER AREA ADDRESS
Int 21/AH=30h - DOS 2+ - GET DOS VERSION
Int 21/AH=31h - DOS 2+ - TERMINATE AND STAY RESIDENT
Int 21/AH=32h - DOS 2+ - GET DOS DRIVE PARAMETER BLOCK FOR SPECIFIC DRIVE
Int 21/AH=33h - DOS 2+ - EXTENDED BREAK CHECKING
Int 21/AX=3302h - DOS 3.x+ internal - GET AND SET EXTENDED CONTROL-BREAK CHECKING STATE
Int 21/AX=3303h - DOS 3.4/4.0 - GET CURRENT CPSW STATE
Int 21/AX=3304h - DOS 3.4/4.0 - SET CPSW STATE
Int 21/AX=3305h - DOS 4.0+ - GET BOOT DRIVE
Int 21/AX=3306h - DOS 5+ - GET TRUE VERSION NUMBER
Int 21/AX=3307h - Windows95 - SET/CLEAR DOS_FLAG
Int 21/AH=34h - DOS 2+ - GET ADDRESS OF INDOS FLAG
Int 21/AH=35h - DOS 2+ - GET INTERRUPT VECTOR
Int 21/AH=36h - DOS 2+ - GET FREE DISK SPACE
Int 21/AH=37h - DOS 2.x and 3.3+ only - AVAILDEV - SPECIFY \DEV\ PREFIX USE
Int 21/AH=38h - DOS 2+ - GET COUNTRY-SPECIFIC INFORMATION
Int 21/AH=38h/DX=FFFFh - DOS 3.0+ - SET COUNTRY CODE
Int 21/AH=39h - DOS 2+ - MKDIR - CREATE SUBDIRECTORY
Int 21/AH=3Ah - DOS 2+ - RMDIR - REMOVE SUBDIRECTORY
Int 21/AH=3Bh - DOS 2+ - CHDIR - SET CURRENT DIRECTORY
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
