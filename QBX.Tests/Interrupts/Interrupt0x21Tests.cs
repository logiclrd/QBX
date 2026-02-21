using System.IO.Enumeration;
using System.Numerics;

using QBX.ExecutionEngine.Execution;
using QBX.Firmware.Fonts;
using QBX.Hardware;
using QBX.Interrupts;
using QBX.OperatingSystem.FileDescriptors;
using QBX.OperatingSystem.FileStructures;
using QBX.OperatingSystem.Memory;
using QBX.Tests.Utility;

using SDL3;

using CapturingTextLibrary = QBX.ExecutionEngine.Compiled.Statements.CapturingTextLibrary;

namespace QBX.Tests.Interrupts;

public class Interrupt0x21Tests
{
	static CP437Encoding s_cp437 = new CP437Encoding(ControlCharacterInterpretation.Semantic);

	[Test]
	public void TerminateProgram_should_mark_DOS_as_terminated()
	{
		// Arrange
		var machine = new Machine();

		var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

		var rin = new Registers();

		rin.AX = (int)Interrupt0x21.Function.TerminateProgram << 8;

		Assume.That(machine.DOS.IsTerminated == false);

		// Act & Assert
		var action = () => sut.Execute(rin);

		action.Should().Throw<TerminatedException>();
		machine.DOS.IsTerminated.Should().BeTrue();
		machine.DOS.LastError.Should().Be(OperatingSystem.DOSError.None);
	}

	[Test]
	public void ReadKeyboardWithEcho_should_receive_queued_events()
	{
		DOSReadTest(Interrupt0x21.Function.ReadKeyboardWithEcho, prequeueEvent: true, shouldEcho: true);
	}

	[Test]
	public void ReadKeyboardWithEcho_should_receive_new_events()
	{
		DOSReadTest(Interrupt0x21.Function.ReadKeyboardWithEcho, prequeueEvent: false, shouldEcho: true);
	}

	[Test]
	public void DisplayCharacter_should_send_character_to_display()
	{
		// Arrange
		var machine = new Machine();

		var captureBuffer = new StringValue();

		var capture = new CapturingTextLibrary(machine, captureBuffer);

		machine.VideoFirmware.SetTestingVisualLibrary(capture);

		var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

		var rin = new Registers();

		rin.AX = (int)Interrupt0x21.Function.DisplayCharacter << 8;
		rin.DX = 'U';

		// Act
		var rout = sut.Execute(rin);

		// Assert
		captureBuffer.ToString().Should().Be("U");
		machine.DOS.LastError.Should().Be(OperatingSystem.DOSError.None);
	}

	[Test]
	public void AuxiliaryInput_should_not_hang()
	{
		// Arrange
		var machine = new Machine();

		var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

		var rin = new Registers();

		rin.AX = (int)Interrupt0x21.Function.AuxiliaryInput << 8;

		Registers rout;

		// Act & Assert
		Action action = () => rout = sut.Execute(rin);

		action.ExecutionTime().Should().BeLessThan(TimeSpan.FromSeconds(0.25));
	}

	[Test]
	public void AuxiliaryOutput_should_not_hang()
	{
		// Arrange
		var machine = new Machine();

		var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

		var rin = new Registers();

		rin.AX = (int)Interrupt0x21.Function.AuxiliaryOutput << 8;
		rin.DX = 'I';

		Registers rout;

		// Act & Assert
		Action action = () => rout = sut.Execute(rin);

		action.ExecutionTime().Should().BeLessThan(TimeSpan.FromSeconds(0.25));
	}

	[Test]
	public void PrintCharacter_should_not_hang()
	{
		// Arrange
		var machine = new Machine();

		var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

		var rin = new Registers();

		rin.AX = (int)Interrupt0x21.Function.PrintCharacter << 8;
		rin.DX = 'c';

		Registers rout;

		// Act & Assert
		Action action = () => rout = sut.Execute(rin);

		action.ExecutionTime().Should().BeLessThan(TimeSpan.FromSeconds(0.25));
	}

	[Test]
	public void DirectConsoleIO_should_output_characters()
	{
		// Arrange
		var machine = new Machine();

		var captureBuffer = new StringValue();

		var capture = new CapturingTextLibrary(machine, captureBuffer);

		machine.VideoFirmware.SetTestingVisualLibrary(capture);

		var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

		var rin = new Registers();

		rin.AX = (int)Interrupt0x21.Function.DirectConsoleIO << 8;
		rin.DX = 'k';

		// Act
		var rout = sut.Execute(rin);

		// Assert
		captureBuffer.ToString().Should().Be("k");
		machine.DOS.LastError.Should().Be(OperatingSystem.DOSError.None);
	}

	[Test]
	public void DirectConsoleIO_should_read_queued_input()
	{
		// Arrange
		var machine = new Machine();

		var captureBuffer = new StringValue();

		var capture = new CapturingTextLibrary(machine, captureBuffer);

		machine.VideoFirmware.SetTestingVisualLibrary(capture);

		SimulateTyping("b", default, machine);

		var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

		var rin = new Registers();

		rin.AX = (int)Interrupt0x21.Function.DirectConsoleIO << 8;
		rin.DX = 0xFF;

		Registers? rout = null;

		// Act & Assert
		Action action = () => rout = sut.Execute(rin);

		action.ExecutionTime().Should().BeLessThan(TimeSpan.FromSeconds(0.5));

		rout.Should().NotBeNull();
		rout.FLAGS.Should().NotHaveFlag(Flags.Zero);

		byte characterRead = (byte)(rout.AX & 0xFF);

		captureBuffer.ToString().Should().Be("");
		characterRead.Should().Be((byte)'b');
		machine.DOS.LastError.Should().Be(OperatingSystem.DOSError.None);
	}

	[Test]
	public void DirectConsoleIO_should_not_block_when_input_queue_is_empty()
	{
		// Arrange
		var machine = new Machine();

		var captureBuffer = new StringValue();

		var capture = new CapturingTextLibrary(machine, captureBuffer);

		machine.VideoFirmware.SetTestingVisualLibrary(capture);

		var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

		var rin = new Registers();

		rin.AX = (int)Interrupt0x21.Function.DirectConsoleIO << 8;
		rin.DX = 0xFF;

		Registers? rout = null;

		// Act & Assert
		Action action = () => rout = sut.Execute(rin);

		action.ExecutionTime().Should().BeLessThan(TimeSpan.FromSeconds(50.5));

		rout.Should().NotBeNull();
		rout.FLAGS.Should().HaveFlag(Flags.Zero);

		captureBuffer.ToString().Should().Be("");
		machine.DOS.LastError.Should().Be(OperatingSystem.DOSError.None);
	}

	[Test]
	public void DirectConsoleIO_should_not_break_on_CtrlC()
	{
		// Arrange
		var machine = new Machine();

		var captureBuffer = new StringValue();

		var capture = new CapturingTextLibrary(machine, captureBuffer);

		machine.VideoFirmware.SetTestingVisualLibrary(capture);

		SimulateTyping("\x03" + "b", ControlCharacterHandling.CtrlLetter, machine);

		var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

		var rin = new Registers();

		rin.AX = (int)Interrupt0x21.Function.DirectConsoleIO << 8;
		rin.DX = 0xFF;

		Registers? rout = null;

		// Act & Assert
		Action action = () => rout = sut.Execute(rin);

		action.ExecutionTime().Should().BeLessThan(TimeSpan.FromSeconds(0.5));

		rout.Should().NotBeNull();
		rout.FLAGS.Should().NotHaveFlag(Flags.Zero);

		byte characterRead = (byte)(rout.AX & 0xFF);

		captureBuffer.ToString().Should().Be("");
		characterRead.Should().Be((byte)3);
		machine.DOS.LastError.Should().Be(OperatingSystem.DOSError.None);
	}

	[Test]
	public void DirectConsoleInput_should_receive_queued_events()
	{
		DOSReadTest(Interrupt0x21.Function.DirectConsoleInput, prequeueEvent: true);
	}

	[Test]
	public void DirectConsoleInput_should_receive_new_events()
	{
		DOSReadTest(Interrupt0x21.Function.DirectConsoleInput, prequeueEvent: false);
	}

	[Test]
	public void DirectConsoleInput_should_not_break_on_CtrlC()
	{
		DOSReadTest(Interrupt0x21.Function.DirectConsoleInput, prequeueEvent: false, DOSReadInputType.CtrlC, shouldBreak: false);
	}

	[Test]
	public void DirectConsoleInput_should_not_break_on_CtrlBreak()
	{
		DOSReadTest(Interrupt0x21.Function.DirectConsoleInput, prequeueEvent: false, DOSReadInputType.CtrlBreak, shouldBreak: false);
	}

		[Test]
	public void ReadKeyboardWithoutEcho_should_receive_queued_events()
	{
		DOSReadTest(Interrupt0x21.Function.ReadKeyboardWithoutEcho, prequeueEvent: true);
	}

	[Test]
	public void ReadKeyboardWithoutEcho_should_receive_new_events()
	{
		DOSReadTest(Interrupt0x21.Function.ReadKeyboardWithoutEcho, prequeueEvent: false);
	}

	[Test]
	public void ReadKeyboardWithoutEcho_should_break_on_CtrlC()
	{
		DOSReadTest(Interrupt0x21.Function.ReadKeyboardWithoutEcho, prequeueEvent: false, inputType: DOSReadInputType.CtrlC, shouldBreak: true);
	}

	[Test]
	public void ReadKeyboardWithoutEcho_should_break_on_CtrlBreak()
	{
		DOSReadTest(Interrupt0x21.Function.ReadKeyboardWithoutEcho, prequeueEvent: false, inputType: DOSReadInputType.CtrlBreak, shouldBreak: true);
	}

	[Test]
	public void DisplayString_should_send_string_to_output()
	{
		// Arrange
		var machine = new Machine();

		var captureBuffer = new StringValue();

		var capture = new CapturingTextLibrary(machine, captureBuffer);

		machine.VideoFirmware.SetTestingVisualLibrary(capture);

		string message = "QuickBASIC";

		var messageWithTerminator = new StringValue(message + "$");

		int messageAddress = machine.DOS.MemoryManager.AllocateMemory(messageWithTerminator.Length, machine.DOS.CurrentPSPSegment);

		messageWithTerminator.AsSpan().CopyTo(machine.SystemMemory.AsSpan().Slice(messageAddress));

		var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

		var rin = new RegistersEx();

		rin.AX = (int)Interrupt0x21.Function.DisplayString << 8;
		rin.DS = (ushort)(messageAddress / MemoryManager.ParagraphSize);
		rin.DX = (ushort)(messageAddress - rin.DS * MemoryManager.ParagraphSize);

		// Act
		var rout = sut.Execute(rin);

		// Assert
		captureBuffer.ToString().Should().Be(message);
		machine.DOS.LastError.Should().Be(OperatingSystem.DOSError.None);
	}

	[Test]
	public void BufferedKeyboardInput_should_return_on_Enter()
	{
		// Arrange
		var machine = new Machine();

		var captureBuffer = new StringValue();

		var capture = new CapturingTextLibrary(machine, captureBuffer);

		machine.VideoFirmware.SetTestingVisualLibrary(capture);

		string message = "Quick";

		string messageWithCarriageReturn = message + "\r";

		SimulateTyping(messageWithCarriageReturn, ControlCharacterHandling.SemanticKey, machine);

		byte bufferLength = 8;

		int bufferAddress = machine.DOS.MemoryManager.AllocateMemory(bufferLength + 2, machine.DOS.CurrentPSPSegment);

		var bufferHeader = machine.SystemMemory.AsSpan().Slice(bufferAddress, 2);
		var bufferData = machine.SystemMemory.AsSpan().Slice(bufferAddress + 2, bufferLength);

		bufferHeader[0] = bufferLength;

		var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

		var rin = new RegistersEx();

		rin.AX = (int)Interrupt0x21.Function.BufferedKeyboardInput << 8;
		rin.DS = (ushort)(bufferAddress / MemoryManager.ParagraphSize);
		rin.DX = (ushort)(bufferAddress - rin.DS * MemoryManager.ParagraphSize);

		// Act
		var rout = sut.Execute(rin);

		// Assert
		captureBuffer.ToString().Should().Be(messageWithCarriageReturn);
		bufferHeader[0].Should().Be(bufferLength);
		bufferHeader[1].Should().Be((byte)message.Length);

		string bufferContent = s_cp437.GetString(bufferData.Slice(0, message.Length));

		bufferContent.Should().Be(message);

		machine.DOS.LastError.Should().Be(OperatingSystem.DOSError.None);
	}

	[Test]
	public void BufferedKeyboardInput_should_emit_bell_and_ignore_extra_input_when_too_long()
	{
		// Arrange
		var machine = new Machine();

		var captureBuffer = new StringValue();

		var capture = new CapturingTextLibrary(machine, captureBuffer);

		machine.VideoFirmware.SetTestingVisualLibrary(capture);

		string message = "Quick";

		string messageWithCarriageReturn = message + "\r";

		SimulateTyping(messageWithCarriageReturn, ControlCharacterHandling.SemanticKey, machine);

		byte bufferLength = 4;

		int bufferAddress = machine.DOS.MemoryManager.AllocateMemory(bufferLength + 2, machine.DOS.CurrentPSPSegment);

		var bufferHeader = machine.SystemMemory.AsSpan().Slice(bufferAddress, 2);
		var bufferData = machine.SystemMemory.AsSpan().Slice(bufferAddress + 2, bufferLength);

		bufferHeader[0] = bufferLength;

		string truncatedMessage = message.Substring(0, bufferLength - 1);
		string truncatedMessageWithAlerts = truncatedMessage + new string((char)7, message.Length - (bufferLength - 1));
		string truncatedMessageWithAlertsAndCarriageReturn = truncatedMessageWithAlerts + '\r';

		var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

		var rin = new RegistersEx();

		rin.AX = (int)Interrupt0x21.Function.BufferedKeyboardInput << 8;
		rin.DS = (ushort)(bufferAddress / MemoryManager.ParagraphSize);
		rin.DX = (ushort)(bufferAddress - rin.DS * MemoryManager.ParagraphSize);

		// Act
		var rout = sut.Execute(rin);

		// Assert
		captureBuffer.ToString().Should().Be(truncatedMessageWithAlertsAndCarriageReturn);
		bufferHeader[0].Should().Be(bufferLength);
		bufferHeader[1].Should().Be((byte)truncatedMessage.Length);

		string bufferContent = s_cp437.GetString(bufferData.Slice(0, bufferHeader[1]));

		bufferContent.Should().Be(truncatedMessage);

		machine.DOS.LastError.Should().Be(OperatingSystem.DOSError.None);
	}

	[Test]
	public void CheckKeyboardStatus_should_report_not_ready_correctly()
	{
		// Arrange
		var machine = new Machine();

		var captureBuffer = new StringValue();

		var capture = new CapturingTextLibrary(machine, captureBuffer);

		machine.VideoFirmware.SetTestingVisualLibrary(capture);

		var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

		var rin = new RegistersEx();

		rin.AX = (int)Interrupt0x21.Function.CheckKeyboardStatus << 8;

		// Act
		var rout = sut.Execute(rin);

		// Assert
		int al = rout.AX & 0xFF;

		al.Should().Be(0);
		machine.DOS.LastError.Should().Be(OperatingSystem.DOSError.None);
	}

	[Test]
	public void CheckKeyboardStatus_should_report_ready_correctly()
	{
		// Arrange
		var machine = new Machine();

		var captureBuffer = new StringValue();

		var capture = new CapturingTextLibrary(machine, captureBuffer);

		machine.VideoFirmware.SetTestingVisualLibrary(capture);

		SimulateTyping("Foo", default, machine);

		var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

		var rin = new RegistersEx();

		rin.AX = (int)Interrupt0x21.Function.CheckKeyboardStatus << 8;

		// Act
		var rout = sut.Execute(rin);

		// Assert
		int al = rout.AX & 0xFF;

		al.Should().Be(0xFF);
		machine.DOS.LastError.Should().Be(OperatingSystem.DOSError.None);
	}

	[Test]
	public void FlushBufferReadKeyboard_should_discard_existing_input()
	{
		// Arrange
		var machine = new Machine();

		var captureBuffer = new StringValue();

		var capture = new CapturingTextLibrary(machine, captureBuffer);

		machine.VideoFirmware.SetTestingVisualLibrary(capture);

		SimulateTyping("ignore me", default, machine);

		bool eventQueued = false;

		SDL.Keymod modState = 0;

		machine.Keyboard.GetModStateTestHook += () => modState;

		byte expectedCharacterRead = (byte)'a';
		string expectedOutput = "a";

		var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

		var rin = new Registers();

		rin.AX = (int)Interrupt0x21.Function.FlushBufferReadKeyboard << 8; // main function in ah
		rin.AX |= (int)Interrupt0x21.Function.ReadKeyboardWithEcho; // subfunction in al

		var queueThread = new Thread(
			() =>
			{
				Thread.Sleep(100);
				SimulateTyping("a", default, machine);
				eventQueued = true;
			});

		queueThread.IsBackground = true;
		queueThread.Start();

		Registers? rout = null;

		// Act
		Action action = () => rout = sut.Execute(rin);

		action.ExecutionTime().Should().BeLessThan(TimeSpan.FromSeconds(0.25));

		bool eventQueuedOnReturn = eventQueued;

		// Assert
		rout.Should().NotBeNull();

		byte characterRead = (byte)(rout.AX & 0xFF);

		eventQueuedOnReturn.Should().BeTrue();
		captureBuffer.ToString().Should().Be(expectedOutput);
		characterRead.Should().Be(expectedCharacterRead);
		machine.DOS.LastError.Should().Be(OperatingSystem.DOSError.None);
	}

	[Test]
	public void ResetDrive_should_flush_write_buffers()
	{
		// Arrange
		using (var workspace = new TemporaryDirectory())
		{
			Environment.CurrentDirectory = workspace.Path;

			var machine = new Machine();

			const string TestFileName = "TESTFILE.TXT";

			int fileHandle = machine.DOS.OpenFile(
				TestFileName,
				OperatingSystem.FileStructures.FileMode.Create,
				OpenMode.Access_ReadWrite | OpenMode.Share_DenyNone);

			try
			{
				string testFileActualPath = ((RegularFileDescriptor)machine.DOS.Files[fileHandle]!).PhysicalPath;

				byte[] testData = s_cp437.GetBytes("test");

				machine.DOS.Write(
					fileHandle,
					testData,
					out _);

				var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

				var rin = new Registers();

				rin.AX = (int)Interrupt0x21.Function.ResetDrive << 8;

				// Act
				byte[] contentBefore = FileUtility.ReadAllBytes(testFileActualPath);

				sut.Execute(rin);

				byte[] contentAfter = FileUtility.ReadAllBytes(testFileActualPath);

				// Assert
				contentBefore.Should().BeEmpty();
				contentAfter.Should().BeEquivalentTo(testData);
				machine.DOS.LastError.Should().Be(OperatingSystem.DOSError.None);
			}
			finally
			{
				machine.DOS.CloseAllFiles(keepStandardHandles: false);
			}
		}
	}

	[Test]
	public void OpenFileWithFCB_should_open_files()
	{
		// Arrange
		using (var workspace = new TemporaryDirectory())
		{
			Environment.CurrentDirectory = workspace.Path;

			var machine = new Machine();

			machine.DOS.SetUpRunningProgramSegmentPrefix("");

			const string TestFileName = "TESTFILE.TXT";

			byte[] testData = s_cp437.GetBytes(Guid.NewGuid().ToString());

			File.WriteAllBytes(TestFileName, testData);

			var fcb = new FileControlBlock();

			fcb.SetFileName(TestFileName);

			var fcbAddress = machine.DOS.MemoryManager.AllocateMemory(FileControlBlock.Size, machine.DOS.CurrentPSPSegment);

			fcb.MemoryAddress = fcbAddress;

			fcb.Serialize(machine.MemoryBus);

			try
			{
				var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

				var rin = new RegistersEx();

				rin.AX = (int)Interrupt0x21.Function.OpenFileWithFCB << 8;
				rin.DS = (ushort)(fcbAddress / MemoryManager.ParagraphSize);
				rin.DX = (ushort)(fcbAddress % MemoryManager.ParagraphSize);

				// Act
				var rout = sut.Execute(rin);

				// Assert
				int al = rout.AX & 0xFF;

				al.Should().Be(0);

				fcb = FileControlBlock.Deserialize(machine.MemoryBus, fcbAddress);

				int fileHandle = fcb.FileHandle;

				fileHandle.Should().BeInRange(2, machine.DOS.Files.Count - 1);

				var regularFile = (RegularFileDescriptor)machine.DOS.Files[fileHandle]!;

				regularFile.PhysicalPath.Should().Be(Path.GetFullPath(TestFileName));

				string? pathRoot = Path.GetPathRoot(regularFile.Path) ?? "C:\\";

				if (string.IsNullOrEmpty(pathRoot))
					pathRoot = "C:\\";

				byte expectedDriveIdentifier = (byte)(char.ToUpperInvariant(pathRoot[0]) - 'A' + 1);

				fcb.DriveIdentifier.Should().Be(expectedDriveIdentifier);

				byte[] checkDataBuffer = new byte[testData.Length];

				int offset = 0;

				while (offset < checkDataBuffer.Length)
				{
					int count = regularFile.Read(checkDataBuffer.AsSpan().Slice(offset));

					count.Should().BePositive();

					offset += count;
				}

				checkDataBuffer.AsSpan().ShouldMatch(testData);
			}
			finally
			{
				machine.DOS.CloseFile(fcb);
			}
		}
	}

	[Test]
	public void CloseFileWithFCB_should_close_files()
	{
		// Arrange
		using (var workspace = new TemporaryDirectory())
		{
			Environment.CurrentDirectory = workspace.Path;

			var machine = new Machine();

			machine.DOS.SetUpRunningProgramSegmentPrefix("");

			const string TestFileName = "TESTFILE.TXT";

			byte[] testData = s_cp437.GetBytes("QuickBASIC");

			File.WriteAllBytes(TestFileName, testData);

			var fcb = new FileControlBlock();

			fcb.SetFileName(TestFileName);

			machine.DOS.OpenFile(fcb, OperatingSystem.FileStructures.FileMode.Open);

			int fileHandle = fcb.FileHandle;

			var fcbAddress = machine.DOS.MemoryManager.AllocateMemory(FileControlBlock.Size, machine.DOS.CurrentPSPSegment);

			fcb.MemoryAddress = fcbAddress;

			fcb.Serialize(machine.MemoryBus);

			try
			{
				var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

				var rin = new RegistersEx();

				rin.AX = (int)Interrupt0x21.Function.CloseFileWithFCB << 8;
				rin.DS = (ushort)(fcbAddress / MemoryManager.ParagraphSize);
				rin.DX = (ushort)(fcbAddress % MemoryManager.ParagraphSize);

				// Act
				var fileDescriptorBefore = machine.DOS.Files[fileHandle];

				var rout = sut.Execute(rin);

				var fileDescriptorAfter = machine.DOS.Files[fileHandle];

				// Assert
				int al = rout.AX & 0xFF;

				al.Should().Be(0);

				fileDescriptorBefore.Should().NotBeNull();
				fileDescriptorAfter.Should().BeNull();
			}
			finally
			{
				machine.DOS.CloseFile(fcb);
			}
		}
	}

	[Test]
	public void FindFirstFileWithFCB_should_handle_no_matches()
	{
		// Arrange
		using (var workspace = new TemporaryDirectory())
		{
			Environment.CurrentDirectory = workspace.Path;

			var machine = new Machine();

			machine.DOS.SetUpRunningProgramSegmentPrefix("");

			File.WriteAllText("TEST1.TXT", "");
			File.WriteAllText("TEST23.TXT", "");
			File.WriteAllText("TEST24.TXT", "");
			File.WriteAllText("TEST3.TXT", "");

			var fcb = new FileControlBlock();

			fcb.SetFileName("TEST4*.*");

			var fcbAddress = machine.DOS.MemoryManager.AllocateMemory(FileControlBlock.Size, machine.DOS.CurrentPSPSegment);

			fcb.MemoryAddress = fcbAddress;

			fcb.Serialize(machine.MemoryBus);

			var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

			var rin = new RegistersEx();

			rin.AX = (int)Interrupt0x21.Function.FindFirstFileWithFCB<< 8;
			rin.DS = (ushort)(fcbAddress / MemoryManager.ParagraphSize);
			rin.DX = (ushort)(fcbAddress % MemoryManager.ParagraphSize);

			// Act
			var rout = sut.Execute(rin);

			// Assert
			int al = rout.AX & 0xFF;

			al.Should().Be(0xFF);
		}
	}

	[Test]
	public void FindFirstFileWithFCB_should_find_first_file()
	{
		// Arrange
		using (var workspace = new TemporaryDirectory())
		{
			Environment.CurrentDirectory = workspace.Path;

			var machine = new Machine();

			machine.DOS.SetUpRunningProgramSegmentPrefix("");

			File.WriteAllText("TEST1.TXT", "");
			File.WriteAllText("TEST23.TXT", "");
			File.WriteAllText("TEST24.TXT", "");
			File.WriteAllText("TEST3.TXT", "");

			const string TestPattern = "TEST2*.*";

			var fcb = new FileControlBlock();

			fcb.SetFileName(TestPattern);

			var fcbAddress = machine.DOS.MemoryManager.AllocateMemory(FileControlBlock.Size, machine.DOS.CurrentPSPSegment);

			fcb.MemoryAddress = fcbAddress;

			fcb.Serialize(machine.MemoryBus);

			var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

			var rin = new RegistersEx();

			rin.AX = (int)Interrupt0x21.Function.FindFirstFileWithFCB<< 8;
			rin.DS = (ushort)(fcbAddress / MemoryManager.ParagraphSize);
			rin.DX = (ushort)(fcbAddress % MemoryManager.ParagraphSize);

			// Act
			var rout = sut.Execute(rin);

			// Assert
			int al = rout.AX & 0xFF;

			al.Should().Be(0);

			var dirEntrySpan = machine.SystemMemory.AsSpan().Slice(machine.DOS.DiskTransferAddress, 32);

			string fileName = FileControlBlock.GetFileName(dirEntrySpan.Slice(0, 11));

			var validMatches = Directory.GetFiles(workspace.Path, TestPattern).Select(path => Path.GetFileName(path));

			fileName.Should().BeOneOf(validMatches);
		}
	}

	[Test]
	public void FindNextFileWithFCB_should_find_all_matching_files()
	{
		// Arrange
		using (var workspace = new TemporaryDirectory())
		{
			Environment.CurrentDirectory = workspace.Path;

			var machine = new Machine();

			machine.DOS.SetUpRunningProgramSegmentPrefix("");

			File.WriteAllText("TEST1.TXT", "");
			File.WriteAllText("TEST23.TXT", "");
			File.WriteAllText("TEST24.TXT", "");
			File.WriteAllText("TEST3.TXT", "");

			const string TestPattern = "TEST2*.*";

			var fcb = new FileControlBlock();

			fcb.SetFileName(TestPattern);

			var fcbAddress = machine.DOS.MemoryManager.AllocateMemory(FileControlBlock.Size, machine.DOS.CurrentPSPSegment);

			fcb.MemoryAddress = fcbAddress;

			fcb.Serialize(machine.MemoryBus);

			var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

			var rin = new RegistersEx();

			rin.AX = (int)Interrupt0x21.Function.FindFirstFileWithFCB << 8;
			rin.DS = (ushort)(fcbAddress / MemoryManager.ParagraphSize);
			rin.DX = (ushort)(fcbAddress % MemoryManager.ParagraphSize);

			var rout = sut.Execute(rin);

			var allMatches = new List<string>();

			var dirEntrySpan = machine.SystemMemory.AsSpan().Slice(machine.DOS.DiskTransferAddress, 32);

			string fileName = FileControlBlock.GetFileName(dirEntrySpan.Slice(0, 11));

			allMatches.Add(fileName);

			rin.AX = (int)Interrupt0x21.Function.FindNextFileWithFCB << 8;

			// Act
			rout = sut.Execute(rin);

			while ((rin.AX & 0xFF) == 0)
			{
				fileName = FileControlBlock.GetFileName(dirEntrySpan.Slice(0, 11));

				allMatches.Add(fileName);

				rout = sut.Execute(rin);
			}

			// Assert
			var validMatches = Directory.GetFiles(workspace.Path, TestPattern).Select(path => Path.GetFileName(path));

			allMatches.Should().BeEquivalentTo(validMatches);
		}
	}

	[Test]
	public void DeleteFileWithFCB_should_delete_files()
	{
		// Arrange
		using (var workspace = new TemporaryDirectory())
		{
			Environment.CurrentDirectory = workspace.Path;

			var machine = new Machine();

			machine.DOS.SetUpRunningProgramSegmentPrefix("");

			string[] allFiles = [ "TEST1.TXT", "TEST23.TXT", "TEST24.TXT", "TEST3.TXT" ];

			const string TestFileName = "TEST23.TXT";

			foreach (var fileName in allFiles)
				File.WriteAllText(fileName, "");

			var fcb = new FileControlBlock();

			fcb.SetFileName(TestFileName);

			var fcbAddress = machine.DOS.MemoryManager.AllocateMemory(FileControlBlock.Size, machine.DOS.CurrentPSPSegment);

			fcb.MemoryAddress = fcbAddress;

			fcb.Serialize(machine.MemoryBus);

			try
			{
				var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

				var rin = new RegistersEx();

				rin.AX = (int)Interrupt0x21.Function.DeleteFileWithFCB << 8;
				rin.DS = (ushort)(fcbAddress / MemoryManager.ParagraphSize);
				rin.DX = (ushort)(fcbAddress % MemoryManager.ParagraphSize);

				var expectedFiles = allFiles.Except([TestFileName]);

				// Act
				var rout = sut.Execute(rin);

				// Assert
				int al = rout.AX & 0xFF;

				al.Should().Be(0);

				var actualFiles = Directory.GetFiles(workspace.Path);

				for (int i=0; i < actualFiles.Length; i++)
					actualFiles[i] = Path.GetFileName(actualFiles[i]);

				actualFiles.Should().BeEquivalentTo(expectedFiles);
			}
			finally
			{
				machine.DOS.CloseFile(fcb);
			}
		}
	}

	[Test]
	public void SequentialRead_should_read_partial_record()
	{
		// Arrange
		using (var workspace = new TemporaryDirectory())
		{
			Environment.CurrentDirectory = workspace.Path;

			var machine = new Machine();

			machine.DOS.SetUpRunningProgramSegmentPrefix("");

			const string TestFileName = "TESTFILE.TXT";

			byte[] testData = s_cp437.GetBytes("QuickBASIC");

			File.WriteAllBytes(TestFileName, testData);

			var fcb = new FileControlBlock();

			fcb.RecordSize.Should().BeLessThanOrEqualTo(128); // default DTA size

			fcb.SetFileName(TestFileName);

			machine.DOS.OpenFile(fcb, OperatingSystem.FileStructures.FileMode.Open);

			var fcbAddress = machine.DOS.MemoryManager.AllocateMemory(FileControlBlock.Size, machine.DOS.CurrentPSPSegment);

			fcb.MemoryAddress = fcbAddress;

			fcb.Serialize(machine.MemoryBus);

			try
			{
				var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

				var rin = new RegistersEx();

				rin.AX = (int)Interrupt0x21.Function.SequentialRead << 8;
				rin.DS = (ushort)(fcbAddress / MemoryManager.ParagraphSize);
				rin.DX = (ushort)(fcbAddress % MemoryManager.ParagraphSize);

				var expectedResults = new byte[fcb.RecordSize];

				testData.CopyTo(expectedResults);

				var dta = machine.SystemMemory.AsSpan().Slice(machine.DOS.DiskTransferAddress, fcb.RecordSize);

				dta.Fill(1);

				// Act
				var rout = sut.Execute(rin);

				// Assert
				int al = rout.AX & 0xFF;

				al.Should().Be(0);

				dta.ShouldMatch(expectedResults);
			}
			finally
			{
				machine.DOS.CloseFile(fcb);
			}
		}
	}

	[Test]
	public void SequentialRead_should_read_multiple_records_sequentially()
	{
		// Arrange
		using (var workspace = new TemporaryDirectory())
		{
			Environment.CurrentDirectory = workspace.Path;

			var machine = new Machine();

			machine.DOS.SetUpRunningProgramSegmentPrefix("");

			const string TestFileName = "TESTFILE.TXT";

			var fcb = new FileControlBlock();

			fcb.RecordSize.Should().BeLessThanOrEqualTo(128); // default DTA size

			const int NumRecords = 5;

			byte[][] records = new byte[NumRecords][];

			for (int i=0; i < NumRecords; i++)
			{
				records[i] = new byte[fcb.RecordSize];
				TestContext.CurrentContext.Random.NextBytes(records[i]);
			}

			using (var stream = File.OpenWrite(TestFileName))
			{
				for (int i=0; i < NumRecords; i++)
					stream.Write(records[i]);
			}

			fcb.SetFileName(TestFileName);

			machine.DOS.OpenFile(fcb, OperatingSystem.FileStructures.FileMode.Open);

			var fcbAddress = machine.DOS.MemoryManager.AllocateMemory(FileControlBlock.Size, machine.DOS.CurrentPSPSegment);

			fcb.MemoryAddress = fcbAddress;

			fcb.Serialize(machine.MemoryBus);

			try
			{
				var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

				var rin = new RegistersEx();

				rin.AX = (int)Interrupt0x21.Function.SequentialRead << 8;
				rin.DS = (ushort)(fcbAddress / MemoryManager.ParagraphSize);
				rin.DX = (ushort)(fcbAddress % MemoryManager.ParagraphSize);

				var expectedResults = new byte[fcb.RecordSize];

				var dta = machine.SystemMemory.AsSpan().Slice(machine.DOS.DiskTransferAddress, fcb.RecordSize);

				dta.Fill(1);

				int[] returnCodes = new int[NumRecords];
				byte[][] actualData = new byte[NumRecords][];

				// Act
				for (int i=0; i < NumRecords; i++)
				{
					var rout = sut.Execute(rin);

					returnCodes[i] = rout.AX & 0xFF;
					actualData[i] = dta.ToArray();
				}

				// Assert
				returnCodes.Should().AllBeEquivalentTo(0);

				for (int i=0; i < NumRecords; i++)
					actualData[i].ShouldMatch(records[i]);
			}
			finally
			{
				machine.DOS.CloseFile(fcb);
			}
		}
	}

	[Test]
	public void SequentialWrite_should_write_multiple_records_sequentially()
	{
		// Arrange
		using (var workspace = new TemporaryDirectory())
		{
			Environment.CurrentDirectory = workspace.Path;

			var machine = new Machine();

			machine.DOS.SetUpRunningProgramSegmentPrefix("");

			const string TestFileName = "TESTFILE.TXT";

			var fcb = new FileControlBlock();

			fcb.RecordSize.Should().BeLessThanOrEqualTo(128); // default DTA size

			const int NumRecords = 5;

			byte[][] records = new byte[NumRecords][];

			for (int i=0; i < NumRecords; i++)
			{
				records[i] = new byte[fcb.RecordSize];
				TestContext.CurrentContext.Random.NextBytes(records[i]);
			}

			fcb.SetFileName(TestFileName);

			machine.DOS.OpenFile(fcb, OperatingSystem.FileStructures.FileMode.CreateNew);

			var fcbAddress = machine.DOS.MemoryManager.AllocateMemory(FileControlBlock.Size, machine.DOS.CurrentPSPSegment);

			fcb.MemoryAddress = fcbAddress;

			fcb.Serialize(machine.MemoryBus);

			int[] returnCodes = new int[NumRecords];
			byte[][] actualData = new byte[NumRecords][];

			try
			{
				var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

				var rin = new RegistersEx();

				rin.AX = (int)Interrupt0x21.Function.SequentialWrite << 8;
				rin.DS = (ushort)(fcbAddress / MemoryManager.ParagraphSize);
				rin.DX = (ushort)(fcbAddress % MemoryManager.ParagraphSize);

				var dta = machine.SystemMemory.AsSpan().Slice(machine.DOS.DiskTransferAddress, fcb.RecordSize);

				// Act
				for (int i=0; i < NumRecords; i++)
				{
					records[i].CopyTo(dta);

					var rout = sut.Execute(rin);

					returnCodes[i] = rout.AX & 0xFF;
				}
			}
			finally
			{
				machine.DOS.CloseFile(fcb);
			}

			// Assert
			returnCodes.Should().AllBeEquivalentTo(0);

			using (var file = File.OpenRead(TestFileName))
			{
				byte[] buffer = new byte[fcb.RecordSize];

				for (int i=0; i < NumRecords; i++)
				{
					file.ReadExactly(buffer);

					buffer.ShouldMatch(records[i]);
				}
			}
		}
	}

	[Test]
	public void CreateFileWithFCB_should_create_files()
	{
		// Arrange
		using (var workspace = new TemporaryDirectory())
		{
			Environment.CurrentDirectory = workspace.Path;

			var machine = new Machine();

			machine.DOS.SetUpRunningProgramSegmentPrefix("");

			const string TestFileName = "TESTFILE.TXT";

			var fcb = new FileControlBlock();

			fcb.SetFileName(TestFileName);

			var fcbAddress = machine.DOS.MemoryManager.AllocateMemory(FileControlBlock.Size, machine.DOS.CurrentPSPSegment);

			fcb.MemoryAddress = fcbAddress;

			fcb.Serialize(machine.MemoryBus);

			try
			{
				var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

				var rin = new RegistersEx();

				rin.AX = (int)Interrupt0x21.Function.CreateFileWithFCB << 8;
				rin.DS = (ushort)(fcbAddress / MemoryManager.ParagraphSize);
				rin.DX = (ushort)(fcbAddress % MemoryManager.ParagraphSize);

				// Act
				bool existsBefore = File.Exists(TestFileName);

				var rout = sut.Execute(rin);

				bool existsAfter = File.Exists(TestFileName);

				// Assert
				int al = rout.AX & 0xFF;

				al.Should().Be(0);

				existsBefore.Should().BeFalse();
				existsAfter.Should().BeTrue();

				fcb = FileControlBlock.Deserialize(machine.MemoryBus, fcbAddress);

				int fileHandle = fcb.FileHandle;

				fileHandle.Should().BeInRange(2, machine.DOS.Files.Count - 1);

				var regularFile = (RegularFileDescriptor)machine.DOS.Files[fileHandle]!;

				regularFile.PhysicalPath.Should().Be(Path.GetFullPath(TestFileName));

				string? pathRoot = Path.GetPathRoot(regularFile.Path) ?? "C:\\";

				if (string.IsNullOrEmpty(pathRoot))
					pathRoot = "C:\\";

				byte expectedDriveIdentifier = (byte)(char.ToUpperInvariant(pathRoot[0]) - 'A' + 1);

				fcb.DriveIdentifier.Should().Be(expectedDriveIdentifier);
			}
			finally
			{
				machine.DOS.CloseFile(fcb);
			}
		}
	}

	[Test]
	public void CreateFileWithFCB_should_truncate_existing_files()
	{
		// Arrange
		using (var workspace = new TemporaryDirectory())
		{
			Environment.CurrentDirectory = workspace.Path;

			var machine = new Machine();

			machine.DOS.SetUpRunningProgramSegmentPrefix("");

			const string TestFileName = "TESTFILE.TXT";

			File.WriteAllText(TestFileName, "QuickBASIC");

			var fcb = new FileControlBlock();

			fcb.SetFileName(TestFileName);

			var fcbAddress = machine.DOS.MemoryManager.AllocateMemory(FileControlBlock.Size, machine.DOS.CurrentPSPSegment);

			fcb.MemoryAddress = fcbAddress;

			fcb.Serialize(machine.MemoryBus);

			try
			{
				var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

				var rin = new RegistersEx();

				rin.AX = (int)Interrupt0x21.Function.CreateFileWithFCB << 8;
				rin.DS = (ushort)(fcbAddress / MemoryManager.ParagraphSize);
				rin.DX = (ushort)(fcbAddress % MemoryManager.ParagraphSize);

				// Act
				bool existsBefore = File.Exists(TestFileName);

				var rout = sut.Execute(rin);

				bool existsAfter = File.Exists(TestFileName);

				// Assert
				int al = rout.AX & 0xFF;

				al.Should().Be(0);

				existsBefore.Should().BeTrue();
				existsAfter.Should().BeTrue();

				fcb = FileControlBlock.Deserialize(machine.MemoryBus, fcbAddress);

				int fileHandle = fcb.FileHandle;

				fileHandle.Should().BeInRange(2, machine.DOS.Files.Count - 1);

				var regularFile = (RegularFileDescriptor)machine.DOS.Files[fileHandle]!;

				regularFile.PhysicalPath.Should().Be(Path.GetFullPath(TestFileName));

				string? pathRoot = Path.GetPathRoot(regularFile.Path) ?? "C:\\";

				if (string.IsNullOrEmpty(pathRoot))
					pathRoot = "C:\\";

				byte expectedDriveIdentifier = (byte)(char.ToUpperInvariant(pathRoot[0]) - 'A' + 1);

				fcb.DriveIdentifier.Should().Be(expectedDriveIdentifier);

				new FileInfo(regularFile.PhysicalPath).Length.Should().Be(0);
			}
			finally
			{
				machine.DOS.CloseFile(fcb);
			}
		}
	}

	[Test]
	public void RenameFileWithFCB_should_rename_individual_file()
	{
		// Arrange
		using (var workspace = new TemporaryDirectory())
		{
			Environment.CurrentDirectory = workspace.Path;

			var machine = new Machine();

			machine.DOS.SetUpRunningProgramSegmentPrefix("");

			const string TestOldFileName = "TESTFILE.TXT";
			const string TestNewFileName = "TEST2.TXT";

			File.WriteAllText(TestOldFileName, "QuickBASIC");

			var rfcb = new RenameFileControlBlock();

			rfcb.SetOldFileName(TestOldFileName);
			rfcb.SetNewFileName(TestNewFileName);

			var rfcbAddress = machine.DOS.MemoryManager.AllocateMemory(RenameFileControlBlock.Size, machine.DOS.CurrentPSPSegment);

			rfcb.Serialize(machine.MemoryBus, rfcbAddress);

			var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

			var rin = new RegistersEx();

			rin.AX = (int)Interrupt0x21.Function.RenameFileWithFCB << 8;
			rin.DS = (ushort)(rfcbAddress / MemoryManager.ParagraphSize);
			rin.DX = (ushort)(rfcbAddress % MemoryManager.ParagraphSize);

			// Act
			bool oldExistsBefore = File.Exists(TestOldFileName);
			bool newExistsBefore = File.Exists(TestNewFileName);

			var rout = sut.Execute(rin);

			bool oldExistsAfter = File.Exists(TestOldFileName);
			bool newExistsAfter = File.Exists(TestNewFileName);

			// Assert
			int al = rout.AX & 0xFF;

			al.Should().Be(0);

			oldExistsBefore.Should().BeTrue();
			oldExistsAfter.Should().BeFalse();

			newExistsBefore.Should().BeFalse();
			newExistsAfter.Should().BeTrue();
		}
	}

	[Test]
	public void RenameFileWithFCB_should_rename_multiple_matching_files()
	{
		// Arrange
		using (var workspace = new TemporaryDirectory())
		{
			Environment.CurrentDirectory = workspace.Path;

			var machine = new Machine();

			machine.DOS.SetUpRunningProgramSegmentPrefix("");

			const string TestOldFileName = "TEST2*.TXT";
			const string TestNewFileName = "TEST2*.BIN";

			var fileSet = new List<string>();

			fileSet.Add("TEST1.TXT");
			fileSet.Add("TEST23.TXT");
			fileSet.Add("TEST24.TXT");
			fileSet.Add("TEST3.TXT");

			fileSet.ForEach(fileName => File.WriteAllText(fileName, ""));

			var expectedNewFileSet = new List<string>();

			foreach (var fileName in fileSet)
			{
				if (FileSystemName.MatchesSimpleExpression(TestOldFileName, fileName))
					expectedNewFileSet.Add(Path.ChangeExtension(fileName, ".BIN"));
				else
					expectedNewFileSet.Add(fileName);
			}

			var rfcb = new RenameFileControlBlock();

			rfcb.SetOldFileName(TestOldFileName);
			rfcb.SetNewFileName(TestNewFileName);

			var rfcbAddress = machine.DOS.MemoryManager.AllocateMemory(RenameFileControlBlock.Size, machine.DOS.CurrentPSPSegment);

			rfcb.Serialize(machine.MemoryBus, rfcbAddress);

			var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

			var rin = new RegistersEx();

			rin.AX = (int)Interrupt0x21.Function.RenameFileWithFCB << 8;
			rin.DS = (ushort)(rfcbAddress / MemoryManager.ParagraphSize);
			rin.DX = (ushort)(rfcbAddress % MemoryManager.ParagraphSize);

			// Act
			var rout = sut.Execute(rin);

			// Assert
			int al = rout.AX & 0xFF;

			al.Should().Be(0);

			var newFileSet = Directory.GetFiles(workspace.Path).Select(path => Path.GetFileName(path));

			newFileSet.Should().BeEquivalentTo(expectedNewFileSet);
		}
	}

	[Test]
	public void RenameFileWithFCB_should_fail_if_old_filename_does_not_exist()
	{
		// Arrange
		using (var workspace = new TemporaryDirectory())
		{
			Environment.CurrentDirectory = workspace.Path;

			var machine = new Machine();

			machine.DOS.SetUpRunningProgramSegmentPrefix("");

			const string TestOldFileName = "TESTFILE.TXT";
			const string TestNewFileName = "TEST2.TXT";

			var fcb = new RenameFileControlBlock();

			fcb.SetOldFileName(TestOldFileName);
			fcb.SetNewFileName(TestNewFileName);

			var fcbAddress = machine.DOS.MemoryManager.AllocateMemory(RenameFileControlBlock.Size, machine.DOS.CurrentPSPSegment);

			fcb.Serialize(machine.MemoryBus, fcbAddress);

			var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

			var rin = new RegistersEx();

			rin.AX = (int)Interrupt0x21.Function.RenameFileWithFCB << 8;
			rin.DS = (ushort)(fcbAddress / MemoryManager.ParagraphSize);
			rin.DX = (ushort)(fcbAddress % MemoryManager.ParagraphSize);

			// Act
			bool oldExistsBefore = File.Exists(TestOldFileName);
			bool newExistsBefore = File.Exists(TestNewFileName);

			var rout = sut.Execute(rin);

			bool oldExistsAfter = File.Exists(TestOldFileName);
			bool newExistsAfter = File.Exists(TestNewFileName);

			// Assert
			int al = rout.AX & 0xFF;

			al.Should().Be(0xFF);

			oldExistsBefore.Should().BeFalse();
			oldExistsAfter.Should().BeFalse();

			newExistsBefore.Should().BeFalse();
			newExistsAfter.Should().BeFalse();
		}
	}

	[Test]
	public void RenameFileWithFCB_should_fail_if_new_filename_already_in_use()
	{
		// Arrange
		using (var workspace = new TemporaryDirectory())
		{
			Environment.CurrentDirectory = workspace.Path;

			var machine = new Machine();

			machine.DOS.SetUpRunningProgramSegmentPrefix("");

			const string TestOldFileName = "TESTFILE.TXT";
			const string TestNewFileName = "TEST2.TXT";

			const string SubjectFileContent = "QuickBasic";
			const string BlockerFileContent = "Turbo Pascal";

			File.WriteAllText(TestOldFileName, SubjectFileContent);
			File.WriteAllText(TestNewFileName, BlockerFileContent);

			var fcb = new RenameFileControlBlock();

			fcb.SetOldFileName(TestOldFileName);
			fcb.SetNewFileName(TestNewFileName);

			var fcbAddress = machine.DOS.MemoryManager.AllocateMemory(RenameFileControlBlock.Size, machine.DOS.CurrentPSPSegment);

			fcb.Serialize(machine.MemoryBus, fcbAddress);

			var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

			var rin = new RegistersEx();

			rin.AX = (int)Interrupt0x21.Function.RenameFileWithFCB << 8;
			rin.DS = (ushort)(fcbAddress / MemoryManager.ParagraphSize);
			rin.DX = (ushort)(fcbAddress % MemoryManager.ParagraphSize);

			// Act
			bool oldExistsBefore = File.Exists(TestOldFileName);
			bool newExistsBefore = File.Exists(TestNewFileName);

			var rout = sut.Execute(rin);

			bool oldExistsAfter = File.Exists(TestOldFileName);
			bool newExistsAfter = File.Exists(TestNewFileName);

			// Assert
			int al = rout.AX & 0xFF;

			al.Should().Be(0xFF);

			oldExistsBefore.Should().BeTrue();
			oldExistsAfter.Should().BeTrue();

			newExistsBefore.Should().BeTrue();
			newExistsAfter.Should().BeTrue();

			string content = File.ReadAllText(TestNewFileName);

			content.Should().Be(BlockerFileContent);
		}
	}

	[Test]
	public void RenameFileWithFCB_should_not_interfere_with_new_filename_when_old_filename_does_not_exist()
	{
		// Arrange
		using (var workspace = new TemporaryDirectory())
		{
			Environment.CurrentDirectory = workspace.Path;

			var machine = new Machine();

			machine.DOS.SetUpRunningProgramSegmentPrefix("");

			const string TestOldFileName = "TESTFILE.TXT";
			const string TestNewFileName = "TEST2.TXT";

			const string BlockerFileContent = "Turbo Pascal";

			File.WriteAllText(TestNewFileName, BlockerFileContent);

			var fcb = new RenameFileControlBlock();

			fcb.SetOldFileName(TestOldFileName);
			fcb.SetNewFileName(TestNewFileName);

			var fcbAddress = machine.DOS.MemoryManager.AllocateMemory(RenameFileControlBlock.Size, machine.DOS.CurrentPSPSegment);

			fcb.Serialize(machine.MemoryBus, fcbAddress);

			var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

			var rin = new RegistersEx();

			rin.AX = (int)Interrupt0x21.Function.RenameFileWithFCB << 8;
			rin.DS = (ushort)(fcbAddress / MemoryManager.ParagraphSize);
			rin.DX = (ushort)(fcbAddress % MemoryManager.ParagraphSize);

			// Act
			bool oldExistsBefore = File.Exists(TestOldFileName);
			bool newExistsBefore = File.Exists(TestNewFileName);

			var rout = sut.Execute(rin);

			bool oldExistsAfter = File.Exists(TestOldFileName);
			bool newExistsAfter = File.Exists(TestNewFileName);

			// Assert
			int al = rout.AX & 0xFF;

			al.Should().Be(0xFF);

			oldExistsBefore.Should().BeFalse();
			oldExistsAfter.Should().BeFalse();

			newExistsBefore.Should().BeTrue();
			newExistsAfter.Should().BeTrue();

			string content = File.ReadAllText(TestNewFileName);

			content.Should().Be(BlockerFileContent);
		}
	}

	[Test]
	public void GetDefaultDrive_should_return_correct_drive_letter()
	{
		// Arrange
		var machine = new Machine();

		machine.DOS.SetUpRunningProgramSegmentPrefix("");

		var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

		var rin = new RegistersEx();

		rin.AX = (int)Interrupt0x21.Function.GetDefaultDrive << 8;

		// Act
		var rout = sut.Execute(rin);

		// Assert
		int al = rout.AX & 0xFF;

		int driveIdentifier;

		if ((Path.GetPathRoot(Environment.CurrentDirectory) is string pathRoot)
		 && (pathRoot.Length >= 2)
		 && (pathRoot[1] == Path.VolumeSeparatorChar))
			driveIdentifier = char.ToUpperInvariant(pathRoot[0]) - 'A';
		else
			driveIdentifier = 2; // "C:/" synthetic drive on platforms with no drive letters

		al.Should().Be(driveIdentifier);
	}

	[Test]
	public void SetDiskTransferAddress_should_set_disk_transfer_address()
	{
		// Arrange
		using (var workspace = new TemporaryDirectory())
		{
			Environment.CurrentDirectory = workspace.Path;

			var machine = new Machine();

			machine.DOS.SetUpRunningProgramSegmentPrefix("");

			const string TestFileName = "TESTFILE.TXT";

			var fcb = new FileControlBlock();

			fcb.RecordSize.Should().BeLessThanOrEqualTo(128); // default DTA size

			const int NumRecords = 5;

			byte[][] records = new byte[NumRecords][];

			for (int i=0; i < NumRecords; i++)
			{
				records[i] = new byte[fcb.RecordSize];
				TestContext.CurrentContext.Random.NextBytes(records[i]);
			}

			using (var stream = File.OpenWrite(TestFileName))
			{
				for (int i=0; i < NumRecords; i++)
					stream.Write(records[i]);
			}

			fcb.SetFileName(TestFileName);

			machine.DOS.OpenFile(fcb, OperatingSystem.FileStructures.FileMode.Open);

			try
			{
				int[] dtaAddresses = new int[NumRecords];

				for (int i=0; i < NumRecords; i++)
					dtaAddresses[i] = machine.DOS.MemoryManager.AllocateMemory(fcb.RecordSize, machine.DOS.CurrentPSPSegment);

				var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

				var expectedResults = new byte[fcb.RecordSize];

				int[] returnCodes = new int[NumRecords];
				byte[][] actualData = new byte[NumRecords][];

				// Act
				for (int i=0; i < NumRecords; i++)
				{
					var rin = new RegistersEx();

					rin.AX = (int)Interrupt0x21.Function.SetDiskTransferAddress << 8;
					rin.DS = (ushort)(dtaAddresses[i] / MemoryManager.ParagraphSize);
					rin.DX = (ushort)(dtaAddresses[i] % MemoryManager.ParagraphSize);

					sut.Execute(rin);

					machine.DOS.ReadRecord(fcb, advance: true);
				}

				// Assert
				for (int i=0; i < NumRecords; i++)
				{
					var dtaSpan = machine.SystemMemory.AsSpan().Slice(dtaAddresses[i], fcb.RecordSize);

					dtaSpan.ShouldMatch(records[i]);
				}
			}
			finally
			{
				machine.DOS.CloseFile(fcb);
			}
		}
	}

	[Test]
	public void GetDefaultDriveData_should_return_some_values()
	{
		// Arrange
		var machine = new Machine();

		machine.DOS.SetUpRunningProgramSegmentPrefix("");

		var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

		var rin = new RegistersEx();

		rin.AX = (int)Interrupt0x21.Function.GetDefaultDriveData << 8;

		// Act
		var rout = sut.Execute(rin);

		// Assert
		BitOperations.PopCount(rout.AX).Should().Be(1);
		BitOperations.PopCount(rout.CX).Should().Be(1);
		rout.DX.Should().BeGreaterThan(1000);

		var routEx = rout.AsRegistersEx();

		var mediaDescriptorAddress = new SegmentedAddress(routEx.DS, rout.BX);

		var mediaDescriptor = (MediaDescriptor)machine.MemoryBus[mediaDescriptorAddress.ToLinearAddress()];

		mediaDescriptor.Should().Be(MediaDescriptor.FixedDisk);
	}

	[Test]
	public void GetDriveData_should_return_some_values_for_default_drive()
	{
		// Arrange
		var machine = new Machine();

		machine.DOS.SetUpRunningProgramSegmentPrefix("");

		var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

		var rin = new RegistersEx();

		rin.AX = (int)Interrupt0x21.Function.GetDriveData << 8;
		rin.DX = 0;

		// Act
		var rout = sut.Execute(rin);

		// Assert
		BitOperations.PopCount(rout.AX).Should().Be(1);
		BitOperations.PopCount(rout.CX).Should().Be(1);
		rout.DX.Should().BeGreaterThan(1000);

		var routEx = rout.AsRegistersEx();

		var mediaDescriptorAddress = new SegmentedAddress(routEx.DS, rout.BX);

		var mediaDescriptor = (MediaDescriptor)machine.MemoryBus[mediaDescriptorAddress.ToLinearAddress()];

		mediaDescriptor.Should().Be(MediaDescriptor.FixedDisk);
	}

	[Test]
	public void GetDriveData_should_return_some_values_for_specified_drive()
	{
		// Arrange
		var machine = new Machine();

		machine.DOS.SetUpRunningProgramSegmentPrefix("");

		var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

		var rin = new RegistersEx();

		rin.AX = (int)Interrupt0x21.Function.GetDriveData << 8;
		rin.DX = (ushort)(machine.DOS.GetDefaultDrive() + 1);

		rin.DX.Should().BeInRange(1, 26);

		// Act
		var rout = sut.Execute(rin);

		// Assert
		BitOperations.PopCount(rout.AX).Should().Be(1);
		BitOperations.PopCount(rout.CX).Should().Be(1);
		rout.DX.Should().BeGreaterThan(1000);

		var routEx = rout.AsRegistersEx();

		var mediaDescriptorAddress = new SegmentedAddress(routEx.DS, rout.BX);

		var mediaDescriptor = (MediaDescriptor)machine.MemoryBus[mediaDescriptorAddress.ToLinearAddress()];

		mediaDescriptor.Should().Be(MediaDescriptor.FixedDisk);
	}

	[Test]
	public void GetDefaultDPB_should_return_correct_memory_address()
	{
		// Arrange
		var machine = new Machine();

		machine.DOS.SetUpRunningProgramSegmentPrefix("");

		var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

		var rin = new RegistersEx();

		rin.AX = (int)Interrupt0x21.Function.GetDefaultDPB << 8;

		var expectedAddress = machine.DOS.GetDefaultDriveParameterBlock();

		// Act
		var rout = sut.Execute(rin);

		// Assert
		int al = rout.AX & 0xFF;

		al.Should().Be(0);

		var routEx = rout.AsRegistersEx();

		var returnedAddress = new SegmentedAddress(routEx.DS, rout.BX);

		returnedAddress.Should().Be(expectedAddress);
	}

	[Test]
	public void RandomRead_should_read_multiple_records_in_arbitrary_order()
	{
		// Arrange
		using (var workspace = new TemporaryDirectory())
		{
			Environment.CurrentDirectory = workspace.Path;

			var machine = new Machine();

			machine.DOS.SetUpRunningProgramSegmentPrefix("");

			const string TestFileName = "TESTFILE.TXT";

			var fcb = new FileControlBlock();

			fcb.RecordSize.Should().BeLessThanOrEqualTo(128); // default DTA size

			const int NumRecords = 5;

			byte[][] records = new byte[NumRecords][];

			for (int i=0; i < NumRecords; i++)
			{
				records[i] = new byte[fcb.RecordSize];
				TestContext.CurrentContext.Random.NextBytes(records[i]);
			}

			using (var stream = File.OpenWrite(TestFileName))
			{
				for (int i=0; i < NumRecords; i++)
					stream.Write(records[i]);
			}

			fcb.SetFileName(TestFileName);

			machine.DOS.OpenFile(fcb, OperatingSystem.FileStructures.FileMode.Open);

			var fcbAddress = machine.DOS.MemoryManager.AllocateMemory(FileControlBlock.Size, machine.DOS.CurrentPSPSegment);

			fcb.MemoryAddress = fcbAddress;

			fcb.Serialize(machine.MemoryBus);

			try
			{
				var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

				var rin = new RegistersEx();

				rin.AX = (int)Interrupt0x21.Function.RandomRead << 8;
				rin.DS = (ushort)(fcbAddress / MemoryManager.ParagraphSize);
				rin.DX = (ushort)(fcbAddress % MemoryManager.ParagraphSize);

				var expectedResults = new byte[fcb.RecordSize];

				var dta = machine.SystemMemory.AsSpan().Slice(machine.DOS.DiskTransferAddress, fcb.RecordSize);

				dta.Fill(1);

				int[] returnCodes = new int[NumRecords];
				byte[][] actualData = new byte[NumRecords][];

				// Act
				for (int i=0; i < NumRecords; i++)
				{
					int recordNumber = (i * 3 + 1) % NumRecords;

					fcb = FileControlBlock.Deserialize(machine.MemoryBus, fcb.MemoryAddress);
					fcb.RandomRecordNumber = (uint)recordNumber;
					fcb.Serialize(machine.MemoryBus);

					var rout = sut.Execute(rin);

					returnCodes[recordNumber] = rout.AX & 0xFF;
					actualData[recordNumber] = dta.ToArray();
				}

				// Assert
				returnCodes.Should().AllBeEquivalentTo(0);

				for (int i=0; i < NumRecords; i++)
					actualData[i].ShouldMatch(records[i]);
			}
			finally
			{
				machine.DOS.CloseFile(fcb);
			}
		}
	}

	[Test]
	public void RandomWrite_should_write_multiple_records_in_arbitrary_order()
	{
		// Arrange
		using (var workspace = new TemporaryDirectory())
		{
			Environment.CurrentDirectory = workspace.Path;

			var machine = new Machine();

			machine.DOS.SetUpRunningProgramSegmentPrefix("");

			const string TestFileName = "TESTFILE.TXT";

			var fcb = new FileControlBlock();

			fcb.RecordSize.Should().BeLessThanOrEqualTo(128); // default DTA size

			const int NumRecords = 5;

			byte[][] records = new byte[NumRecords][];

			for (int i=0; i < NumRecords; i++)
			{
				records[i] = new byte[fcb.RecordSize];
				TestContext.CurrentContext.Random.NextBytes(records[i]);
			}

			fcb.SetFileName(TestFileName);

			machine.DOS.OpenFile(fcb, OperatingSystem.FileStructures.FileMode.CreateNew);

			var fcbAddress = machine.DOS.MemoryManager.AllocateMemory(FileControlBlock.Size, machine.DOS.CurrentPSPSegment);

			fcb.MemoryAddress = fcbAddress;

			fcb.Serialize(machine.MemoryBus);

			int[] returnCodes = new int[NumRecords];
			byte[][] actualData = new byte[NumRecords][];

			try
			{
				var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

				var rin = new RegistersEx();

				rin.AX = (int)Interrupt0x21.Function.RandomWrite << 8;
				rin.DS = (ushort)(fcbAddress / MemoryManager.ParagraphSize);
				rin.DX = (ushort)(fcbAddress % MemoryManager.ParagraphSize);

				var dta = machine.SystemMemory.AsSpan().Slice(machine.DOS.DiskTransferAddress, fcb.RecordSize);

				// Act
				for (int i=0; i < NumRecords; i++)
				{
					int recordNumber = (i * 3 + 1) % NumRecords;

					fcb = FileControlBlock.Deserialize(machine.MemoryBus, fcb.MemoryAddress);
					fcb.RandomRecordNumber = (uint)recordNumber;
					fcb.Serialize(machine.MemoryBus);

					records[recordNumber].CopyTo(dta);

					var rout = sut.Execute(rin);

					returnCodes[recordNumber] = rout.AX & 0xFF;
				}
			}
			finally
			{
				machine.DOS.CloseFile(fcb);
			}

			// Assert
			returnCodes.Should().AllBeEquivalentTo(0);

			using (var file = File.OpenRead(TestFileName))
			{
				byte[] buffer = new byte[fcb.RecordSize];

				for (int i=0; i < NumRecords; i++)
				{
					file.ReadExactly(buffer);

					buffer.ShouldMatch(records[i]);
				}
			}
		}
	}

	[Test]
	public void GetFileSize_should_return_file_sizes()
	{
		// Arrange
		using (var workspace = new TemporaryDirectory())
		{
			Environment.CurrentDirectory = workspace.Path;

			var machine = new Machine();

			machine.DOS.SetUpRunningProgramSegmentPrefix("");

			const string TestFileName = "TESTFILE.BIN";

			uint fileSize = (uint)TestContext.CurrentContext.Random.Next(10000, 2000000);

			byte[] testData = new byte[fileSize];

			TestContext.CurrentContext.Random.NextBytes(testData);

			File.WriteAllBytes(TestFileName, testData);

			var fcb = new FileControlBlock();

			fcb.SetFileName(TestFileName);

			var fcbAddress = machine.DOS.MemoryManager.AllocateMemory(FileControlBlock.Size, machine.DOS.CurrentPSPSegment);

			fcb.MemoryAddress = fcbAddress;

			fcb.Serialize(machine.MemoryBus);

			try
			{
				var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

				var rin = new RegistersEx();

				rin.AX = (int)Interrupt0x21.Function.GetFileSize << 8;
				rin.DS = (ushort)(fcbAddress / MemoryManager.ParagraphSize);
				rin.DX = (ushort)(fcbAddress % MemoryManager.ParagraphSize);

				// Act
				var rout = sut.Execute(rin);

				// Assert
				int al = rout.AX & 0xFF;

				al.Should().Be(0);

				fcb = FileControlBlock.Deserialize(machine.MemoryBus, fcbAddress);

				fcb.FileSize.Should().Be(fileSize);
			}
			finally
			{
				machine.DOS.CloseFile(fcb);
			}
		}
	}

	[Test]
	public void SetRandomRecordNumber_should_assign_correct_value_to_correct_field()
	{
		// Arrange
		using (var workspace = new TemporaryDirectory())
		{
			Environment.CurrentDirectory = workspace.Path;

			var machine = new Machine();

			machine.DOS.SetUpRunningProgramSegmentPrefix("");

			const string TestFileName = "TESTFILE.TXT";

			var fcb = new FileControlBlock();

			fcb.SetFileName(TestFileName);

			machine.DOS.OpenFile(fcb, OperatingSystem.FileStructures.FileMode.CreateNew);

			var fcbAddress = machine.DOS.MemoryManager.AllocateMemory(FileControlBlock.Size, machine.DOS.CurrentPSPSegment);

			fcb.MemoryAddress = fcbAddress;

			try
			{
				var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

				var rin = new RegistersEx();

				rin.AX = (int)Interrupt0x21.Function.SetRandomRecordNumber << 8;
				rin.DS = (ushort)(fcbAddress / MemoryManager.ParagraphSize);
				rin.DX = (ushort)(fcbAddress % MemoryManager.ParagraphSize);

				fcb.CurrentBlockNumber = 3;
				fcb.CurrentRecordNumber = 27;

				fcb.Serialize(machine.MemoryBus);

				uint linearRecordNumber = (uint)(fcb.CurrentBlockNumber * 128 + fcb.CurrentRecordNumber);

				// Act
				var rout = sut.Execute(rin);

				// Assert
				int al = rout.AX & 0xFF;

				al.Should().Be(0);

				var updatedFCB = FileControlBlock.Deserialize(machine.MemoryBus, fcbAddress);

				updatedFCB.RandomRecordNumber.Should().Be(linearRecordNumber);

				fcb.RandomRecordNumber = updatedFCB.RandomRecordNumber;

				updatedFCB.Should().BeEquivalentTo(fcb);
			}
			finally
			{
				machine.DOS.CloseFile(fcb);
			}
		}
	}

	[Test]
	public void SetRandomRecordNumber_should_fail_if_random_necord_number_field_is_not_zero()
	{
		// Arrange
		using (var workspace = new TemporaryDirectory())
		{
			Environment.CurrentDirectory = workspace.Path;

			var machine = new Machine();

			machine.DOS.SetUpRunningProgramSegmentPrefix("");

			const string TestFileName = "TESTFILE.TXT";

			var fcb = new FileControlBlock();

			fcb.SetFileName(TestFileName);

			machine.DOS.OpenFile(fcb, OperatingSystem.FileStructures.FileMode.CreateNew);

			fcb.RandomRecordNumber = 37;

			var fcbAddress = machine.DOS.MemoryManager.AllocateMemory(FileControlBlock.Size, machine.DOS.CurrentPSPSegment);

			fcb.MemoryAddress = fcbAddress;

			fcb.Serialize(machine.MemoryBus);

			try
			{
				var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

				var rin = new RegistersEx();

				rin.AX = (int)Interrupt0x21.Function.SetRandomRecordNumber << 8;
				rin.DS = (ushort)(fcbAddress / MemoryManager.ParagraphSize);
				rin.DX = (ushort)(fcbAddress % MemoryManager.ParagraphSize);

				// Act
				var rout = sut.Execute(rin);

				// Assert
				int al = rout.AX & 0xFF;

				al.Should().Be(0xFF);

				var updatedFCB = FileControlBlock.Deserialize(machine.MemoryBus, fcbAddress);

				updatedFCB.Should().BeEquivalentTo(fcb);
			}
			finally
			{
				machine.DOS.CloseFile(fcb);
			}
		}
	}

	[Test]
	public void RandomBlockRead_should_read_block_of_records_in_one_call()
	{
		// Arrange
		using (var workspace = new TemporaryDirectory())
		{
			Environment.CurrentDirectory = workspace.Path;

			var machine = new Machine();

			machine.DOS.SetUpRunningProgramSegmentPrefix("");

			const string TestFileName = "TESTFILE.TXT";

			var fcb = new FileControlBlock();

			fcb.RecordSize.Should().BeLessThanOrEqualTo(128); // default DTA size

			const int NumRecords = 5;

			byte[][] records = new byte[NumRecords][];

			for (int i=0; i < NumRecords; i++)
			{
				records[i] = new byte[fcb.RecordSize];
				TestContext.CurrentContext.Random.NextBytes(records[i]);
			}

			using (var stream = File.OpenWrite(TestFileName))
			{
				for (int i=0; i < NumRecords; i++)
					stream.Write(records[i]);
			}

			fcb.SetFileName(TestFileName);

			machine.DOS.OpenFile(fcb, OperatingSystem.FileStructures.FileMode.Open);

			const int BlockReadStart = 1;
			const int BlockReadSize = 3;

			fcb.RandomRecordNumber = BlockReadStart;

			var fcbAddress = machine.DOS.MemoryManager.AllocateMemory(FileControlBlock.Size, machine.DOS.CurrentPSPSegment);

			fcb.MemoryAddress = fcbAddress;

			fcb.Serialize(machine.MemoryBus);

			var dtaAddress = machine.DOS.MemoryManager.AllocateMemory(fcb.RecordSize * BlockReadSize, machine.DOS.CurrentPSPSegment);

			var dtaAddressSegmented = new SegmentedAddress(dtaAddress);

			machine.DOS.DiskTransferAddressSegment = dtaAddressSegmented.Segment;
			machine.DOS.DiskTransferAddressOffset = dtaAddressSegmented.Offset;

			try
			{
				var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

				var rin = new RegistersEx();

				rin.AX = (int)Interrupt0x21.Function.RandomBlockRead << 8;
				rin.CX = BlockReadSize;
				rin.DS = (ushort)(fcbAddress / MemoryManager.ParagraphSize);
				rin.DX = (ushort)(fcbAddress % MemoryManager.ParagraphSize);

				var expectedResults = new byte[fcb.RecordSize];

				var dta = machine.SystemMemory.AsSpan().Slice(machine.DOS.DiskTransferAddress, fcb.RecordSize * BlockReadSize);

				dta.Fill(1);

				// Act
				var rout = sut.Execute(rin);

				// Assert
				int al = rout.AX & 0xFF;

				al.Should().Be(0);

				for (int i=0; i < BlockReadSize; i++)
					dta.Slice(i * fcb.RecordSize, fcb.RecordSize).ShouldMatch(records[BlockReadStart + i]);
			}
			finally
			{
				machine.DOS.CloseFile(fcb);
			}
		}
	}

	[Test]
	public void RandomBlockWrite_should_write_block_of_records_in_one_call()
	{
		// Arrange
		using (var workspace = new TemporaryDirectory())
		{
			Environment.CurrentDirectory = workspace.Path;

			var machine = new Machine();

			machine.DOS.SetUpRunningProgramSegmentPrefix("");

			const string TestFileName = "TESTFILE.TXT";

			var fcb = new FileControlBlock();

			fcb.SetFileName(TestFileName);

			machine.DOS.OpenFile(fcb, OperatingSystem.FileStructures.FileMode.CreateNew);

			const int NumRecords = 5;

			byte[][] records = new byte[NumRecords][];

			for (int i=0; i < NumRecords; i++)
			{
				records[i] = new byte[fcb.RecordSize];
				TestContext.CurrentContext.Random.NextBytes(records[i]);
			}

			const int BlockWriteStart = 1;
			const int BlockWriteSize = 3;

			fcb.RandomRecordNumber = BlockWriteStart;

			var fcbAddress = machine.DOS.MemoryManager.AllocateMemory(FileControlBlock.Size, machine.DOS.CurrentPSPSegment);

			fcb.MemoryAddress = fcbAddress;

			fcb.Serialize(machine.MemoryBus);

			var dtaAddress = machine.DOS.MemoryManager.AllocateMemory(fcb.RecordSize * BlockWriteSize, machine.DOS.CurrentPSPSegment);

			var dtaAddressSegmented = new SegmentedAddress(dtaAddress);

			machine.DOS.DiskTransferAddressSegment = dtaAddressSegmented.Segment;
			machine.DOS.DiskTransferAddressOffset = dtaAddressSegmented.Offset;

			Registers rout;

			try
			{
				var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

				var rin = new RegistersEx();

				rin.AX = (int)Interrupt0x21.Function.RandomBlockWrite << 8;
				rin.CX = BlockWriteSize;
				rin.DS = (ushort)(fcbAddress / MemoryManager.ParagraphSize);
				rin.DX = (ushort)(fcbAddress % MemoryManager.ParagraphSize);

				var dta = machine.SystemMemory.AsSpan().Slice(machine.DOS.DiskTransferAddress, BlockWriteSize * fcb.RecordSize);

				for (int i=0; i < BlockWriteSize; i++)
					records[BlockWriteStart + i].CopyTo(dta.Slice(i * fcb.RecordSize));

				// Act
				rout = sut.Execute(rin);
			}
			finally
			{
				machine.DOS.CloseFile(fcb);
			}

			// Assert
			int al = rout.AX & 0xFF;

			al.Should().Be(0);

			using (var file = File.OpenRead(TestFileName))
			{
				byte[] buffer = new byte[(BlockWriteStart + BlockWriteSize) * fcb.RecordSize];

				file.Length.Should().Be(buffer.Length);

				file.ReadExactly(buffer);

				byte[] zeroBytes = new byte[BlockWriteStart * fcb.RecordSize];

				buffer.AsSpan().Slice(0, BlockWriteStart * fcb.RecordSize).ShouldMatch(zeroBytes);

				for (int i=0; i < BlockWriteSize; i++)
					buffer.AsSpan().Slice((BlockWriteStart + i) * fcb.RecordSize, fcb.RecordSize).ShouldMatch(records[BlockWriteStart + i]);
			}
		}
	}

	[Test]
	public void ParseFilename_should_parse_complete_filename()
	{
		ParseFilenameTest(
			input: "D:FILENAME.TXT",
			parseControl: default,
			expectFailure: false,
			expectContainsWildcard: false,
			expectDriveIdentifier: 'D',
			expectFileNameBytes: "FILENAMETXT");
	}

	[Test]
	public void ParseFilename_should_normalize_filename_case()
	{
		ParseFilenameTest(
			input: "D:Filénæme.txt",
			parseControl: default,
			expectFailure: false,
			expectContainsWildcard: false,
			expectDriveIdentifier: 'D',
			expectFileNameBytes: "FILENÆMETXT");
	}

	[Test]
	public void ParseFilename_should_pad_short_filename_with_spaces()
	{
		ParseFilenameTest(
			input: "E:FILE.TXT",
			parseControl: default,
			expectFailure: false,
			expectContainsWildcard: false,
			expectDriveIdentifier: 'E',
			expectFileNameBytes: "FILE    TXT");
	}

	[Test]
	public void ParseFilename_should_pad_short_extension_with_spaces()
	{
		ParseFilenameTest(
			input: "F:FILENAME.T",
			parseControl: default,
			expectFailure: false,
			expectContainsWildcard: false,
			expectDriveIdentifier: 'F',
			expectFileNameBytes: "FILENAMET  ");
	}

	[Test]
	public void ParseFilename_should_ignore_leading_whitespace()
	{
		ParseFilenameTest(
			input: "  \t G:TESTFILE.BIN",
			parseControl: default,
			expectFailure: false,
			expectContainsWildcard: false,
			expectDriveIdentifier: 'G',
			expectFileNameBytes: "TESTFILEBIN");
	}

	[Test]
	public void ParseFilename_should_abort_on_leading_separator()
	{
		ParseFilenameTest(
			input: ";H:TESTFILE.BIN",
			parseControl: default,
			expectFailure: false,
			expectContainsWildcard: false,
			expectDriveIdentifier: default,
			expectFileNameBytes: "           ",
			expectConsumedInputBytes: 0);
	}

	[Test]
	public void ParseFilename_should_ignore_one_leading_separator_when_configured()
	{
		ParseFilenameTest(
			input: ";H:TESTFILE.BIN",
			parseControl: ParseFlags.IgnoreOneLeadingSeparator,
			expectFailure: false,
			expectContainsWildcard: false,
			expectDriveIdentifier: 'H',
			expectFileNameBytes: "TESTFILEBIN");
	}

	[Test]
	public void ParseFilename_should_not_ignore_more_than_one_leading_separator()
	{
		ParseFilenameTest(
			input: ";,H:TESTFILE.BIN",
			parseControl: ParseFlags.IgnoreOneLeadingSeparator,
			expectFailure: false,
			expectContainsWildcard: false,
			expectDriveIdentifier: default,
			expectFileNameBytes: "           ",
			expectConsumedInputBytes: 1);
	}

	[Test]
	public void ParseFilename_should_terminate_on_control_character()
	{
		ParseFilenameTest(
			input: "I:TEST\bILE.BIN",
			parseControl: default,
			expectFailure: false,
			expectContainsWildcard: false,
			expectDriveIdentifier: 'I',
			expectFileNameBytes: "TEST       ",
			expectConsumedInputBytes: 6);
	}

	[Test]
	public void ParseFilename_should_terminate_on_illegal_character([Values('/', '"', '[', ']', '<', '>', '|')] char illegalCharacter)
	{
		ParseFilenameTest(
			input: "I:TEST" + illegalCharacter + "ILE.BIN",
			parseControl: default,
			expectFailure: false,
			expectContainsWildcard: false,
			expectDriveIdentifier: 'I',
			expectFileNameBytes: "TEST       ",
			expectConsumedInputBytes: 6);
	}

	[Test]
	public void ParseFilename_should_clear_drive_identifier_when_input_has_no_drive_letter()
	{
		ParseFilenameTest(
			initialDriveIdentifier: 'J',
			input: "TESTFILE.CAT",
			parseControl: default,
			expectFailure: false,
			expectContainsWildcard: false,
			expectDriveIdentifier: default,
			expectFileNameBytes: "TESTFILECAT");
	}

	[Test]
	public void ParseFilename_should_leave_drive_identifier_when_input_has_no_drive_letter_if_configured()
	{
		ParseFilenameTest(
			initialDriveIdentifier: 'J',
			input: "TESTFILE.CAT",
			parseControl: ParseFlags.DoNotSetDefaultDriveIdentifier,
			expectFailure: false,
			expectContainsWildcard: false,
			expectDriveIdentifier: 'J',
			expectFileNameBytes: "TESTFILECAT");
	}

	[Test]
	public void ParseFilename_should_clear_filename_when_input_has_no_filename()
	{
		ParseFilenameTest(
			initialFileName: "FILETEST.DOG",
			input: "K:.CAT",
			parseControl: default,
			expectFailure: false,
			expectContainsWildcard: false,
			expectDriveIdentifier: 'K',
			expectFileNameBytes: "        CAT");
	}

	[Test]
	public void ParseFilename_should_leave_filename_when_input_has_no_filename_if_configured()
	{
		ParseFilenameTest(
			initialFileName: "FILETEST.DOG",
			input: "K:.CAT",
			parseControl: ParseFlags.DoNotClearOnInvalidFileName,
			expectFailure: false,
			expectContainsWildcard: false,
			expectDriveIdentifier: 'K',
			expectFileNameBytes: "FILETESTCAT");
	}

	[Test]
	public void ParseFilename_should_clear_extension_when_input_has_no_extension()
	{
		ParseFilenameTest(
			initialFileName: "FILETEST.DOG",
			input: "L:TESTFILE",
			parseControl: default,
			expectFailure: false,
			expectContainsWildcard: false,
			expectDriveIdentifier: 'L',
			expectFileNameBytes: "TESTFILE   ");
	}

	[Test]
	public void ParseFilename_should_leave_extension_when_input_has_no_extension_if_configured()
	{
		ParseFilenameTest(
			initialFileName: "FILETEST.DOG",
			input: "L:TESTFILE",
			parseControl: ParseFlags.DoNotClearOnInvalidExtension,
			expectFailure: false,
			expectContainsWildcard: false,
			expectDriveIdentifier: 'L',
			expectFileNameBytes: "TESTFILEDOG");
	}

	[Test]
	public void ParseFilename_should_treat_final_dot_as_specified_empty_extension()
	{
		ParseFilenameTest(
			initialFileName: "FILETEST.DOG",
			input: "M:TESTFILE.",
			parseControl: ParseFlags.DoNotClearOnInvalidExtension,
			expectFailure: false,
			expectContainsWildcard: false,
			expectDriveIdentifier: 'M',
			expectFileNameBytes: "TESTFILE   ");
	}

	[Test]
	public void ParseFilename_should_expand_wildcards_in_filename()
	{
		ParseFilenameTest(
			input: "O:FIL*.DAT",
			parseControl: default,
			expectFailure: false,
			expectContainsWildcard: true,
			expectDriveIdentifier: 'O',
			expectFileNameBytes: "FIL?????DAT");
	}

	[Test]
	public void ParseFilename_should_expand_wildcards_in_extension()
	{
		ParseFilenameTest(
			input: "P:FILE.D*",
			parseControl: default,
			expectFailure: false,
			expectContainsWildcard: true,
			expectDriveIdentifier: 'P',
			expectFileNameBytes: "FILE    D??");
	}

	[Test]
	public void GetDate_should_return_date()
	{
		// Arrange
		var machine = new Machine();

		var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

		var rin = new RegistersEx();

		rin.AX = (int)Interrupt0x21.Function.GetDate << 8;

		var expectedDate = DateTime.Now.Date;

		if (DateTime.Now.AddSeconds(2).Date != expectedDate)
			Assert.Inconclusive("Too close to midnight");

		// Act
		var rout = sut.Execute(rin);

		int actualDayOfWeek = rout.AX & 0xFF;
		int actualYear = rout.CX;
		int actualMonth = rout.DX >> 8;
		int actualMonthDay = rout.DX & 0xFF;

		// Assert
		actualDayOfWeek.Should().Be((byte)expectedDate.DayOfWeek);
		actualYear.Should().Be(expectedDate.Year);
		actualMonth.Should().Be(expectedDate.Month);
		actualMonthDay.Should().Be(expectedDate.Day);
	}

	[Test]
	public void SetDate_should_modify_date([Random(1980, 2099, 5)] int year, [Random(1, 12, 5)] int month, [Random(1, 31, 5)] int monthDay)
	{
		// Arrange
		monthDay = Math.Min(monthDay, DateTime.DaysInMonth(year, month));

		var machine = new Machine();

		var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

		var rin = new RegistersEx();

		rin.AX = (int)Interrupt0x21.Function.SetDate << 8;
		rin.CX = (ushort)year;
		rin.DX = unchecked((ushort)(
			(month << 8) |
			monthDay));

		if (DateTime.Now.AddSeconds(2).Date != DateTime.Now.Date)
			Assert.Inconclusive("Too close to midnight");

		var expectedDate = new DateTime(year, month, monthDay);

		// Act
		var rout = sut.Execute(rin);

		// Assert
		machine.SystemClock.Now.Date.Should().Be(expectedDate);
	}

	[Test]
	public void GetTime_should_return_time()
	{
		// Arrange
		var machine = new Machine();

		var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

		var rin = new RegistersEx();

		rin.AX = (int)Interrupt0x21.Function.GetTime << 8;

		var expectedTime = DateTime.Now;

		// Act
		var rout = sut.Execute(rin);

		int actualHour = rout.CX >> 8;
		int actualMinutes = rout.CX & 0xFF;
		int actualSeconds = rout.DX >> 8;
		int actualHundredths = rout.DX & 0xFF;

		// Assert
		var actualTime = DateTime.Today + new TimeSpan(
			days: 0, actualHour, actualMinutes, actualSeconds, milliseconds: actualHundredths * 10);

		actualTime.Should().BeCloseTo(DateTime.Now, TimeSpan.FromMilliseconds(50));
	}

	[Test]
	public void SetTime_should_modify_time([Random(0, 23, 5)] int hour, [Random(0, 59, 5)] int minute, [Random(0, 5999, 5)] int hundredths)
	{
		// Arrange
		int second = hundredths / 100;

		hundredths %= 100;

		var machine = new Machine();

		var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

		var rin = new RegistersEx();

		rin.AX = (int)Interrupt0x21.Function.SetTime << 8;
		rin.CX = unchecked((ushort)(
			(hour << 8) |
			minute));
		rin.DX = unchecked((ushort)(
			(second << 8) |
			hundredths));

		if (DateTime.Now.AddSeconds(2).Date != DateTime.Now.Date)
			Assert.Inconclusive("Too close to midnight");

		var expectedTime = DateTime.Today + new TimeSpan(
			days: 0, hour, minute, second, milliseconds: hundredths * 10);

		// Act
		var rout = sut.Execute(rin);

		// Assert
		machine.SystemClock.Now.Should().BeCloseTo(expectedTime, TimeSpan.FromMilliseconds(50));
	}

	[Test]
	public void SetResetVerifyFlag_should_update_flag_correctly([Values] bool initialFlagValue, [Values] bool newFlagValue)
	{
		// Arrange
		var machine = new Machine();

		machine.DOS.VerifyWrites = initialFlagValue;

		var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

		var rin = new RegistersEx();

		rin.AX = (int)Interrupt0x21.Function.SetResetVerifyFlag << 8;
		if (newFlagValue)
			rin.AX |= 1;

		// Act
		bool valueBefore = machine.DOS.VerifyWrites;

		var rout = sut.Execute(rin);

		bool valueAfter = machine.DOS.VerifyWrites;

		// Assert
		valueBefore.Should().Be(initialFlagValue);
		valueAfter.Should().Be(newFlagValue);
	}

	/*
	public enum Function : byte
	{
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

	enum DOSReadInputType
	{
		Character,
		CtrlC,
		CtrlBreak,
	}

	void DOSReadTest(Interrupt0x21.Function function, bool prequeueEvent, DOSReadInputType inputType = DOSReadInputType.Character, bool shouldBreak = false, bool shouldEcho = false)
	{
		// Arrange
		var machine = new Machine();

		var captureBuffer = new StringValue();

		var capture = new CapturingTextLibrary(machine, captureBuffer);

		machine.VideoFirmware.SetTestingVisualLibrary(capture);

		bool breakEventOccurred = false;

		machine.DOS.Break += () => breakEventOccurred = true;

		bool eventQueued = false;

		SDL.Keymod modState = 0;

		machine.Keyboard.GetModStateTestHook += () => modState;

		void QueueEvent()
		{
			switch (inputType)
			{
				case DOSReadInputType.CtrlC: SimulateTyping("\x03", ControlCharacterHandling.CtrlLetter, machine); break;
				case DOSReadInputType.CtrlBreak: SimulateTyping(CtrlBreakCharacter.ToString(), default, machine); break;
			}

			// Also send the character, in case break is disabled or doesn't work.
			SimulateTyping("a", default, machine);

			eventQueued = true;
		}

		byte expectedCharacterRead;

		if (shouldBreak)
			expectedCharacterRead = 0;
		else if (inputType == DOSReadInputType.CtrlC)
			expectedCharacterRead = 3;
		else
			expectedCharacterRead = (byte)'a';

		string expectedOutput = shouldEcho ? CP437Encoding.GetCharSemantic(expectedCharacterRead).ToString() : "";

		if (prequeueEvent)
			QueueEvent();

		var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

		var rin = new Registers();

		rin.AX = (ushort)((int)function << 8);

		if (!prequeueEvent)
		{
			var queueThread = new Thread(
				() =>
				{
					Thread.Sleep(100);
					QueueEvent();
				});

			queueThread.IsBackground = true;
			queueThread.Start();
		}

		Registers? rout = null;

		// Act
		Action action = () => rout = sut.Execute(rin);

		action.ExecutionTime().Should().BeLessThan(TimeSpan.FromSeconds(0.25));

		bool eventQueuedOnReturn = eventQueued;

		// Assert
		rout.Should().NotBeNull();

		byte characterRead = (byte)(rout.AX & 0xFF);

		eventQueuedOnReturn.Should().BeTrue();
		captureBuffer.ToString().Should().Be(expectedOutput);
		breakEventOccurred.Should().Be(shouldBreak);
		characterRead.Should().Be(expectedCharacterRead);
		machine.DOS.LastError.Should().Be(OperatingSystem.DOSError.None);
	}

	void ParseFilenameTest(string input, ParseFlags parseControl, bool expectFailure, bool expectContainsWildcard, char expectDriveIdentifier, string expectFileNameBytes, int expectConsumedInputBytes = -1, char initialDriveIdentifier = '\0', string? initialFileName = null)
	{
		// Arrange
		var machine = new Machine();

		machine.DOS.SetUpRunningProgramSegmentPrefix("");

		var fcb = new FileControlBlock();

		var fcbAddress = machine.DOS.MemoryManager.AllocateMemory(FileControlBlock.Size, machine.DOS.CurrentPSPSegment);

		fcb.MemoryAddress = fcbAddress;

		if (initialFileName != null)
			fcb.SetFileName(initialFileName);
		if (initialDriveIdentifier != '\0')
			fcb.DriveIdentifier = (byte)(char.ToUpperInvariant(initialDriveIdentifier) - 'A' + 1);

		fcb.Serialize(machine.MemoryBus);

		var inputBytes = s_cp437.GetBytes(input);

		if (expectConsumedInputBytes < 0)
			expectConsumedInputBytes = inputBytes.Length;

		var inputAddress = machine.DOS.MemoryManager.AllocateMemory(inputBytes.Length + 1, machine.DOS.CurrentPSPSegment);

		for (int i = 0; i < inputBytes.Length; i++)
			machine.MemoryBus[inputAddress + i] = inputBytes[i];
		machine.MemoryBus[inputAddress + inputBytes.Length] = 0;

		byte expectDriveIdentifierByte =
			expectDriveIdentifier == default
			? (byte)0
			: (byte)(expectDriveIdentifier - 'A' + 1);

		var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

		var rin = new RegistersEx();

		rin.AX = (int)Interrupt0x21.Function.ParseFilename << 8;
		rin.AX |= (byte)parseControl;
		rin.DS = (ushort)(inputAddress / MemoryManager.ParagraphSize);
		rin.SI = (ushort)(inputAddress % MemoryManager.ParagraphSize);
		rin.ES = (ushort)(fcbAddress / MemoryManager.ParagraphSize);
		rin.DI = (ushort)(fcbAddress % MemoryManager.ParagraphSize);

		// Act
		var rout = sut.Execute(rin);

		// Assert
		int al = rout.AX & 0xFF;

		if (expectFailure)
			al.Should().Be(0xFF);
		else if (expectContainsWildcard)
			al.Should().Be(1);
		else
			al.Should().Be(0);

		fcb = FileControlBlock.Deserialize(machine.MemoryBus, fcbAddress);

		fcb.DriveIdentifier.Should().Be(expectDriveIdentifierByte);

		fcb.FileNameBytes.Should().BeEquivalentTo(
			s_cp437.GetBytes(expectFileNameBytes),
			config => config.WithStrictOrdering());

		var newInputAddress = new SegmentedAddress(rout.AsRegistersEx().DS, rout.SI);

		int consumedBytes = newInputAddress.ToLinearAddress() - inputAddress;

		consumedBytes.Should().Be(expectConsumedInputBytes);
	}

	enum ControlCharacterHandling
	{
		CtrlLetter,
		SemanticKey,
	}

	const char CtrlBreakCharacter = '\uE001';

	void SimulateTyping(string text, ControlCharacterHandling controlCharacterHandling, Machine machine)
	{
		SDL.Keymod modState = 0;

		Func<SDL.Keymod> getModState = () => modState;

		machine.Keyboard.GetModStateTestHook += getModState;

		void ChangeModifiers(bool ctrl, bool shift)
		{
			bool alreadyCtrl = (modState & SDL.Keymod.Ctrl) != 0;
			bool alreadyShift = (modState & SDL.Keymod.Shift) != 0;

			if (!shift && alreadyShift)
			{
				modState &= ~SDL.Keymod.Shift;
				machine.Keyboard.HandleEvent(
					new SDL.KeyboardEvent()
					{
						Down = false,
						Scancode = SDL.Scancode.LShift,
					});
			}

			if (ctrl != alreadyCtrl)
			{
				if (ctrl)
					modState |= SDL.Keymod.LCtrl;
				else
					modState &= ~SDL.Keymod.Ctrl;

				machine.Keyboard.HandleEvent(
					new SDL.KeyboardEvent()
					{
						Down = ctrl,
						Scancode = SDL.Scancode.LCtrl,
					});
			}

			if (shift && !alreadyShift)
			{
				modState |= SDL.Keymod.LShift;
				machine.Keyboard.HandleEvent(
					new SDL.KeyboardEvent()
					{
						Down = true,
						Scancode = SDL.Scancode.LShift,
					});
			}
		}

		try
		{
			foreach (char ch in text)
			{
				SDL.Scancode scanCode = 0;

				if (ch == CtrlBreakCharacter)
				{
					ChangeModifiers(ctrl: true, shift: false);

					scanCode = SDL.Scancode.Pause;
				}
				else if ((ch >= 1) && (ch <= 26))
				{
					if (controlCharacterHandling == ControlCharacterHandling.CtrlLetter)
					{
						ChangeModifiers(ctrl: true, shift: false);

						scanCode = GetScanCodeForCharacter((char)(ch + 'a' - 1));
					}
					else
					{
						scanCode =
							ch switch
							{
								'\b' => SDL.Scancode.Backspace,
								'\t' => SDL.Scancode.Tab,
								'\n' => SDL.Scancode.KpEnter,
								'\r' => SDL.Scancode.Return,

								_ => 0,
							};
					}
				}
				else
				{
					ChangeModifiers(ctrl: false, shift: IsShifted(ch));

					scanCode = GetScanCodeForCharacter(ch);
				}

				if (scanCode != 0)
				{
					machine.Keyboard.HandleEvent(
						new SDL.KeyboardEvent()
						{
							Down = true,
							Scancode = scanCode,
						});

					machine.Keyboard.HandleEvent(
						new SDL.KeyboardEvent()
						{
							Down = false,
							Scancode = scanCode,
						});
				}
			}

			ChangeModifiers(ctrl: false, shift: false);
		}
		finally
		{
			machine.Keyboard.GetModStateTestHook -= getModState;
		}
	}

	SDL.Scancode GetScanCodeForCharacter(char ch)
	{
		switch (ch)
		{
			case 'A': case 'a': return SDL.Scancode.A;
			case 'B': case 'b': return SDL.Scancode.B;
			case 'C': case 'c': return SDL.Scancode.C;
			case 'D': case 'd': return SDL.Scancode.D;
			case 'E': case 'e': return SDL.Scancode.E;
			case 'F': case 'f': return SDL.Scancode.F;
			case 'G': case 'g': return SDL.Scancode.G;
			case 'H': case 'h': return SDL.Scancode.H;
			case 'I': case 'i': return SDL.Scancode.I;
			case 'J': case 'j': return SDL.Scancode.J;
			case 'K': case 'k': return SDL.Scancode.K;
			case 'L': case 'l': return SDL.Scancode.L;
			case 'M': case 'm': return SDL.Scancode.M;
			case 'N': case 'n': return SDL.Scancode.N;
			case 'O': case 'o': return SDL.Scancode.O;
			case 'P': case 'p': return SDL.Scancode.P;
			case 'Q': case 'q': return SDL.Scancode.Q;
			case 'R': case 'r': return SDL.Scancode.R;
			case 'S': case 's': return SDL.Scancode.S;
			case 'T': case 't': return SDL.Scancode.T;
			case 'U': case 'u': return SDL.Scancode.U;
			case 'V': case 'v': return SDL.Scancode.V;
			case 'W': case 'w': return SDL.Scancode.W;
			case 'X': case 'x': return SDL.Scancode.X;
			case 'Y': case 'y': return SDL.Scancode.Y;
			case 'Z': case 'z': return SDL.Scancode.Z;

			case '1': case '!': return SDL.Scancode.Alpha1;
			case '2': case '@': return SDL.Scancode.Alpha2;
			case '3': case '#': return SDL.Scancode.Alpha3;
			case '4': case '$': return SDL.Scancode.Alpha4;
			case '5': case '%': return SDL.Scancode.Alpha5;
			case '6': case '^': return SDL.Scancode.Alpha6;
			case '7': case '&': return SDL.Scancode.Alpha7;
			case '8': case '*': return SDL.Scancode.Alpha8;
			case '9': case '(': return SDL.Scancode.Alpha9;
			case '0': case ')': return SDL.Scancode.Alpha0;

			case '`': case '~': return SDL.Scancode.Grave;
			case '-': case '_': return SDL.Scancode.Minus;
			case '=': case '+': return SDL.Scancode.Equals;
			case '[': case '{': return SDL.Scancode.Leftbracket;
			case ']': case '}': return SDL.Scancode.Rightbracket;
			case '\\': case '|': return SDL.Scancode.Backslash;
			case ';': case ':': return SDL.Scancode.Semicolon;
			case '\'': case '"': return SDL.Scancode.Apostrophe;
			case ',': case '<': return SDL.Scancode.Comma;
			case '.': case '>': return SDL.Scancode.Period;
			case '/': case '?': return SDL.Scancode.Slash;

			case ' ': return SDL.Scancode.Space;
		}

		return 0;
	}

	bool IsShifted(char ch)
	{
		switch (ch)
		{
			case 'A': case 'B': case 'C': case 'D': case 'E': case 'F':
			case 'G': case 'H': case 'I': case 'J': case 'K': case 'L':
			case 'M': case 'N': case 'O': case 'P': case 'Q': case 'R':
			case 'S': case 'T': case 'U': case 'V': case 'W': case 'X':
			case 'Y': case 'Z':
			case '!': case '@': case '#': case '$': case '%': case '^':
			case '&': case '*': case '(': case ')':
			case '~': case '_': case '+': case '{': case '}': case '|':
			case ':': case '"': case '<': case '>': case '?':
				return true;
		}

		return false;
	}
}
