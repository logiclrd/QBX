using QBX.ExecutionEngine.Execution;
using QBX.Hardware;
using QBX.Interrupts;

namespace QBX.Tests.Interrupts;

public class Interrupt0x21Tests
{
	[Test]
	public void TerminateProgram_should_mark_DOS_as_terminated()
	{
		// Arrange
		var machine = new Machine();

		var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

		var rin = new Registers();

		rin.AX = (int)Interrupt0x21.Function.TerminateProgram << 8;

		Assume.That(machine.DOS.IsTerminated == false);

		var action = () => sut.Execute(rin);

		// Act & Assert
		action.Should().Throw<TerminatedException>();
		machine.DOS.IsTerminated.Should().BeTrue();
	}
	/*
	public enum Function : byte
	{
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
	 */
}
