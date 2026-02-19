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

			var dirEntrySpan = machine.SystemMemory.AsSpan().Slice(machine.DOS.DataTransferAddress, 32);

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

			var dirEntrySpan = machine.SystemMemory.AsSpan().Slice(machine.DOS.DataTransferAddress, 32);

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

				var expectedResults = new byte[128];

				testData.CopyTo(expectedResults);

				var dta = machine.SystemMemory.AsSpan().Slice(machine.DOS.DataTransferAddress, 128);

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

			Random rnd = new Random();

			const string TestFileName = "TESTFILE.TXT";

			const int NumRecords = 5;

			byte[][] records = new byte[NumRecords][];

			for (int i=0; i < NumRecords; i++)
			{
				records[i] = new byte[128];
				rnd.NextBytes(records[i]);
			}

			using (var stream = File.OpenWrite(TestFileName))
			{
				for (int i=0; i < NumRecords; i++)
					stream.Write(records[i]);
			}

			var fcb = new FileControlBlock();

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

				var expectedResults = new byte[128];

				var dta = machine.SystemMemory.AsSpan().Slice(machine.DOS.DataTransferAddress, 128);

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

			Random rnd = new Random();

			const string TestFileName = "TESTFILE.TXT";

			const int NumRecords = 5;

			byte[][] records = new byte[NumRecords][];

			for (int i=0; i < NumRecords; i++)
			{
				records[i] = new byte[128];
				rnd.NextBytes(records[i]);
			}

			var fcb = new FileControlBlock();

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

				var dta = machine.SystemMemory.AsSpan().Slice(machine.DOS.DataTransferAddress, 128);

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
				byte[] buffer = new byte[128];

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
	public void RenameFileWithFCB_should_rename_files()
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

			al.Should().Be(0);

			oldExistsBefore.Should().BeTrue();
			oldExistsAfter.Should().BeFalse();

			newExistsBefore.Should().BeFalse();
			newExistsAfter.Should().BeTrue();
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
		 && (pathRoot[1] == Path.PathSeparator))
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

			Random rnd = new Random();

			const string TestFileName = "TESTFILE.TXT";

			const int NumRecords = 5;

			byte[][] records = new byte[NumRecords][];

			for (int i=0; i < NumRecords; i++)
			{
				records[i] = new byte[128];
				rnd.NextBytes(records[i]);
			}

			using (var stream = File.OpenWrite(TestFileName))
			{
				for (int i=0; i < NumRecords; i++)
					stream.Write(records[i]);
			}

			var fcb = new FileControlBlock();

			fcb.SetFileName(TestFileName);

			machine.DOS.OpenFile(fcb, OperatingSystem.FileStructures.FileMode.Open);

			try
			{
				int[] dtaAddresses = new int[NumRecords];

				for (int i=0; i < NumRecords; i++)
					dtaAddresses[i] = machine.DOS.MemoryManager.AllocateMemory(fcb.RecordSize, machine.DOS.CurrentPSPSegment);

				var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

				var expectedResults = new byte[128];

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

			Random rnd = new Random();

			const string TestFileName = "TESTFILE.TXT";

			const int NumRecords = 5;

			byte[][] records = new byte[NumRecords][];

			for (int i=0; i < NumRecords; i++)
			{
				records[i] = new byte[128];
				rnd.NextBytes(records[i]);
			}

			using (var stream = File.OpenWrite(TestFileName))
			{
				for (int i=0; i < NumRecords; i++)
					stream.Write(records[i]);
			}

			var fcb = new FileControlBlock();

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

				var expectedResults = new byte[128];

				var dta = machine.SystemMemory.AsSpan().Slice(machine.DOS.DataTransferAddress, 128);

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

			Random rnd = new Random();

			const string TestFileName = "TESTFILE.TXT";

			const int NumRecords = 5;

			byte[][] records = new byte[NumRecords][];

			for (int i=0; i < NumRecords; i++)
			{
				records[i] = new byte[128];
				rnd.NextBytes(records[i]);
			}

			var fcb = new FileControlBlock();

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

				var dta = machine.SystemMemory.AsSpan().Slice(machine.DOS.DataTransferAddress, 128);

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
				byte[] buffer = new byte[128];

				for (int i=0; i < NumRecords; i++)
				{
					file.ReadExactly(buffer);

					buffer.ShouldMatch(records[i]);
				}
			}
		}
	}
	/*
	public enum Function : byte
	{
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
