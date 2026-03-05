using System.Globalization;
using System.IO.Enumeration;
using System.Numerics;
using System.Runtime.InteropServices;

using NSubstitute;

using QBX.ExecutionEngine.Execution;
using QBX.Firmware.Fonts;
using QBX.Hardware;
using QBX.Interrupts;
using QBX.OperatingSystem;
using QBX.OperatingSystem.FileDescriptors;
using QBX.OperatingSystem.FileStructures;
using QBX.OperatingSystem.Globalization;
using QBX.OperatingSystem.Memory;
using QBX.OperatingSystem.Processes;
using QBX.Tests.Utility;
using QBX.Tests.Utility.Interop;

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
		machine.DOS.LastError.Should().Be(DOSError.None);
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
		machine.DOS.LastError.Should().Be(DOSError.None);
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
		machine.DOS.LastError.Should().Be(DOSError.None);
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
		machine.DOS.LastError.Should().Be(DOSError.None);
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

		action.ExecutionTime().Should().BeLessThan(TimeSpan.FromSeconds(0.5));

		rout.Should().NotBeNull();
		rout.FLAGS.Should().HaveFlag(Flags.Zero);

		captureBuffer.ToString().Should().Be("");
		machine.DOS.LastError.Should().Be(DOSError.None);
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
		machine.DOS.LastError.Should().Be(DOSError.None);
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
		machine.DOS.LastError.Should().Be(DOSError.None);
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

		machine.DOS.LastError.Should().Be(DOSError.None);
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

		Registers? rout = null;

		// Act & Assert
		Action action = () => rout = sut.Execute(rin);

		action.ExecutionTime().Should().BeLessThan(TimeSpan.FromSeconds(0.1));

		// Assert
		rout.Should().NotBeNull();

		captureBuffer.ToString().Should().Be(truncatedMessageWithAlertsAndCarriageReturn);
		bufferHeader[0].Should().Be(bufferLength);
		bufferHeader[1].Should().Be((byte)truncatedMessage.Length);

		string bufferContent = s_cp437.GetString(bufferData.Slice(0, bufferHeader[1]));

		bufferContent.Should().Be(truncatedMessage);

		machine.DOS.LastError.Should().Be(DOSError.None);
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
		machine.DOS.LastError.Should().Be(DOSError.None);
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
		machine.DOS.LastError.Should().Be(DOSError.None);
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
		machine.DOS.LastError.Should().Be(DOSError.None);
	}

	[Test, NonParallelizable]
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
				QBX.OperatingSystem.FileStructures.FileMode.Create,
				OpenMode.Access_ReadWrite | OpenMode.Share_DenyNone);

			try
			{
				string testFileActualPath = ((RegularFileDescriptor)machine.DOS.Files[fileHandle]!).PhysicalPath;

				byte[] testData = s_cp437.GetBytes("test");

				int numWritten = machine.DOS.Write(
					fileHandle,
					testData,
					out _);

				numWritten.Should().Be(testData.Length);

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
				machine.DOS.LastError.Should().Be(DOSError.None);
			}
			finally
			{
				machine.DOS.CloseAllFiles(keepStandardHandles: false);
			}
		}
	}

	[Test, NonParallelizable]
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

	[Test, NonParallelizable]
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

			machine.DOS.OpenFile(fcb, QBX.OperatingSystem.FileStructures.FileMode.Open);

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

	[Test, NonParallelizable]
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

			rin.AX = (int)Interrupt0x21.Function.FindFirstFileWithFCB << 8;
			rin.DS = (ushort)(fcbAddress / MemoryManager.ParagraphSize);
			rin.DX = (ushort)(fcbAddress % MemoryManager.ParagraphSize);

			// Act
			var rout = sut.Execute(rin);

			// Assert
			int al = rout.AX & 0xFF;

			al.Should().Be(0xFF);
		}
	}

	[Test, NonParallelizable]
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

			rin.AX = (int)Interrupt0x21.Function.FindFirstFileWithFCB << 8;
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

	[Test, NonParallelizable]
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

			while ((rout.AX & 0xFF) == 0)
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

	[Test, NonParallelizable]
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

	[Test, NonParallelizable]
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

			machine.DOS.OpenFile(fcb, QBX.OperatingSystem.FileStructures.FileMode.Open);

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

	[Test, NonParallelizable]
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

			machine.DOS.OpenFile(fcb, QBX.OperatingSystem.FileStructures.FileMode.Open);

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

	[Test, NonParallelizable]
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

			machine.DOS.OpenFile(fcb, QBX.OperatingSystem.FileStructures.FileMode.CreateNew);

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

	[Test, NonParallelizable]
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

	[Test, NonParallelizable]
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

	[Test, NonParallelizable]
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

	[Test, NonParallelizable]
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

	[Test, NonParallelizable]
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

	[Test, NonParallelizable]
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

	[Test, NonParallelizable]
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

	[Test, NonParallelizable]
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

		int driveIdentifier = PathCharacter.GetDriveLetter(Environment.CurrentDirectory) - 'A';

		al.Should().Be(driveIdentifier);
	}

	[Test, NonParallelizable]
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

			machine.DOS.OpenFile(fcb, QBX.OperatingSystem.FileStructures.FileMode.Open);

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

	[Test, NonParallelizable]
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

			machine.DOS.OpenFile(fcb, QBX.OperatingSystem.FileStructures.FileMode.Open);

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

	[Test, NonParallelizable]
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

			machine.DOS.OpenFile(fcb, QBX.OperatingSystem.FileStructures.FileMode.CreateNew);

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

	[Test, NonParallelizable]
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

	[Test, NonParallelizable]
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

			machine.DOS.OpenFile(fcb, QBX.OperatingSystem.FileStructures.FileMode.CreateNew);

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

	[Test, NonParallelizable]
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

			machine.DOS.OpenFile(fcb, QBX.OperatingSystem.FileStructures.FileMode.CreateNew);

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

	[Test, NonParallelizable]
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

			machine.DOS.OpenFile(fcb, QBX.OperatingSystem.FileStructures.FileMode.Open);

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

	[Test, NonParallelizable]
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

			machine.DOS.OpenFile(fcb, QBX.OperatingSystem.FileStructures.FileMode.CreateNew);

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

	[Test]
	public void GetDiskTransferAddress_should_return_DTA_address()
	{
		// Arrange
		var machine = new Machine();

		var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

		var rin = new RegistersEx();

		rin.AX = (int)Interrupt0x21.Function.GetDiskTransferAddress << 8;

		var expectedValue = new SegmentedAddress(
			machine.DOS.DiskTransferAddressSegment,
			machine.DOS.DiskTransferAddressOffset);

		// Act
		var rout = sut.Execute(rin);

		// Assert
		var actualValue = new SegmentedAddress(
			rout.AsRegistersEx().ES,
			rout.BX);

		actualValue.Should().BeEquivalentTo(expectedValue);
	}

	[Test]
	public void GetVersionNumber_should_return_some_values()
	{
		// Arrange
		var machine = new Machine();

		var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

		var rin = new RegistersEx();

		rin.AX = (int)Interrupt0x21.Function.GetVersionNumber << 8;

		// Act
		var rout = sut.Execute(rin);

		int versionMajor = rout.AX & 0xFF;
		int versionMinor = rout.AX >> 8;
		int versionFlag = rout.BX >> 8;
		int serialNumber = ((rout.BX & 0xFF) << 16) | rout.CX;

		// Assert
		versionMajor.Should().BeInRange(5, 6);
		versionMinor.Should().Be(0);
		versionFlag.Should().Be(0);
		serialNumber.Should().Be(0);
	}

	static IEnumerable<object[]> EnumerateDriveNumbers()
	{
		var driveNumbers = new List<int>();

		new Machine().DOS.EnumerateDriveParameterBlocks(
			(addr, ref dpb) =>
			{
				if ((dpb.DriveIdentifier >= 0) && (dpb.DriveIdentifier <= 25))
					driveNumbers.Add(dpb.DriveIdentifier + 1); // Convert A=0 to A=1

				return true;
			});

		yield return [0];
		foreach (var driveNumber in driveNumbers)
			yield return [driveNumber];
	}

	[TestCaseSource(nameof(EnumerateDriveNumbers))]
	public void GetDPB_should_return_correct_address(int driveNumber)
	{
		// Arrange
		var machine = new Machine();

		if (driveNumber == 0)
			driveNumber = machine.DOS.GetDefaultDrive();
		else
			driveNumber--;

		var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

		var rin = new RegistersEx();

		rin.AX = (int)Interrupt0x21.Function.GetDPB << 8;
		rin.DX = (ushort)driveNumber;

		var expectedAddress = machine.DOS.GetDriveParameterBlock(driveNumber);

		// Act
		var rout = sut.Execute(rin);

		var actualAddress = new SegmentedAddress(rout.AsRegistersEx().DS, rout.BX);

		// Assert
		int al = rout.AX & 0xFF;

		al.Should().Be(0);

		actualAddress.Should().BeEquivalentTo(expectedAddress);
	}

	[Test]
	public void GetCtrlCCheckFlag_should_return_successfully()
	{
		// Arrange
		var machine = new Machine();

		var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

		var rin = new RegistersEx();

		rin.AX = (int)Interrupt0x21.Function.Function33 << 8;
		rin.AX |= (int)Interrupt0x21.Function33.GetCtrlCCheckFlag;

		// Act
		var rout = sut.Execute(rin);

		// Assert
		int dl = rout.DX & 0xFF;

		dl.Should().BeOneOf(0, 1);
	}

	[Test]
	public void SetCtrlCCheckFlag_should_return_successfully()
	{
		// Arrange
		var machine = new Machine();

		var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

		var rin = new RegistersEx();

		rin.AX = (int)Interrupt0x21.Function.Function33 << 8;
		rin.AX |= (int)Interrupt0x21.Function33.SetCtrlCCheckFlag;

		// Act
		var rout = sut.Execute(rin);
	}

	[Test, NonParallelizable]
	public void GetStartupDrive_should_return_current_drive_at_startup()
	{
		// Arrange
		var preStartupCurrentDirectory = Environment.CurrentDirectory;

		int expectedStartupDrive = PathCharacter.GetDriveLetter(preStartupCurrentDirectory) - 'A' + 1;

		var machine = new Machine();

		var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

		var rin = new RegistersEx();

		rin.AX = (int)Interrupt0x21.Function.Function33 << 8;
		rin.AX |= (int)Interrupt0x21.Function33.GetStartupDrive;

		// Act
		var rout = sut.Execute(rin);

		// Assert
		int dl = rout.DX & 0xFF;

		dl.Should().Be(expectedStartupDrive);
	}

	[Test]
	public void GetMSDOSVersion_should_return_some_values()
	{
		// Arrange
		var machine = new Machine();

		var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

		var rin = new RegistersEx();

		rin.AX = (int)Interrupt0x21.Function.Function33 << 8;
		rin.AX |= (int)Interrupt0x21.Function33.GetMSDOSVersion;

		// Act
		var rout = sut.Execute(rin);

		int versionMajor = rout.BX & 0xFF;
		int versionMinor = rout.BX >> 8;
		int revisionNumber = rout.DX & 0b00000111;
		int versionFlag = rout.DX >> 8;

		// Assert
		versionMajor.Should().BeInRange(5, 6);
		versionMinor.Should().Be(0);
		revisionNumber.Should().Be(0);
		versionFlag.Should().Be(0x08);
	}

	[Test]
	public void GetInDOSFlagAddress_should_return_correct_address()
	{
		// Arrange
		var machine = new Machine();

		var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

		var rin = new RegistersEx();

		rin.AX = (int)Interrupt0x21.Function.GetInDOSFlagAddress << 8;

		var expectedValue = machine.DOS.InDOSFlagAddress;

		// Act
		var rout = sut.Execute(rin);

		// Assert
		var actualValue = new SegmentedAddress(
			rout.AsRegistersEx().ES,
			rout.BX);

		actualValue.Should().BeEquivalentTo(expectedValue);
	}

	[Test]
	public void GetDiskFreeSpace_should_return_some_numbers_for_default_drive()
	{
		// Arrange
		var machine = new Machine();

		var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

		var rin = new RegistersEx();

		rin.AX = (int)Interrupt0x21.Function.GetDiskFreeSpace << 8;

		// DL 0 = use default drive

		// Act
		var rout = sut.Execute(rin);

		// Assert
		rout.AX.Should().NotBe(0xFFFF);
		rout.BX.Should().Be(0xFFFF);
		rout.CX.Should().NotBe(0);
		rout.DX.Should().Be(0xFFFF);
	}

	[Test]
	public void GetDiskFreeSpace_should_return_some_values_when_drive_identifier_is_specified()
	{
		// Arrange
		int driveIdentifier = -1;

		for (char driveLetter = 'A'; driveLetter <= 'Z'; driveLetter++)
		{
			if (new DriveInfo(string.Concat(driveLetter, PathCharacter.VolumeSeparatorChar)).DriveType != DriveType.NoRootDirectory)
			{
				driveIdentifier = driveLetter - 'A' + 1;
				break;
			}
		}

		if (driveIdentifier < 0)
			driveIdentifier = 3; // "C:/" synthetic drive on platforms with no drive letters

		var machine = new Machine();

		var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

		var rin = new RegistersEx();

		rin.AX = (int)Interrupt0x21.Function.GetDiskFreeSpace << 8;
		rin.DX = (ushort)driveIdentifier;

		// Act
		var rout = sut.Execute(rin);

		// Assert
		rout.AX.Should().NotBe(0xFFFF);
		rout.BX.Should().Be(0xFFFF);
		rout.CX.Should().NotBe(0);
		rout.DX.Should().Be(0xFFFF);
	}

	[Test]
	public void GetDiskFreeSpace_should_fail_on_valid_drive_identifier_that_does_not_refer_to_any_drive()
	{
		// Arrange
		int driveIdentifier = -1;

		for (char driveLetter = 'A'; driveLetter <= 'Z'; driveLetter++)
		{
			if (driveLetter != 'C')
			{
				if (new DriveInfo(string.Concat(driveLetter, PathCharacter.VolumeSeparatorChar)).DriveType == DriveType.NoRootDirectory)
				{
					driveIdentifier = driveLetter - 'A' + 1;
					break;
				}
			}
		}

		if (driveIdentifier < 0)
			throw new InconclusiveException("Couldn't find an unused drive letter");

		var machine = new Machine();

		var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

		var rin = new RegistersEx();

		rin.AX = (int)Interrupt0x21.Function.GetDiskFreeSpace << 8;
		rin.DX = (ushort)driveIdentifier;

		// Act
		var rout = sut.Execute(rin);

		// Assert
		rout.AX.Should().Be(0xFFFF);
	}

	[Test]
	public void GetSetCountryInformation_should_retrieve_current_country_information()
	{
		// Arrange
		var machine = new Machine();

		machine.DOS.SetUpRunningProgramSegmentPrefix("");

		var countryInfoBufferAddress = machine.DOS.MemoryManager.AllocateMemory(CountryInfo.Size, machine.DOS.CurrentPSPSegment);

		var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

		var rin = new RegistersEx();

		rin.AX = (int)Interrupt0x21.Function.GetSetCountryInformation << 8;
		rin.DS = (ushort)(countryInfoBufferAddress / MemoryManager.ParagraphSize);
		rin.DX = (ushort)(countryInfoBufferAddress % MemoryManager.ParagraphSize);

		var expectedInfo = machine.DOS.CurrentCulture;

		var expectedCountryCode = expectedInfo.ToCountryCode();

		var expectedCountryInfo = new CountryInfo();

		expectedCountryInfo.Import(expectedInfo);

		// Act
		var rout = sut.Execute(rin);

		// Assert
		rout.FLAGS.Should().NotHaveFlag(Flags.Carry);

		int al = rout.AX & 0xFF;

		al.Should().Be((int)expectedCountryCode & 0xFF);
		rout.DX.Should().Be((ushort)expectedCountryCode);

		var actualCountryInfo = new CountryInfo();

		actualCountryInfo.Deserialize(machine.MemoryBus, countryInfoBufferAddress);

		actualCountryInfo.Should().BeEquivalentTo(expectedCountryInfo);
	}

	[Test]
	public void GetSetCountryInformation_should_set_current_country_information([Values] CountryCode countryCode)
	{
		// Arrange
		var machine = new Machine();

		var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

		var rin = new RegistersEx();

		rin.AX = (int)Interrupt0x21.Function.GetSetCountryInformation << 8;
		rin.DX = 0xFFFF;

		if ((int)countryCode <= 254)
			rin.AX |= (ushort)countryCode;
		else
		{
			rin.AX |= 0xFF;
			rin.BX = (ushort)countryCode;
		}

		var expectedCultureInfo = CultureInfo.GetCultureInfo(countryCode.ToCultureName());

		// Act
		var rout = sut.Execute(rin);

		// Assert
		rout.FLAGS.Should().NotHaveFlag(Flags.Carry);

		machine.DOS.CurrentCulture.Should().Be(expectedCultureInfo);
	}

	[Test, NonParallelizable]
	public void CreateDirectory_should_create_directories()
	{
		// Arrange
		using (var workspace = new TemporaryDirectory())
		{
			Environment.CurrentDirectory = workspace.Path;

			var machine = new Machine();

			machine.DOS.SetUpRunningProgramSegmentPrefix("");

			const string TestDirectoryName = "TESTDIR.FOO";

			byte[] directoryNameBytes = s_cp437.GetBytes(TestDirectoryName);

			int directoryNameBufferSize = directoryNameBytes.Length + 1;

			var directoryNameAddress = machine.DOS.MemoryManager.AllocateMemory(directoryNameBufferSize, machine.DOS.CurrentPSPSegment);

			var directoryNameSpan = machine.SystemMemory.AsSpan().Slice(directoryNameAddress, directoryNameBufferSize);

			directoryNameBytes.CopyTo(directoryNameSpan);
			directoryNameSpan[directoryNameBytes.Length] = 0;

			var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

			var rin = new RegistersEx();

			rin.AX = (int)Interrupt0x21.Function.CreateDirectory << 8;
			rin.DS = (ushort)(directoryNameAddress / MemoryManager.ParagraphSize);
			rin.DX = (ushort)(directoryNameAddress % MemoryManager.ParagraphSize);

			// Act
			bool existsBefore = Directory.Exists(TestDirectoryName);

			var rout = sut.Execute(rin);

			bool existsAfter = Directory.Exists(TestDirectoryName);

			// Assert
			int al = rout.AX & 0xFF;

			al.Should().Be(0);

			existsBefore.Should().BeFalse();
			existsAfter.Should().BeTrue();
		}
	}

	[Test, NonParallelizable]
	public void CreateDirectory_should_create_directories_with_absolute_paths()
	{
		// Arrange
		using (var workspace = new TemporaryDirectory())
		{
			Environment.CurrentDirectory = workspace.Path;

			var machine = new Machine();

			machine.DOS.SetUpRunningProgramSegmentPrefix("");

			const string ExistingDirectoryName = "A";

			const string TestDirectoryName = "TESTDIR.FOO";

			Directory.CreateDirectory(ExistingDirectoryName);

			if (!ShortFileNames.TryMap(Environment.CurrentDirectory, out var currentDirectoryShortPath))
				throw new Exception("Couldn't map current directory");

			var testDirectoryPath = Path.Join(ExistingDirectoryName, TestDirectoryName);

			var testDirectoryAbsolutePath = Path.Join(currentDirectoryShortPath, ExistingDirectoryName, TestDirectoryName);

			byte[] directoryNameBytes = s_cp437.GetBytes(testDirectoryAbsolutePath);

			int directoryNameBufferSize = directoryNameBytes.Length + 1;

			var directoryNameAddress = machine.DOS.MemoryManager.AllocateMemory(directoryNameBufferSize, machine.DOS.CurrentPSPSegment);

			var directoryNameSpan = machine.SystemMemory.AsSpan().Slice(directoryNameAddress, directoryNameBufferSize);

			directoryNameBytes.CopyTo(directoryNameSpan);
			directoryNameSpan[directoryNameBytes.Length] = 0;

			var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

			var rin = new RegistersEx();

			rin.AX = (int)Interrupt0x21.Function.CreateDirectory << 8;
			rin.DS = (ushort)(directoryNameAddress / MemoryManager.ParagraphSize);
			rin.DX = (ushort)(directoryNameAddress % MemoryManager.ParagraphSize);

			// Act
			bool existsBefore = Directory.Exists(testDirectoryPath);

			var rout = sut.Execute(rin);

			bool existsAfter = Directory.Exists(testDirectoryPath);

			// Assert
			int al = rout.AX & 0xFF;

			al.Should().Be(0);

			existsBefore.Should().BeFalse();
			existsAfter.Should().BeTrue();
		}
	}

	[Test, NonParallelizable]
	public void CreateDirectory_should_create_directories_with_relative_paths()
	{
		// Arrange
		using (var workspace = new TemporaryDirectory())
		{
			Environment.CurrentDirectory = workspace.Path;

			var machine = new Machine();

			machine.DOS.SetUpRunningProgramSegmentPrefix("");

			const string ExistingDirectoryName = "A";

			const string TestDirectoryName = "TESTDIR.FOO";

			Directory.CreateDirectory(ExistingDirectoryName);

			var testDirectoryPath = Path.Combine(ExistingDirectoryName, TestDirectoryName);

			byte[] directoryNameBytes = s_cp437.GetBytes(testDirectoryPath);

			int directoryNameBufferSize = directoryNameBytes.Length + 1;

			var directoryNameAddress = machine.DOS.MemoryManager.AllocateMemory(directoryNameBufferSize, machine.DOS.CurrentPSPSegment);

			var directoryNameSpan = machine.SystemMemory.AsSpan().Slice(directoryNameAddress, directoryNameBufferSize);

			directoryNameBytes.CopyTo(directoryNameSpan);
			directoryNameSpan[directoryNameBytes.Length] = 0;

			var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

			var rin = new RegistersEx();

			rin.AX = (int)Interrupt0x21.Function.CreateDirectory << 8;
			rin.DS = (ushort)(directoryNameAddress / MemoryManager.ParagraphSize);
			rin.DX = (ushort)(directoryNameAddress % MemoryManager.ParagraphSize);

			// Act
			bool existsBefore = Directory.Exists(testDirectoryPath);

			var rout = sut.Execute(rin);

			bool existsAfter = Directory.Exists(testDirectoryPath);

			// Assert
			int al = rout.AX & 0xFF;

			al.Should().Be(0);

			existsBefore.Should().BeFalse();
			existsAfter.Should().BeTrue();
		}
	}

	[Test, NonParallelizable]
	public void CreateDirectory_should_create_directories_with_relative_paths_including_parent_references()
	{
		// Arrange
		using (var workspace = new TemporaryDirectory())
		{
			Environment.CurrentDirectory = workspace.Path;

			var machine = new Machine();

			machine.DOS.SetUpRunningProgramSegmentPrefix("");

			const string CurrentDirectoryName = "A";

			Directory.CreateDirectory(CurrentDirectoryName);
			Environment.CurrentDirectory = CurrentDirectoryName;

			const string TestDirectoryName = "TESTDIR.FOO";

			var testDirectoryPath = Path.Combine("..", TestDirectoryName);

			byte[] directoryNameBytes = s_cp437.GetBytes(testDirectoryPath);

			int directoryNameBufferSize = directoryNameBytes.Length + 1;

			var directoryNameAddress = machine.DOS.MemoryManager.AllocateMemory(directoryNameBufferSize, machine.DOS.CurrentPSPSegment);

			var directoryNameSpan = machine.SystemMemory.AsSpan().Slice(directoryNameAddress, directoryNameBufferSize);

			directoryNameBytes.CopyTo(directoryNameSpan);
			directoryNameSpan[directoryNameBytes.Length] = 0;

			var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

			var rin = new RegistersEx();

			rin.AX = (int)Interrupt0x21.Function.CreateDirectory << 8;
			rin.DS = (ushort)(directoryNameAddress / MemoryManager.ParagraphSize);
			rin.DX = (ushort)(directoryNameAddress % MemoryManager.ParagraphSize);

			// Act
			bool existsBefore = Directory.Exists(testDirectoryPath);

			var rout = sut.Execute(rin);

			bool existsAfter = Directory.Exists(testDirectoryPath);

			// Assert
			int al = rout.AX & 0xFF;

			al.Should().Be(0);

			existsBefore.Should().BeFalse();
			existsAfter.Should().BeTrue();
		}
	}

	[Test, NonParallelizable]
	public void RemoveDirectory_should_remove_directories()
	{
		// Arrange
		using (var workspace = new TemporaryDirectory())
		{
			Environment.CurrentDirectory = workspace.Path;

			var machine = new Machine();

			machine.DOS.SetUpRunningProgramSegmentPrefix("");

			const string TestDirectoryName = "TESTDIR.FOO";

			Directory.CreateDirectory(TestDirectoryName);

			byte[] directoryNameBytes = s_cp437.GetBytes(TestDirectoryName);

			int directoryNameBufferSize = directoryNameBytes.Length + 1;

			var directoryNameAddress = machine.DOS.MemoryManager.AllocateMemory(directoryNameBufferSize, machine.DOS.CurrentPSPSegment);

			var directoryNameSpan = machine.SystemMemory.AsSpan().Slice(directoryNameAddress, directoryNameBufferSize);

			directoryNameBytes.CopyTo(directoryNameSpan);
			directoryNameSpan[directoryNameBytes.Length] = 0;

			var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

			var rin = new RegistersEx();

			rin.AX = (int)Interrupt0x21.Function.RemoveDirectory << 8;
			rin.DS = (ushort)(directoryNameAddress / MemoryManager.ParagraphSize);
			rin.DX = (ushort)(directoryNameAddress % MemoryManager.ParagraphSize);

			// Act
			bool existsBefore = Directory.Exists(TestDirectoryName);

			var rout = sut.Execute(rin);

			bool existsAfter = Directory.Exists(TestDirectoryName);

			// Assert
			int al = rout.AX & 0xFF;

			al.Should().Be(0);

			existsBefore.Should().BeTrue();
			existsAfter.Should().BeFalse();
		}
	}

	[Test, NonParallelizable]
	public void RemoveDirectory_should_remove_directories_with_absolute_paths()
	{
		// Arrange
		using (var workspace = new TemporaryDirectory())
		{
			Environment.CurrentDirectory = workspace.Path;

			var machine = new Machine();

			machine.DOS.SetUpRunningProgramSegmentPrefix("");

			const string ExistingDirectoryName = "A";

			const string TestDirectoryName = "TESTDIR.FOO";

			if (!ShortFileNames.TryMap(Environment.CurrentDirectory, out var currentDirectoryShortPath))
				throw new Exception("Couldn't map current directory");

			var testDirectoryPath = Path.Join(ExistingDirectoryName, TestDirectoryName);

			Directory.CreateDirectory(testDirectoryPath);

			var testDirectoryAbsolutePath = Path.Join(currentDirectoryShortPath, ExistingDirectoryName, TestDirectoryName);

			byte[] directoryNameBytes = s_cp437.GetBytes(testDirectoryAbsolutePath);

			int directoryNameBufferSize = directoryNameBytes.Length + 1;

			var directoryNameAddress = machine.DOS.MemoryManager.AllocateMemory(directoryNameBufferSize, machine.DOS.CurrentPSPSegment);

			var directoryNameSpan = machine.SystemMemory.AsSpan().Slice(directoryNameAddress, directoryNameBufferSize);

			directoryNameBytes.CopyTo(directoryNameSpan);
			directoryNameSpan[directoryNameBytes.Length] = 0;

			var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

			var rin = new RegistersEx();

			rin.AX = (int)Interrupt0x21.Function.RemoveDirectory << 8;
			rin.DS = (ushort)(directoryNameAddress / MemoryManager.ParagraphSize);
			rin.DX = (ushort)(directoryNameAddress % MemoryManager.ParagraphSize);

			// Act
			bool existsBefore = Directory.Exists(testDirectoryPath);

			var rout = sut.Execute(rin);

			bool existsAfter = Directory.Exists(testDirectoryPath);

			// Assert
			int al = rout.AX & 0xFF;

			al.Should().Be(0);

			existsBefore.Should().BeTrue();
			existsAfter.Should().BeFalse();
		}
	}

	[Test, NonParallelizable]
	public void RemoveDirectory_should_remove_directories_with_relative_paths()
	{
		// Arrange
		using (var workspace = new TemporaryDirectory())
		{
			Environment.CurrentDirectory = workspace.Path;

			var machine = new Machine();

			machine.DOS.SetUpRunningProgramSegmentPrefix("");

			const string ExistingDirectoryName = "A";

			const string TestDirectoryName = "TESTDIR.FOO";

			var testDirectoryPath = Path.Combine(ExistingDirectoryName, TestDirectoryName);

			Directory.CreateDirectory(testDirectoryPath);

			byte[] directoryNameBytes = s_cp437.GetBytes(testDirectoryPath);

			int directoryNameBufferSize = directoryNameBytes.Length + 1;

			var directoryNameAddress = machine.DOS.MemoryManager.AllocateMemory(directoryNameBufferSize, machine.DOS.CurrentPSPSegment);

			var directoryNameSpan = machine.SystemMemory.AsSpan().Slice(directoryNameAddress, directoryNameBufferSize);

			directoryNameBytes.CopyTo(directoryNameSpan);
			directoryNameSpan[directoryNameBytes.Length] = 0;

			var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

			var rin = new RegistersEx();

			rin.AX = (int)Interrupt0x21.Function.RemoveDirectory << 8;
			rin.DS = (ushort)(directoryNameAddress / MemoryManager.ParagraphSize);
			rin.DX = (ushort)(directoryNameAddress % MemoryManager.ParagraphSize);

			// Act
			bool existsBefore = Directory.Exists(testDirectoryPath);

			var rout = sut.Execute(rin);

			bool existsAfter = Directory.Exists(testDirectoryPath);

			// Assert
			int al = rout.AX & 0xFF;

			al.Should().Be(0);

			existsBefore.Should().BeTrue();
			existsAfter.Should().BeFalse();
		}
	}

	[Test, NonParallelizable]
	public void RemoveDirectory_should_remove_directories_with_relative_paths_including_parent_references()
	{
		// Arrange
		using (var workspace = new TemporaryDirectory())
		{
			Environment.CurrentDirectory = workspace.Path;

			var machine = new Machine();

			machine.DOS.SetUpRunningProgramSegmentPrefix("");

			const string CurrentDirectoryName = "A";

			Directory.CreateDirectory(CurrentDirectoryName);
			Environment.CurrentDirectory = CurrentDirectoryName;

			const string TestDirectoryName = "TESTDIR.FOO";

			var testDirectoryPath = Path.Combine("..", TestDirectoryName);

			Directory.CreateDirectory(testDirectoryPath);

			byte[] directoryNameBytes = s_cp437.GetBytes(testDirectoryPath);

			int directoryNameBufferSize = directoryNameBytes.Length + 1;

			var directoryNameAddress = machine.DOS.MemoryManager.AllocateMemory(directoryNameBufferSize, machine.DOS.CurrentPSPSegment);

			var directoryNameSpan = machine.SystemMemory.AsSpan().Slice(directoryNameAddress, directoryNameBufferSize);

			directoryNameBytes.CopyTo(directoryNameSpan);
			directoryNameSpan[directoryNameBytes.Length] = 0;

			var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

			var rin = new RegistersEx();

			rin.AX = (int)Interrupt0x21.Function.RemoveDirectory << 8;
			rin.DS = (ushort)(directoryNameAddress / MemoryManager.ParagraphSize);
			rin.DX = (ushort)(directoryNameAddress % MemoryManager.ParagraphSize);

			// Act
			bool existsBefore = Directory.Exists(testDirectoryPath);

			var rout = sut.Execute(rin);

			bool existsAfter = Directory.Exists(testDirectoryPath);

			// Assert
			int al = rout.AX & 0xFF;

			al.Should().Be(0);

			existsBefore.Should().BeTrue();
			existsAfter.Should().BeFalse();
		}
	}

	[Test, NonParallelizable]
	public void ChangeCurrentDirectory_should_change_directories()
	{
		// Arrange
		using (var workspace = new TemporaryDirectory())
		{
			Environment.CurrentDirectory = workspace.Path;

			var machine = new Machine();

			machine.DOS.SetUpRunningProgramSegmentPrefix("");

			const string TestDirectoryName = "TESTDIR.FOO";

			Directory.CreateDirectory(TestDirectoryName);

			string semaphoreFileName = Guid.NewGuid().ToString();

			File.WriteAllText(Path.Join(TestDirectoryName, semaphoreFileName), "test");

			byte[] directoryNameBytes = s_cp437.GetBytes(TestDirectoryName);

			int directoryNameBufferSize = directoryNameBytes.Length + 1;

			var directoryNameAddress = machine.DOS.MemoryManager.AllocateMemory(directoryNameBufferSize, machine.DOS.CurrentPSPSegment);

			var directoryNameSpan = machine.SystemMemory.AsSpan().Slice(directoryNameAddress, directoryNameBufferSize);

			directoryNameBytes.CopyTo(directoryNameSpan);
			directoryNameSpan[directoryNameBytes.Length] = 0;

			var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

			var rin = new RegistersEx();

			rin.AX = (int)Interrupt0x21.Function.ChangeCurrentDirectory << 8;
			rin.DS = (ushort)(directoryNameAddress / MemoryManager.ParagraphSize);
			rin.DX = (ushort)(directoryNameAddress % MemoryManager.ParagraphSize);

			// Act
			bool existsBefore = File.Exists(semaphoreFileName);

			var rout = sut.Execute(rin);

			bool existsAfter = File.Exists(semaphoreFileName);

			// Assert
			int al = rout.AX & 0xFF;

			al.Should().Be(0);

			existsBefore.Should().BeFalse();
			existsAfter.Should().BeTrue();
		}
	}

	[Test, NonParallelizable]
	public void ChangeCurrentDirectory_should_change_directories_with_absolute_paths()
	{
		// Arrange
		using (var workspace = new TemporaryDirectory())
		{
			Environment.CurrentDirectory = workspace.Path;

			var machine = new Machine();

			machine.DOS.SetUpRunningProgramSegmentPrefix("");

			const string ExistingDirectoryName = "A";

			const string TestDirectoryName = "TESTDIR.FOO";

			if (!ShortFileNames.TryMap(Environment.CurrentDirectory, out var currentDirectoryShortPath))
				throw new Exception("Couldn't map current directory");

			var testDirectoryPath = Path.Join(ExistingDirectoryName, TestDirectoryName);

			Directory.CreateDirectory(testDirectoryPath);

			string semaphoreFileName = Guid.NewGuid().ToString();

			File.WriteAllText(Path.Join(testDirectoryPath, semaphoreFileName), "test");

			var testDirectoryAbsolutePath = Path.Join(currentDirectoryShortPath, ExistingDirectoryName, TestDirectoryName);

			byte[] directoryNameBytes = s_cp437.GetBytes(testDirectoryAbsolutePath);

			int directoryNameBufferSize = directoryNameBytes.Length + 1;

			var directoryNameAddress = machine.DOS.MemoryManager.AllocateMemory(directoryNameBufferSize, machine.DOS.CurrentPSPSegment);

			var directoryNameSpan = machine.SystemMemory.AsSpan().Slice(directoryNameAddress, directoryNameBufferSize);

			directoryNameBytes.CopyTo(directoryNameSpan);
			directoryNameSpan[directoryNameBytes.Length] = 0;

			var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

			var rin = new RegistersEx();

			rin.AX = (int)Interrupt0x21.Function.ChangeCurrentDirectory << 8;
			rin.DS = (ushort)(directoryNameAddress / MemoryManager.ParagraphSize);
			rin.DX = (ushort)(directoryNameAddress % MemoryManager.ParagraphSize);

			// Act
			bool existsBefore = File.Exists(semaphoreFileName);

			var rout = sut.Execute(rin);

			bool existsAfter = File.Exists(semaphoreFileName);

			// Assert
			int al = rout.AX & 0xFF;

			al.Should().Be(0);

			existsBefore.Should().BeFalse();
			existsAfter.Should().BeTrue();
		}
	}

	[Test, NonParallelizable]
	public void ChangeCurrentDirectory_should_change_directories_with_relative_paths()
	{
		// Arrange
		using (var workspace = new TemporaryDirectory())
		{
			Environment.CurrentDirectory = workspace.Path;

			var machine = new Machine();

			machine.DOS.SetUpRunningProgramSegmentPrefix("");

			const string ExistingDirectoryName = "A";

			const string TestDirectoryName = "TESTDIR.FOO";

			var testDirectoryPath = Path.Combine(ExistingDirectoryName, TestDirectoryName);

			Directory.CreateDirectory(testDirectoryPath);

			string semaphoreFileName = Guid.NewGuid().ToString();

			File.WriteAllText(Path.Join(testDirectoryPath, semaphoreFileName), "test");

			byte[] directoryNameBytes = s_cp437.GetBytes(testDirectoryPath);

			int directoryNameBufferSize = directoryNameBytes.Length + 1;

			var directoryNameAddress = machine.DOS.MemoryManager.AllocateMemory(directoryNameBufferSize, machine.DOS.CurrentPSPSegment);

			var directoryNameSpan = machine.SystemMemory.AsSpan().Slice(directoryNameAddress, directoryNameBufferSize);

			directoryNameBytes.CopyTo(directoryNameSpan);
			directoryNameSpan[directoryNameBytes.Length] = 0;

			var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

			var rin = new RegistersEx();

			rin.AX = (int)Interrupt0x21.Function.ChangeCurrentDirectory << 8;
			rin.DS = (ushort)(directoryNameAddress / MemoryManager.ParagraphSize);
			rin.DX = (ushort)(directoryNameAddress % MemoryManager.ParagraphSize);

			// Act
			bool existsBefore = File.Exists(semaphoreFileName);

			var rout = sut.Execute(rin);

			bool existsAfter = File.Exists(semaphoreFileName);

			// Assert
			int al = rout.AX & 0xFF;

			al.Should().Be(0);

			existsBefore.Should().BeFalse();
			existsAfter.Should().BeTrue();
		}
	}

	[Test, NonParallelizable]
	public void ChangeCurrentDirectory_should_change_directories_with_relative_paths_including_parent_references()
	{
		// Arrange
		using (var workspace = new TemporaryDirectory())
		{
			Environment.CurrentDirectory = workspace.Path;

			var machine = new Machine();

			machine.DOS.SetUpRunningProgramSegmentPrefix("");

			const string CurrentDirectoryName = "A";

			Directory.CreateDirectory(CurrentDirectoryName);
			Environment.CurrentDirectory = CurrentDirectoryName;

			const string TestDirectoryName = "TESTDIR.FOO";

			var testDirectoryPath = Path.Combine("..", TestDirectoryName);

			Directory.CreateDirectory(testDirectoryPath);

			string semaphoreFileName = Guid.NewGuid().ToString();

			File.WriteAllText(Path.Join(testDirectoryPath, semaphoreFileName), "test");

			byte[] directoryNameBytes = s_cp437.GetBytes(testDirectoryPath);

			int directoryNameBufferSize = directoryNameBytes.Length + 1;

			var directoryNameAddress = machine.DOS.MemoryManager.AllocateMemory(directoryNameBufferSize, machine.DOS.CurrentPSPSegment);

			var directoryNameSpan = machine.SystemMemory.AsSpan().Slice(directoryNameAddress, directoryNameBufferSize);

			directoryNameBytes.CopyTo(directoryNameSpan);
			directoryNameSpan[directoryNameBytes.Length] = 0;

			var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

			var rin = new RegistersEx();

			rin.AX = (int)Interrupt0x21.Function.ChangeCurrentDirectory << 8;
			rin.DS = (ushort)(directoryNameAddress / MemoryManager.ParagraphSize);
			rin.DX = (ushort)(directoryNameAddress % MemoryManager.ParagraphSize);

			// Act
			bool existsBefore = File.Exists(semaphoreFileName);

			var rout = sut.Execute(rin);

			bool existsAfter = File.Exists(semaphoreFileName);

			// Assert
			int al = rout.AX & 0xFF;

			al.Should().Be(0);

			existsBefore.Should().BeFalse();
			existsAfter.Should().BeTrue();
		}
	}

	[Test, NonParallelizable]
	public void CreateFileWithHandle_should_create_files()
	{
		// Arrange
		using (var workspace = new TemporaryDirectory())
		{
			Environment.CurrentDirectory = workspace.Path;

			var machine = new Machine();

			machine.DOS.SetUpRunningProgramSegmentPrefix("");

			const string TestFileName = "TESTFILE.TXT";

			byte[] fileNameBytes = s_cp437.GetBytes(TestFileName);

			int fileNameBufferSize = fileNameBytes.Length + 1;

			var fileNameAddress = machine.DOS.MemoryManager.AllocateMemory(fileNameBufferSize, machine.DOS.CurrentPSPSegment);

			var fileNameSpan = machine.SystemMemory.AsSpan().Slice(fileNameAddress, fileNameBufferSize);

			fileNameBytes.CopyTo(fileNameSpan);
			fileNameSpan[fileNameBytes.Length] = 0;

			int fileHandle = -1;

			try
			{
				var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

				var rin = new RegistersEx();

				rin.AX = (int)Interrupt0x21.Function.CreateFileWithHandle << 8;
				rin.DS = (ushort)(fileNameAddress / MemoryManager.ParagraphSize);
				rin.DX = (ushort)(fileNameAddress % MemoryManager.ParagraphSize);
				rin.CX = (ushort)QBX.OperatingSystem.FileStructures.FileAttributes.Normal;

				// Act
				bool existsBefore = File.Exists(TestFileName);

				var rout = sut.Execute(rin);

				bool existsAfter = File.Exists(TestFileName);

				// Assert
				rout.FLAGS.Should().NotHaveFlag(Flags.Carry);

				fileHandle = rout.AX;

				existsBefore.Should().BeFalse();
				existsAfter.Should().BeTrue();

				fileHandle.Should().BeInRange(2, machine.DOS.Files.Count - 1);

				var regularFile = (RegularFileDescriptor)machine.DOS.Files[fileHandle]!;

				regularFile.PhysicalPath.Should().Be(Path.GetFullPath(TestFileName));
			}
			finally
			{
				machine.DOS.CloseFile(fileHandle);
			}
		}
	}

	[Test, NonParallelizable]
	public void CreateFileWithHandle_should_truncate_existing_files()
	{
		// Arrange
		using (var workspace = new TemporaryDirectory())
		{
			Environment.CurrentDirectory = workspace.Path;

			var machine = new Machine();

			machine.DOS.SetUpRunningProgramSegmentPrefix("");

			const string TestFileName = "TESTFILE.TXT";

			File.WriteAllText(TestFileName, "QuickBASIC");

			byte[] fileNameBytes = s_cp437.GetBytes(TestFileName);

			int fileNameBufferSize = fileNameBytes.Length + 1;

			var fileNameAddress = machine.DOS.MemoryManager.AllocateMemory(fileNameBufferSize, machine.DOS.CurrentPSPSegment);

			var fileNameSpan = machine.SystemMemory.AsSpan().Slice(fileNameAddress, fileNameBufferSize);

			fileNameBytes.CopyTo(fileNameSpan);
			fileNameSpan[fileNameBytes.Length] = 0;

			int fileHandle = -1;

			try
			{
				var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

				var rin = new RegistersEx();

				rin.AX = (int)Interrupt0x21.Function.CreateFileWithHandle << 8;
				rin.DS = (ushort)(fileNameAddress / MemoryManager.ParagraphSize);
				rin.DX = (ushort)(fileNameAddress % MemoryManager.ParagraphSize);

				// Act
				bool existsBefore = File.Exists(TestFileName);

				var rout = sut.Execute(rin);

				bool existsAfter = File.Exists(TestFileName);

				// Assert
				rout.FLAGS.Should().NotHaveFlag(Flags.Carry);

				fileHandle = rout.AX;

				existsBefore.Should().BeTrue();
				existsAfter.Should().BeTrue();

				fileHandle.Should().BeInRange(2, machine.DOS.Files.Count - 1);

				var regularFile = (RegularFileDescriptor)machine.DOS.Files[fileHandle]!;

				regularFile.PhysicalPath.Should().Be(Path.GetFullPath(TestFileName));

				string? pathRoot = Path.GetPathRoot(regularFile.Path) ?? "C:\\";

				new FileInfo(regularFile.PhysicalPath).Length.Should().Be(0);
			}
			finally
			{
				machine.DOS.CloseFile(fileHandle);
			}
		}
	}

	[Test, NonParallelizable]
	public void OpenFileWithHandle_should_open_files(
		[Values(OpenMode.Access_ReadOnly, OpenMode.Access_ReadWrite, OpenMode.Access_WriteOnly)] OpenMode openMode)
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

			byte[] fileNameBytes = s_cp437.GetBytes(TestFileName);

			int fileNameBufferSize = fileNameBytes.Length + 1;

			var fileNameAddress = machine.DOS.MemoryManager.AllocateMemory(fileNameBufferSize, machine.DOS.CurrentPSPSegment);

			var fileNameSpan = machine.SystemMemory.AsSpan().Slice(fileNameAddress, fileNameBufferSize);

			fileNameBytes.CopyTo(fileNameSpan);
			fileNameSpan[fileNameBytes.Length] = 0;

			bool shouldBeReadable = (openMode == OpenMode.Access_ReadWrite) || (openMode == OpenMode.Access_ReadOnly);
			bool shouldBeWritable = (openMode == OpenMode.Access_ReadWrite) || (openMode == OpenMode.Access_WriteOnly);

			int fileHandle = -1;

			try
			{
				var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

				var rin = new RegistersEx();

				rin.AX = (int)Interrupt0x21.Function.OpenFileWithHandle << 8;
				rin.AX |= (ushort)openMode;
				rin.DS = (ushort)(fileNameAddress / MemoryManager.ParagraphSize);
				rin.DX = (ushort)(fileNameAddress % MemoryManager.ParagraphSize);

				// Act
				var rout = sut.Execute(rin);

				// Assert
				rout.FLAGS.Should().NotHaveFlag(Flags.Carry);

				fileHandle = rout.AX;

				fileHandle.Should().BeInRange(2, machine.DOS.Files.Count - 1);

				var regularFile = (RegularFileDescriptor)machine.DOS.Files[fileHandle]!;

				regularFile.PhysicalPath.Should().Be(Path.GetFullPath(TestFileName));

				if (shouldBeReadable)
				{
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

				if (shouldBeWritable)
				{
					ReadAllBytesFromFile(TestFileName).AsSpan().ShouldMatch(testData, because: "it should not have truncated the file");

					TestContext.CurrentContext.Random.NextBytes(testData);

					regularFile.Seek(0, MoveMethod.FromBeginning);
					regularFile.Write(testData);
					regularFile.FlushWriteBuffer(flushToDisk: true);

					ReadAllBytesFromFile(TestFileName).AsSpan().ShouldMatch(testData);
				}
			}
			finally
			{
				machine.DOS.CloseFile(fileHandle);
			}
		}
	}

	private byte[] ReadAllBytesFromFile(string testFileName)
	{
		using (var stream = new FileStream(testFileName, System.IO.FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
		{
			byte[] data = new byte[stream.Length];

			stream.ReadExactly(data);

			return data;
		}
	}

	[Test, NonParallelizable]
	public void OpenFileWithHandle_should_open_files_with_relative_paths(
		[Values("SUB/TESTFILE.TXT", "../TESTFILE.TXT")] string testFileName)
	{
		// Arrange
		using (var workspace = new TemporaryDirectory())
		{
			Environment.CurrentDirectory = workspace.Path;

			Directory.CreateDirectory("A");

			Environment.CurrentDirectory = "A";

			if ((Path.GetDirectoryName(testFileName) is string containerName)
			 && (containerName != ".."))
				Directory.CreateDirectory(containerName);

			var machine = new Machine();

			machine.DOS.SetUpRunningProgramSegmentPrefix("");

			byte[] testData = s_cp437.GetBytes(Guid.NewGuid().ToString());

			File.WriteAllBytes(testFileName, testData);

			byte[] fileNameBytes = s_cp437.GetBytes(testFileName);

			int fileNameBufferSize = fileNameBytes.Length + 1;

			var fileNameAddress = machine.DOS.MemoryManager.AllocateMemory(fileNameBufferSize, machine.DOS.CurrentPSPSegment);

			var fileNameSpan = machine.SystemMemory.AsSpan().Slice(fileNameAddress, fileNameBufferSize);

			fileNameBytes.CopyTo(fileNameSpan);
			fileNameSpan[fileNameBytes.Length] = 0;

			int fileHandle = -1;

			try
			{
				var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

				var rin = new RegistersEx();

				rin.AX = (int)Interrupt0x21.Function.OpenFileWithHandle << 8;
				rin.AX |= (ushort)OpenMode.Access_ReadWrite;
				rin.DS = (ushort)(fileNameAddress / MemoryManager.ParagraphSize);
				rin.DX = (ushort)(fileNameAddress % MemoryManager.ParagraphSize);

				// Act
				var rout = sut.Execute(rin);

				// Assert
				rout.FLAGS.Should().NotHaveFlag(Flags.Carry);

				fileHandle = rout.AX;

				fileHandle.Should().BeInRange(2, machine.DOS.Files.Count - 1);

				var regularFile = (RegularFileDescriptor)machine.DOS.Files[fileHandle]!;

				regularFile.PhysicalPath.Should().Be(Path.GetFullPath(testFileName));

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
				machine.DOS.CloseFile(fileHandle);
			}
		}
	}

	[Test, NonParallelizable]
	public void OpenFileWithHandle_should_not_create_files()
	{
		// Arrange
		using (var workspace = new TemporaryDirectory())
		{
			Environment.CurrentDirectory = workspace.Path;

			var machine = new Machine();

			machine.DOS.SetUpRunningProgramSegmentPrefix("");

			const string TestFileName = "TESTFILE.TXT";

			byte[] fileNameBytes = s_cp437.GetBytes(TestFileName);

			int fileNameBufferSize = fileNameBytes.Length + 1;

			var fileNameAddress = machine.DOS.MemoryManager.AllocateMemory(fileNameBufferSize, machine.DOS.CurrentPSPSegment);

			var fileNameSpan = machine.SystemMemory.AsSpan().Slice(fileNameAddress, fileNameBufferSize);

			fileNameBytes.CopyTo(fileNameSpan);
			fileNameSpan[fileNameBytes.Length] = 0;

			var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

			var rin = new RegistersEx();

			rin.AX = (int)Interrupt0x21.Function.OpenFileWithHandle << 8;
			rin.DS = (ushort)(fileNameAddress / MemoryManager.ParagraphSize);
			rin.DX = (ushort)(fileNameAddress % MemoryManager.ParagraphSize);

			// Act
			var rout = sut.Execute(rin);

			// Assert
			rout.FLAGS.Should().HaveFlag(Flags.Carry);
			rout.AX.Should().Be((ushort)DOSError.FileNotFound);

			File.Exists(TestFileName).Should().BeFalse();
		}
	}

	[Test, NonParallelizable]
	public void CloseFileWithHandle_should_close_handle()
	{
		// Arrange
		using (var workspace = new TemporaryDirectory())
		{
			Environment.CurrentDirectory = workspace.Path;

			var machine = new Machine();

			machine.DOS.SetUpRunningProgramSegmentPrefix("");

			const string TestFileName = "TESTFILE.TXT";

			int fileHandle = machine.DOS.OpenFile(
				TestFileName,
				QBX.OperatingSystem.FileStructures.FileMode.CreateNew,
				OpenMode.Access_ReadWrite);

			fileHandle.Should().BeInRange(2, machine.DOS.Files.Count - 1);

			try
			{
				var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

				var rin = new RegistersEx();

				rin.AX = (int)Interrupt0x21.Function.CloseFileWithHandle << 8;
				rin.BX = (ushort)fileHandle;

				// Act
				var fileDescriptorBefore = (RegularFileDescriptor)machine.DOS.Files[fileHandle]!;

				var rout = sut.Execute(rin);

				var fileDescriptorAfter = (RegularFileDescriptor)machine.DOS.Files[fileHandle]!;

				// Assert
				rout.FLAGS.Should().NotHaveFlag(Flags.Carry);

				fileDescriptorBefore.Should().NotBeNull();
				fileDescriptorAfter.Should().BeNull();
			}
			finally
			{
				// should be a no-op on a defunct handle
				using (machine.DOS.SuppressExceptionsInScope())
					machine.DOS.CloseFile(fileHandle);
			}
		}
	}

	[Test, NonParallelizable]
	public void CloseFileWithHandle_should_fail_gracefully_on_invalid_handle()
	{
		// Arrange
		using (var workspace = new TemporaryDirectory())
		{
			Environment.CurrentDirectory = workspace.Path;

			var machine = new Machine();

			machine.DOS.SetUpRunningProgramSegmentPrefix("");

			var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

			var rin = new RegistersEx();

			rin.AX = (int)Interrupt0x21.Function.CloseFileWithHandle << 8;
			rin.BX = ushort.MaxValue;

			// Act
			var rout = sut.Execute(rin);

			// Assert
			rout.FLAGS.Should().HaveFlag(Flags.Carry);
			rout.AX.Should().Be((ushort)DOSError.InvalidHandle);
		}
	}

	[Test, NonParallelizable]
	public void CloseFileWithHandle_should_fail_gracefully_on_double_close()
	{
		// Arrange
		using (var workspace = new TemporaryDirectory())
		{
			Environment.CurrentDirectory = workspace.Path;

			var machine = new Machine();

			machine.DOS.SetUpRunningProgramSegmentPrefix("");

			const string TestFileName = "TESTFILE.TXT";

			int fileHandle = machine.DOS.OpenFile(
				TestFileName,
				QBX.OperatingSystem.FileStructures.FileMode.CreateNew,
				OpenMode.Access_ReadWrite);

			fileHandle.Should().BeInRange(2, machine.DOS.Files.Count - 1);

			try
			{
				var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

				var rin = new RegistersEx();

				rin.AX = (int)Interrupt0x21.Function.CloseFileWithHandle << 8;
				rin.BX = (ushort)fileHandle;

				var rin2 = new RegistersEx();

				rin2.AX = (int)Interrupt0x21.Function.CloseFileWithHandle << 8;
				rin2.BX = (ushort)fileHandle;

				// Act
				var fileDescriptorBefore = (RegularFileDescriptor)machine.DOS.Files[fileHandle]!;

				var rout = sut.Execute(rin);

				var fileDescriptorAfter = (RegularFileDescriptor)machine.DOS.Files[fileHandle]!;

				var rout2 = sut.Execute(rin2);

				// Assert
				rout.FLAGS.Should().NotHaveFlag(Flags.Carry);

				fileDescriptorBefore.Should().NotBeNull();
				fileDescriptorAfter.Should().BeNull();

				rout2.FLAGS.Should().HaveFlag(Flags.Carry);
				rout2.AX.Should().Be((ushort)DOSError.InvalidHandle);
			}
			finally
			{
				// should be a no-op on a defunct handle
				using (machine.DOS.SuppressExceptionsInScope())
					machine.DOS.CloseFile(fileHandle);
			}
		}
	}

	[Test, NonParallelizable]
	public void ReadFileOrDevice_should_read_data_from_files()
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

			int fileHandle = machine.DOS.OpenFile(
				TestFileName,
				QBX.OperatingSystem.FileStructures.FileMode.Open,
				OpenMode.Access_ReadWrite);

			const int ReadSize = 512;

			int bufferAddress = machine.DOS.MemoryManager.AllocateMemory(ReadSize, machine.DOS.CurrentPSPSegment);

			var bufferSpan = machine.SystemMemory.AsSpan().Slice(bufferAddress, ReadSize);

			try
			{
				var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

				var rin = new RegistersEx();

				rin.AX = (int)Interrupt0x21.Function.ReadFileOrDevice << 8;
				rin.BX = (ushort)fileHandle;
				rin.CX = (ushort)ReadSize;
				rin.DS = (ushort)(bufferAddress / MemoryManager.ParagraphSize);
				rin.DX = (ushort)(bufferAddress % MemoryManager.ParagraphSize);

				// Act
				var rout = sut.Execute(rin);

				// Assert
				rout.FLAGS.Should().NotHaveFlag(Flags.Carry);

				int actualBytes = rout.AX;

				actualBytes.Should().Be(testData.Length);

				bufferSpan.Slice(0, actualBytes).ShouldMatch(testData);
			}
			finally
			{
				machine.DOS.CloseFile(fileHandle);
			}
		}
	}

	[Test]
	public void ReadFileOrDevice_should_read_data_from_console()
	{
		// Arrange
		var machine = new Machine();

		SDL.Keymod modState = 0;

		machine.Keyboard.GetModStateTestHook += () => modState;

		string testCharacters = Guid.NewGuid().ToString();
		byte[] testData = s_cp437.GetBytes(testCharacters);

		SimulateTyping(testCharacters + '\r', ControlCharacterHandling.SemanticKey, machine);

		int bufferAddress = machine.DOS.MemoryManager.AllocateMemory(testCharacters.Length, machine.DOS.CurrentPSPSegment);

		var bufferSpan = machine.SystemMemory.AsSpan().Slice(bufferAddress, testCharacters.Length);

		var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

		var rin = new RegistersEx();

		rin.AX = (int)Interrupt0x21.Function.ReadFileOrDevice << 8;
		rin.BX = DOS.StandardInput;
		rin.CX = (ushort)testCharacters.Length;
		rin.DS = (ushort)(bufferAddress / MemoryManager.ParagraphSize);
		rin.DX = (ushort)(bufferAddress % MemoryManager.ParagraphSize);

		Registers? rout = null;

		// Act
		Action action = () => rout = sut.Execute(rin);

		action.ExecutionTime().Should().BeLessThan(TimeSpan.FromSeconds(0.25));

		// Assert
		rout.Should().NotBeNull();

		rout.FLAGS.Should().NotHaveFlag(Flags.Carry);

		int actualBytes = rout.AX;

		actualBytes.Should().Be(testCharacters.Length);

		bufferSpan.Slice(0, actualBytes).ShouldMatch(testData);
	}

	[Test]
	public void ReadFileOrDevice_should_stop_reading_data_from_console_when_enter_is_received()
	{
		// Arrange
		var machine = new Machine();

		SDL.Keymod modState = 0;

		machine.Keyboard.GetModStateTestHook += () => modState;

		string testCharacters = Guid.NewGuid().ToString();

		string testInput = testCharacters + '\r';
		string expectedTestCharacters = testCharacters + "\r\n";

		string extraTestCharacters = Guid.NewGuid().ToString();

		byte[] expectedTestData = s_cp437.GetBytes(expectedTestCharacters);

		testInput += extraTestCharacters;

		SimulateTyping(testInput, ControlCharacterHandling.SemanticKey, machine);

		int bufferAddress = machine.DOS.MemoryManager.AllocateMemory(testInput.Length, machine.DOS.CurrentPSPSegment);

		var bufferSpan = machine.SystemMemory.AsSpan().Slice(bufferAddress, testInput.Length);

		var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

		var rin = new RegistersEx();

		rin.AX = (int)Interrupt0x21.Function.ReadFileOrDevice << 8;
		rin.BX = DOS.StandardInput;
		rin.CX = (ushort)testInput.Length;
		rin.DS = (ushort)(bufferAddress / MemoryManager.ParagraphSize);
		rin.DX = (ushort)(bufferAddress % MemoryManager.ParagraphSize);

		Registers? rout = null;

		// Act
		Action action = () => rout = sut.Execute(rin);

		action.ExecutionTime().Should().BeLessThan(TimeSpan.FromSeconds(0.25));

		// Assert
		rout.Should().NotBeNull();

		rout.FLAGS.Should().NotHaveFlag(Flags.Carry);

		int actualBytes = rout.AX;

		actualBytes.Should().Be(expectedTestCharacters.Length);

		bufferSpan.Slice(0, actualBytes).ShouldMatch(expectedTestData);
	}

	[Test]
	public void ReadFileOrDevice_should_ignore_newline_characters_preceding_enter_when_reading_from_console()
	{
		// Arrange
		var machine = new Machine();

		SDL.Keymod modState = 0;

		machine.Keyboard.GetModStateTestHook += () => modState;

		string testCharacters = Guid.NewGuid().ToString();

		string testInput = testCharacters + "\n\n\n\n\r";
		string expectedTestCharacters = testCharacters + "\r\n";

		string extraTestCharacters = Guid.NewGuid().ToString();

		byte[] expectedTestData = s_cp437.GetBytes(expectedTestCharacters);

		testInput += extraTestCharacters;

		SimulateTyping(testInput, ControlCharacterHandling.CtrlLetter, machine);

		int bufferAddress = machine.DOS.MemoryManager.AllocateMemory(testInput.Length, machine.DOS.CurrentPSPSegment);

		var bufferSpan = machine.SystemMemory.AsSpan().Slice(bufferAddress, testInput.Length);

		var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

		var rin = new RegistersEx();

		rin.AX = (int)Interrupt0x21.Function.ReadFileOrDevice << 8;
		rin.BX = DOS.StandardInput;
		rin.CX = (ushort)testInput.Length;
		rin.DS = (ushort)(bufferAddress / MemoryManager.ParagraphSize);
		rin.DX = (ushort)(bufferAddress % MemoryManager.ParagraphSize);

		Registers? rout = null;

		// Act
		Action action = () => rout = sut.Execute(rin);

		action.ExecutionTime().Should().BeLessThan(TimeSpan.FromSeconds(0.25));

		// Assert
		rout.Should().NotBeNull();

		rout.FLAGS.Should().NotHaveFlag(Flags.Carry);

		int actualBytes = rout.AX;

		actualBytes.Should().Be(expectedTestCharacters.Length);

		bufferSpan.Slice(0, actualBytes).ShouldMatch(expectedTestData);
	}

	[Test]
	public void ReadFileOrDevice_should_not_read_data_from_console_without_enter_keypress()
	{
		// Arrange
		var machine = new Machine();

		SDL.Keymod modState = 0;

		machine.Keyboard.GetModStateTestHook += () => modState;

		string testCharacters = Guid.NewGuid().ToString();
		byte[] testData = s_cp437.GetBytes(testCharacters);

		SimulateTyping(testCharacters, ControlCharacterHandling.SemanticKey, machine);

		int bufferAddress = machine.DOS.MemoryManager.AllocateMemory(testCharacters.Length, machine.DOS.CurrentPSPSegment);

		var bufferSpan = machine.SystemMemory.AsSpan().Slice(bufferAddress, testCharacters.Length);

		var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

		var rin = new RegistersEx();

		rin.AX = (int)Interrupt0x21.Function.ReadFileOrDevice << 8;
		rin.BX = DOS.StandardInput;
		rin.CX = (ushort)testCharacters.Length;
		rin.DS = (ushort)(bufferAddress / MemoryManager.ParagraphSize);
		rin.DX = (ushort)(bufferAddress % MemoryManager.ParagraphSize);

		Registers? rout = null;

		// Act & Assert
		Action action = () => rout = sut.Execute(rin);

		action.ExecutionTime().Should().BeGreaterThan(TimeSpan.FromSeconds(0.5));
	}

	[Test, NonParallelizable]
	public void WriteFileOrDevice_should_write_data_to_files()
	{
		// Arrange
		using (var workspace = new TemporaryDirectory())
		{
			Environment.CurrentDirectory = workspace.Path;

			var machine = new Machine();

			machine.DOS.SetUpRunningProgramSegmentPrefix("");

			const string TestFileName = "TESTFILE.TXT";

			byte[] testData = s_cp437.GetBytes(Guid.NewGuid().ToString());

			int fileHandle = machine.DOS.OpenFile(
				TestFileName,
				QBX.OperatingSystem.FileStructures.FileMode.CreateNew,
				OpenMode.Access_ReadWrite);

			int bufferAddress = machine.DOS.MemoryManager.AllocateMemory(testData.Length, machine.DOS.CurrentPSPSegment);

			var bufferSpan = machine.SystemMemory.AsSpan().Slice(bufferAddress, testData.Length);

			testData.CopyTo(bufferSpan);

			try
			{
				var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

				var rin = new RegistersEx();

				rin.AX = (int)Interrupt0x21.Function.WriteFileOrDevice << 8;
				rin.BX = (ushort)fileHandle;
				rin.CX = (ushort)testData.Length;
				rin.DS = (ushort)(bufferAddress / MemoryManager.ParagraphSize);
				rin.DX = (ushort)(bufferAddress % MemoryManager.ParagraphSize);

				// Act
				var rout = sut.Execute(rin);

				// Assert
				rout.FLAGS.Should().NotHaveFlag(Flags.Carry);

				int actualBytes = rout.AX;

				actualBytes.Should().Be(testData.Length);

				bufferSpan.Slice(0, actualBytes).ShouldMatch(testData);
			}
			finally
			{
				machine.DOS.CloseFile(fileHandle);
			}
		}
	}

	[Test]
	public void WriteFileOrDevice_should_write_data_to_console()
	{
		// Arrange
		var machine = new Machine();

		var captureBuffer = new StringValue();

		var capture = new CapturingTextLibrary(machine, captureBuffer);

		machine.VideoFirmware.SetTestingVisualLibrary(capture);

		byte[] testData = s_cp437.GetBytes(Guid.NewGuid().ToString());

		int bufferAddress = machine.DOS.MemoryManager.AllocateMemory(testData.Length, machine.DOS.CurrentPSPSegment);

		var bufferSpan = machine.SystemMemory.AsSpan().Slice(bufferAddress, testData.Length);

		testData.CopyTo(bufferSpan);

		var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

		var rin = new RegistersEx();

		rin.AX = (int)Interrupt0x21.Function.WriteFileOrDevice << 8;
		rin.BX = DOS.StandardOutput;
		rin.CX = (ushort)testData.Length;
		rin.DS = (ushort)(bufferAddress / MemoryManager.ParagraphSize);
		rin.DX = (ushort)(bufferAddress % MemoryManager.ParagraphSize);

		// Act
		var rout = sut.Execute(rin);

		// Assert
		rout.FLAGS.Should().NotHaveFlag(Flags.Carry);

		int actualBytes = rout.AX;

		actualBytes.Should().Be(testData.Length);

		captureBuffer.AsSpan().ShouldMatch(testData);
	}

	[Test, NonParallelizable]
	public void DeleteFile_should_delete_files()
	{
		// Arrange
		using (var workspace = new TemporaryDirectory())
		{
			Environment.CurrentDirectory = workspace.Path;

			var machine = new Machine();

			machine.DOS.SetUpRunningProgramSegmentPrefix("");

			const string TestFileName = "TESTFILE.TXT";

			File.WriteAllText(TestFileName, "QuickBASIC");

			byte[] fileNameBytes = s_cp437.GetBytes(TestFileName);

			int fileNameBufferSize = fileNameBytes.Length + 1;

			var fileNameAddress = machine.DOS.MemoryManager.AllocateMemory(fileNameBufferSize, machine.DOS.CurrentPSPSegment);

			var fileNameSpan = machine.SystemMemory.AsSpan().Slice(fileNameAddress, fileNameBufferSize);

			fileNameBytes.CopyTo(fileNameSpan);
			fileNameSpan[fileNameBytes.Length] = 0;

			var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

			var rin = new RegistersEx();

			rin.AX = (int)Interrupt0x21.Function.DeleteFile << 8;
			rin.DS = (ushort)(fileNameAddress / MemoryManager.ParagraphSize);
			rin.DX = (ushort)(fileNameAddress % MemoryManager.ParagraphSize);

			// Act
			bool existsBefore = File.Exists(TestFileName);

			var rout = sut.Execute(rin);

			bool existsAfter = File.Exists(TestFileName);

			// Assert
			rout.FLAGS.Should().NotHaveFlag(Flags.Carry);

			existsBefore.Should().BeTrue();
			existsAfter.Should().BeFalse();
		}
	}

	[Test, NonParallelizable]
	public void DeleteFile_should_delete_files_with_relative_paths()
	{
		// Arrange
		using (var workspace = new TemporaryDirectory())
		{
			Environment.CurrentDirectory = workspace.Path;

			var machine = new Machine();

			machine.DOS.SetUpRunningProgramSegmentPrefix("");

			const string TestFileName = "A/TESTFILE.TXT";

			Directory.CreateDirectory("A");

			File.WriteAllText(TestFileName, "QuickBASIC");

			byte[] fileNameBytes = s_cp437.GetBytes(TestFileName);

			int fileNameBufferSize = fileNameBytes.Length + 1;

			var fileNameAddress = machine.DOS.MemoryManager.AllocateMemory(fileNameBufferSize, machine.DOS.CurrentPSPSegment);

			var fileNameSpan = machine.SystemMemory.AsSpan().Slice(fileNameAddress, fileNameBufferSize);

			fileNameBytes.CopyTo(fileNameSpan);
			fileNameSpan[fileNameBytes.Length] = 0;

			var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

			var rin = new RegistersEx();

			rin.AX = (int)Interrupt0x21.Function.DeleteFile << 8;
			rin.DS = (ushort)(fileNameAddress / MemoryManager.ParagraphSize);
			rin.DX = (ushort)(fileNameAddress % MemoryManager.ParagraphSize);

			// Act
			bool existsBefore = File.Exists(TestFileName);

			var rout = sut.Execute(rin);

			bool existsAfter = File.Exists(TestFileName);

			// Assert
			rout.FLAGS.Should().NotHaveFlag(Flags.Carry);

			existsBefore.Should().BeTrue();
			existsAfter.Should().BeFalse();
		}
	}

	[Test, NonParallelizable]
	public void DeleteFile_should_delete_files_with_relative_paths_including_parent_references()
	{
		// Arrange
		using (var workspace = new TemporaryDirectory())
		{
			Environment.CurrentDirectory = workspace.Path;

			var machine = new Machine();

			machine.DOS.SetUpRunningProgramSegmentPrefix("");

			const string TestFileName = "../TESTFILE.TXT";

			Directory.CreateDirectory("A");
			Environment.CurrentDirectory = "A";

			File.WriteAllText(TestFileName, "QuickBASIC");

			byte[] fileNameBytes = s_cp437.GetBytes(TestFileName);

			int fileNameBufferSize = fileNameBytes.Length + 1;

			var fileNameAddress = machine.DOS.MemoryManager.AllocateMemory(fileNameBufferSize, machine.DOS.CurrentPSPSegment);

			var fileNameSpan = machine.SystemMemory.AsSpan().Slice(fileNameAddress, fileNameBufferSize);

			fileNameBytes.CopyTo(fileNameSpan);
			fileNameSpan[fileNameBytes.Length] = 0;

			var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

			var rin = new RegistersEx();

			rin.AX = (int)Interrupt0x21.Function.DeleteFile << 8;
			rin.DS = (ushort)(fileNameAddress / MemoryManager.ParagraphSize);
			rin.DX = (ushort)(fileNameAddress % MemoryManager.ParagraphSize);

			// Act
			bool existsBefore = File.Exists(TestFileName);

			var rout = sut.Execute(rin);

			bool existsAfter = File.Exists(TestFileName);

			// Assert
			rout.FLAGS.Should().NotHaveFlag(Flags.Carry);

			existsBefore.Should().BeTrue();
			existsAfter.Should().BeFalse();
		}
	}

	[Test, NonParallelizable]
	public void MoveFilePointer_should_move_pointer_to_a_byte_that_exists_in_the_file()
	{
		// Arrange
		using (var workspace = new TemporaryDirectory())
		{
			Environment.CurrentDirectory = workspace.Path;

			var machine = new Machine();

			machine.DOS.SetUpRunningProgramSegmentPrefix("");

			const string TestFileName = "TESTFILE.TXT";
			const int TestFileSize = 10240;
			const int TestFilePointer = 5120;

			byte[] testData = new byte[TestFileSize];

			TestContext.CurrentContext.Random.NextBytes(testData);

			File.WriteAllBytes(TestFileName, testData);

			int fileHandle = machine.DOS.OpenFile(
				TestFileName,
				QBX.OperatingSystem.FileStructures.FileMode.Open,
				OpenMode.Access_ReadWrite);

			fileHandle.Should().BeInRange(2, machine.DOS.Files.Count - 1);

			var regularFile = (RegularFileDescriptor)machine.DOS.Files[fileHandle]!;

			try
			{
				var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

				var rin = new RegistersEx();

				rin.AX = (int)Interrupt0x21.Function.MoveFilePointer << 8;
				rin.BX = (ushort)fileHandle;
				rin.CX = unchecked((ushort)(TestFilePointer >> 16));
				rin.DX = unchecked((ushort)TestFilePointer);

				rin.AX |= (int)MoveMethod.FromBeginning;

				// Act
				var rout = sut.Execute(rin);

				// Assert
				rout.FLAGS.Should().NotHaveFlag(Flags.Carry);

				regularFile.FilePointer.Should().Be(TestFilePointer);
			}
			finally
			{
				machine.DOS.CloseFile(fileHandle);
			}
		}
	}

	[Test, NonParallelizable]
	public void MoveFilePointer_should_move_pointer_with_specified_move_method([Values] MoveMethod moveMethod)
	{
		// Arrange
		using (var workspace = new TemporaryDirectory())
		{
			Environment.CurrentDirectory = workspace.Path;

			var machine = new Machine();

			machine.DOS.SetUpRunningProgramSegmentPrefix("");

			const string TestFileName = "TESTFILE.TXT";
			const int TestFileSize = 10240;
			const int InitialTestFilePointer = 5120;

			int relativeSeek = 150;

			int expectedFilePointerAfterSeek;

			switch (moveMethod)
			{
				case MoveMethod.FromBeginning: expectedFilePointerAfterSeek = relativeSeek; break;
				case MoveMethod.FromCurrent: expectedFilePointerAfterSeek = InitialTestFilePointer + relativeSeek; break;
				case MoveMethod.FromEnd:
					relativeSeek = -150;
					expectedFilePointerAfterSeek = TestFileSize + relativeSeek;
					break;

				default: throw new Exception("Internal error: Unrecognized moveMethod in test case data");
			}

			byte[] testData = new byte[TestFileSize];

			TestContext.CurrentContext.Random.NextBytes(testData);

			File.WriteAllBytes(TestFileName, testData);

			int fileHandle = machine.DOS.OpenFile(
				TestFileName,
				QBX.OperatingSystem.FileStructures.FileMode.Open,
				OpenMode.Access_ReadWrite);

			fileHandle.Should().BeInRange(2, machine.DOS.Files.Count - 1);

			var regularFile = (RegularFileDescriptor)machine.DOS.Files[fileHandle]!;

			regularFile.Seek(InitialTestFilePointer, MoveMethod.FromBeginning);

			try
			{
				var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

				var rin = new RegistersEx();

				rin.AX = (int)Interrupt0x21.Function.MoveFilePointer << 8;
				rin.BX = (ushort)fileHandle;
				rin.CX = unchecked((ushort)(relativeSeek >> 16));
				rin.DX = unchecked((ushort)relativeSeek);

				rin.AX |= (byte)moveMethod;

				// Act
				var rout = sut.Execute(rin);

				// Assert
				rout.FLAGS.Should().NotHaveFlag(Flags.Carry);

				regularFile.FilePointer.Should().Be(expectedFilePointerAfterSeek);
			}
			finally
			{
				machine.DOS.CloseFile(fileHandle);
			}
		}
	}

	[Test, NonParallelizable]
	public void MoveFilePointer_should_move_pointer_to_byte_past_end_of_file()
	{
		// Arrange
		using (var workspace = new TemporaryDirectory())
		{
			Environment.CurrentDirectory = workspace.Path;

			var machine = new Machine();

			machine.DOS.SetUpRunningProgramSegmentPrefix("");

			const string TestFileName = "TESTFILE.TXT";
			const int TestFileSize = 10240;
			const int TestFilePointer = TestFileSize * 10;

			byte[] testData = new byte[TestFileSize];

			TestContext.CurrentContext.Random.NextBytes(testData);

			File.WriteAllBytes(TestFileName, testData);

			int fileHandle = machine.DOS.OpenFile(
				TestFileName,
				QBX.OperatingSystem.FileStructures.FileMode.Open,
				OpenMode.Access_ReadWrite);

			fileHandle.Should().BeInRange(2, machine.DOS.Files.Count - 1);

			var regularFile = (RegularFileDescriptor)machine.DOS.Files[fileHandle]!;

			try
			{
				var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

				var rin = new RegistersEx();

				rin.AX = (int)Interrupt0x21.Function.MoveFilePointer << 8;
				rin.BX = (ushort)fileHandle;
				rin.CX = unchecked((ushort)(TestFilePointer >> 16));
				rin.DX = unchecked((ushort)TestFilePointer);

				rin.AX |= (int)MoveMethod.FromBeginning;

				// Act
				var rout = sut.Execute(rin);

				// Assert
				rout.FLAGS.Should().NotHaveFlag(Flags.Carry);

				regularFile.FilePointer.Should().Be(TestFilePointer);
			}
			finally
			{
				machine.DOS.CloseFile(fileHandle);
			}

			new FileInfo(TestFileName).Length.Should().Be(TestFileSize);
		}
	}

	[Test, NonParallelizable]
	public void MoveFilePointer_should_move_pointer_to_maximum_possible_file_pointer_value()
	{
		// Arrange
		using (var workspace = new TemporaryDirectory())
		{
			Environment.CurrentDirectory = workspace.Path;

			var machine = new Machine();

			machine.DOS.SetUpRunningProgramSegmentPrefix("");

			const string TestFileName = "TESTFILE.TXT";
			const int TestFileSize = 10240;
			const uint TestFilePointer = uint.MaxValue;

			byte[] testData = new byte[TestFileSize];

			TestContext.CurrentContext.Random.NextBytes(testData);

			File.WriteAllBytes(TestFileName, testData);

			int fileHandle = machine.DOS.OpenFile(
				TestFileName,
				QBX.OperatingSystem.FileStructures.FileMode.Open,
				OpenMode.Access_ReadWrite);

			fileHandle.Should().BeInRange(2, machine.DOS.Files.Count - 1);

			var regularFile = (RegularFileDescriptor)machine.DOS.Files[fileHandle]!;

			try
			{
				var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

				var rin = new RegistersEx();

				rin.AX = (int)Interrupt0x21.Function.MoveFilePointer << 8;
				rin.BX = (ushort)fileHandle;
				rin.CX = unchecked((ushort)(TestFilePointer >> 16));
				rin.DX = unchecked((ushort)TestFilePointer);

				rin.AX |= (int)MoveMethod.FromBeginning;

				// Act
				var rout = sut.Execute(rin);

				// Assert
				rout.FLAGS.Should().NotHaveFlag(Flags.Carry);

				regularFile.FilePointer.Should().Be(TestFilePointer);
			}
			finally
			{
				machine.DOS.CloseFile(fileHandle);
			}

			new FileInfo(TestFileName).Length.Should().Be(TestFileSize);
		}
	}

	[Test, NonParallelizable]
	public void MoveFilePointer_should_move_pointer_to_byte_before_start_of_file()
	{
		// This is documented as completing without error; an error occurs on any subsequent read or write operation.
		// Arrange
		using (var workspace = new TemporaryDirectory())
		{
			Environment.CurrentDirectory = workspace.Path;

			var machine = new Machine();

			machine.DOS.SetUpRunningProgramSegmentPrefix("");

			const string TestFileName = "TESTFILE.TXT";
			const int TestFileSize = 10240;
			const int TestFilePointer = -500;

			byte[] testData = new byte[TestFileSize];

			TestContext.CurrentContext.Random.NextBytes(testData);

			File.WriteAllBytes(TestFileName, testData);

			int fileHandle = machine.DOS.OpenFile(
				TestFileName,
				QBX.OperatingSystem.FileStructures.FileMode.Open,
				OpenMode.Access_ReadWrite);

			fileHandle.Should().BeInRange(2, machine.DOS.Files.Count - 1);

			var regularFile = (RegularFileDescriptor)machine.DOS.Files[fileHandle]!;

			try
			{
				var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

				var rin = new RegistersEx();

				rin.AX = (int)Interrupt0x21.Function.MoveFilePointer << 8;
				rin.BX = (ushort)fileHandle;
				rin.CX = unchecked((ushort)(TestFilePointer >> 16));
				rin.DX = unchecked((ushort)TestFilePointer);

				rin.AX |= (int)MoveMethod.FromCurrent;

				// Act
				var rout = sut.Execute(rin);

				// Assert
				rout.FLAGS.Should().NotHaveFlag(Flags.Carry);

				regularFile.FilePointer.Should().Be(TestFilePointer);
			}
			finally
			{
				machine.DOS.CloseFile(fileHandle);
			}

			new FileInfo(TestFileName).Length.Should().Be(TestFileSize);
		}
	}

	[Test, NonParallelizable]
	public void GetFileAttributes_should_return_file_attributes()
	{
		// Arrange
		using (var workspace = new TemporaryDirectory())
		{
			Environment.CurrentDirectory = workspace.Path;

			var machine = new Machine();

			machine.DOS.SetUpRunningProgramSegmentPrefix("");

			const string TestFileName = "TESTFILE.TXT";

			File.WriteAllText(TestFileName, "QuickBASIC");

			File.SetAttributes(TestFileName, System.IO.FileAttributes.ReadOnly);

			byte[] fileNameBytes = s_cp437.GetBytes(TestFileName);

			int fileNameBufferSize = fileNameBytes.Length + 1;

			var fileNameAddress = machine.DOS.MemoryManager.AllocateMemory(fileNameBufferSize, machine.DOS.CurrentPSPSegment);

			var fileNameSpan = machine.SystemMemory.AsSpan().Slice(fileNameAddress, fileNameBufferSize);

			fileNameBytes.CopyTo(fileNameSpan);
			fileNameSpan[fileNameBytes.Length] = 0;

			var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

			var rin = new RegistersEx();

			rin.AX = (int)Interrupt0x21.Function.Function43 << 8;
			rin.AX |= (int)Interrupt0x21.Function43.GetFileAttributes;
			rin.DS = (ushort)(fileNameAddress / MemoryManager.ParagraphSize);
			rin.DX = (ushort)(fileNameAddress % MemoryManager.ParagraphSize);

			// Act
			var rout = sut.Execute(rin);

			// Assert
			rout.FLAGS.Should().NotHaveFlag(Flags.Carry);

			var attributes = (QBX.OperatingSystem.FileStructures.FileAttributes)rout.CX;

			attributes.Should().HaveFlag(QBX.OperatingSystem.FileStructures.FileAttributes.ReadOnly);
		}
	}

	[Test, NonParallelizable]
	public void GetFileAttributes_should_return_file_attributes_with_relative_paths()
	{
		// Arrange
		using (var workspace = new TemporaryDirectory())
		{
			Environment.CurrentDirectory = workspace.Path;

			var machine = new Machine();

			machine.DOS.SetUpRunningProgramSegmentPrefix("");

			const string TestFileName = "A/TESTFILE.TXT";

			Directory.CreateDirectory("A");

			File.WriteAllText(TestFileName, "QuickBASIC");

			File.SetAttributes(TestFileName, System.IO.FileAttributes.ReadOnly);

			byte[] fileNameBytes = s_cp437.GetBytes(TestFileName);

			int fileNameBufferSize = fileNameBytes.Length + 1;

			var fileNameAddress = machine.DOS.MemoryManager.AllocateMemory(fileNameBufferSize, machine.DOS.CurrentPSPSegment);

			var fileNameSpan = machine.SystemMemory.AsSpan().Slice(fileNameAddress, fileNameBufferSize);

			fileNameBytes.CopyTo(fileNameSpan);
			fileNameSpan[fileNameBytes.Length] = 0;

			var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

			var rin = new RegistersEx();

			rin.AX = (int)Interrupt0x21.Function.Function43 << 8;
			rin.AX |= (int)Interrupt0x21.Function43.GetFileAttributes;
			rin.DS = (ushort)(fileNameAddress / MemoryManager.ParagraphSize);
			rin.DX = (ushort)(fileNameAddress % MemoryManager.ParagraphSize);

			// Act
			var rout = sut.Execute(rin);

			// Assert
			rout.FLAGS.Should().NotHaveFlag(Flags.Carry);

			var attributes = (QBX.OperatingSystem.FileStructures.FileAttributes)rout.CX;

			attributes.Should().HaveFlag(QBX.OperatingSystem.FileStructures.FileAttributes.ReadOnly);
		}
	}

	[Test, NonParallelizable]
	public void GetFileAttributes_should_return_file_attributes_with_relative_paths_including_parent_references()
	{
		// Arrange
		using (var workspace = new TemporaryDirectory())
		{
			Environment.CurrentDirectory = workspace.Path;

			var machine = new Machine();

			machine.DOS.SetUpRunningProgramSegmentPrefix("");

			const string TestFileName = "../TESTFILE.TXT";

			Directory.CreateDirectory("A");

			Environment.CurrentDirectory = "A";

			File.WriteAllText(TestFileName, "QuickBASIC");

			File.SetAttributes(TestFileName, System.IO.FileAttributes.ReadOnly);

			byte[] fileNameBytes = s_cp437.GetBytes(TestFileName);

			int fileNameBufferSize = fileNameBytes.Length + 1;

			var fileNameAddress = machine.DOS.MemoryManager.AllocateMemory(fileNameBufferSize, machine.DOS.CurrentPSPSegment);

			var fileNameSpan = machine.SystemMemory.AsSpan().Slice(fileNameAddress, fileNameBufferSize);

			fileNameBytes.CopyTo(fileNameSpan);
			fileNameSpan[fileNameBytes.Length] = 0;

			var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

			var rin = new RegistersEx();

			rin.AX = (int)Interrupt0x21.Function.Function43 << 8;
			rin.AX |= (int)Interrupt0x21.Function43.GetFileAttributes;
			rin.DS = (ushort)(fileNameAddress / MemoryManager.ParagraphSize);
			rin.DX = (ushort)(fileNameAddress % MemoryManager.ParagraphSize);

			// Act
			var rout = sut.Execute(rin);

			// Assert
			rout.FLAGS.Should().NotHaveFlag(Flags.Carry);

			var attributes = (QBX.OperatingSystem.FileStructures.FileAttributes)rout.CX;

			attributes.Should().HaveFlag(QBX.OperatingSystem.FileStructures.FileAttributes.ReadOnly);
		}
	}

	[Test, NonParallelizable]
	public void SetFileAttributes_should_update_file_attributes()
	{
		// Arrange
		using (var workspace = new TemporaryDirectory())
		{
			Environment.CurrentDirectory = workspace.Path;

			var machine = new Machine();

			machine.DOS.SetUpRunningProgramSegmentPrefix("");

			const string TestFileName = "TESTFILE.TXT";

			File.WriteAllText(TestFileName, "QuickBASIC");

			byte[] fileNameBytes = s_cp437.GetBytes(TestFileName);

			int fileNameBufferSize = fileNameBytes.Length + 1;

			var fileNameAddress = machine.DOS.MemoryManager.AllocateMemory(fileNameBufferSize, machine.DOS.CurrentPSPSegment);

			var fileNameSpan = machine.SystemMemory.AsSpan().Slice(fileNameAddress, fileNameBufferSize);

			fileNameBytes.CopyTo(fileNameSpan);
			fileNameSpan[fileNameBytes.Length] = 0;
			var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

			var rin = new RegistersEx();

			rin.AX = (int)Interrupt0x21.Function.Function43 << 8;
			rin.AX |= (int)Interrupt0x21.Function43.SetFileAttributes;
			rin.DS = (ushort)(fileNameAddress / MemoryManager.ParagraphSize);
			rin.CX = (ushort)QBX.OperatingSystem.FileStructures.FileAttributes.ReadOnly;
			rin.DX = (ushort)(fileNameAddress % MemoryManager.ParagraphSize);

			// Act
			var rout = sut.Execute(rin);

			// Assert
			rout.FLAGS.Should().NotHaveFlag(Flags.Carry);

			File.GetAttributes(TestFileName).Should().HaveFlag(System.IO.FileAttributes.ReadOnly);
		}
	}

	[Test, NonParallelizable]
	public void SetFileAttributes_should_update_file_attributes_with_relative_paths()
	{
		// Arrange
		using (var workspace = new TemporaryDirectory())
		{
			Environment.CurrentDirectory = workspace.Path;

			var machine = new Machine();

			machine.DOS.SetUpRunningProgramSegmentPrefix("");

			const string TestFileName = "A/TESTFILE.TXT";

			Directory.CreateDirectory("A");

			File.WriteAllText(TestFileName, "QuickBASIC");

			byte[] fileNameBytes = s_cp437.GetBytes(TestFileName);

			int fileNameBufferSize = fileNameBytes.Length + 1;

			var fileNameAddress = machine.DOS.MemoryManager.AllocateMemory(fileNameBufferSize, machine.DOS.CurrentPSPSegment);

			var fileNameSpan = machine.SystemMemory.AsSpan().Slice(fileNameAddress, fileNameBufferSize);

			fileNameBytes.CopyTo(fileNameSpan);
			fileNameSpan[fileNameBytes.Length] = 0;

			var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

			var rin = new RegistersEx();

			rin.AX = (int)Interrupt0x21.Function.Function43 << 8;
			rin.AX |= (int)Interrupt0x21.Function43.SetFileAttributes;
			rin.CX = (ushort)QBX.OperatingSystem.FileStructures.FileAttributes.ReadOnly;
			rin.DS = (ushort)(fileNameAddress / MemoryManager.ParagraphSize);
			rin.DX = (ushort)(fileNameAddress % MemoryManager.ParagraphSize);

			// Act
			var rout = sut.Execute(rin);

			// Assert
			rout.FLAGS.Should().NotHaveFlag(Flags.Carry);

			File.GetAttributes(TestFileName).Should().HaveFlag(System.IO.FileAttributes.ReadOnly);
		}
	}

	[Test, NonParallelizable]
	public void SetFileAttributes_should_update_file_attributes_with_relative_paths_including_parent_references()
	{
		// Arrange
		using (var workspace = new TemporaryDirectory())
		{
			Environment.CurrentDirectory = workspace.Path;

			var machine = new Machine();

			machine.DOS.SetUpRunningProgramSegmentPrefix("");

			const string TestFileName = "../TESTFILE.TXT";

			Directory.CreateDirectory("A");

			Environment.CurrentDirectory = "A";

			File.WriteAllText(TestFileName, "QuickBASIC");

			byte[] fileNameBytes = s_cp437.GetBytes(TestFileName);

			int fileNameBufferSize = fileNameBytes.Length + 1;

			var fileNameAddress = machine.DOS.MemoryManager.AllocateMemory(fileNameBufferSize, machine.DOS.CurrentPSPSegment);

			var fileNameSpan = machine.SystemMemory.AsSpan().Slice(fileNameAddress, fileNameBufferSize);

			fileNameBytes.CopyTo(fileNameSpan);
			fileNameSpan[fileNameBytes.Length] = 0;

			var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

			var rin = new RegistersEx();

			rin.AX = (int)Interrupt0x21.Function.Function43 << 8;
			rin.AX |= (int)Interrupt0x21.Function43.SetFileAttributes;
			rin.CX = (ushort)QBX.OperatingSystem.FileStructures.FileAttributes.ReadOnly;
			rin.DS = (ushort)(fileNameAddress / MemoryManager.ParagraphSize);
			rin.DX = (ushort)(fileNameAddress % MemoryManager.ParagraphSize);

			// Act
			var rout = sut.Execute(rin);

			// Assert
			rout.FLAGS.Should().NotHaveFlag(Flags.Carry);

			File.GetAttributes(TestFileName).Should().HaveFlag(System.IO.FileAttributes.ReadOnly);
		}
	}

	[Test, NonParallelizable]
	public void ExtendedLengthFileNameOperations_should_permit_CreateDirectory_with_a_long_input_path()
	{
		// Arrange
		using (var workspace = new TemporaryDirectory())
		{
			Environment.CurrentDirectory = workspace.Path;

			var machine = new Machine();

			machine.DOS.SetUpRunningProgramSegmentPrefix("");

			// This baseline string is 77 characters long, onto which TestDirectoryName will be appended.
			// The ExtendedLengthFileNameOperations function is documented as permitting path names up to
			// 128 characters long, while the base operation has a limit of 67 characters.
			const string ExistingDirectoryName = "TESTDIR1.DIR/TESTDIR2.DIR/TESTDIR3.DIR/TESTDIR4.DIR/TESTDIR5.DIR/TESTDIR6.DIR";

			const string TestDirectoryName = "TESTDIR.FOO";

			Directory.CreateDirectory(ExistingDirectoryName);

			var testDirectoryPath = Path.Combine(ExistingDirectoryName, TestDirectoryName);

			byte[] directoryNameBytes = s_cp437.GetBytes(testDirectoryPath);

			int directoryNameBufferSize = directoryNameBytes.Length + 1;

			var directoryNameAddress = machine.DOS.MemoryManager.AllocateMemory(directoryNameBufferSize, machine.DOS.CurrentPSPSegment);

			var directoryNameSpan = machine.SystemMemory.AsSpan().Slice(directoryNameAddress, directoryNameBufferSize);

			directoryNameBytes.CopyTo(directoryNameSpan);
			directoryNameSpan[directoryNameBytes.Length] = 0;

			var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

			var rin = new RegistersEx();

			rin.AX = (int)Interrupt0x21.Function.Function43 << 8;
			rin.AX |= (int)Interrupt0x21.Function43.ExtendedLengthFileNameOperations;
			rin.BP = 0x5053;
			rin.CX = (int)Interrupt0x21.Function.CreateDirectory;
			rin.DS = (ushort)(directoryNameAddress / MemoryManager.ParagraphSize);
			rin.DX = (ushort)(directoryNameAddress % MemoryManager.ParagraphSize);

			// Act
			bool existsBefore = Directory.Exists(testDirectoryPath);

			var rout = sut.Execute(rin);

			bool existsAfter = Directory.Exists(testDirectoryPath);

			// Assert
			int al = rout.AX & 0xFF;

			al.Should().Be(0);

			existsBefore.Should().BeFalse();
			existsAfter.Should().BeTrue();
		}
	}

	[Test, NonParallelizable]
	public void ExtendedLengthFileNameOperations_should_permit_RenameFile_with_a_long_input_path()
	{
		// Arrange
		using (var workspace = new TemporaryDirectory())
		{
			Environment.CurrentDirectory = workspace.Path;

			var machine = new Machine();

			machine.DOS.SetUpRunningProgramSegmentPrefix("");

			// This baseline string is 77 characters long, onto which TestDirectoryName will be appended.
			// The ExtendedLengthFileNameOperations function is documented as permitting path names up to
			// 128 characters long, while the base operation has a limit of 67 characters.
			const string ExistingDirectoryName = "TESTDIR1.DIR/TESTDIR2.DIR/TESTDIR3.DIR/TESTDIR4.DIR/TESTDIR5.DIR/TESTDIR6.DIR";

			Directory.CreateDirectory(ExistingDirectoryName);

			const string OldFileName = ExistingDirectoryName + "/TEST1.TXT";
			const string NewFileName = ExistingDirectoryName + "/TEST2.TXT";

			File.WriteAllText(OldFileName, "");

			byte[] oldFileNameBytes = s_cp437.GetBytes(OldFileName);
			byte[] newFileNameBytes = s_cp437.GetBytes(NewFileName);

			int oldFileNameAddress = machine.DOS.MemoryManager.AllocateMemory(oldFileNameBytes.Length + 1, machine.DOS.CurrentPSPSegment);
			int newFileNameAddress = machine.DOS.MemoryManager.AllocateMemory(newFileNameBytes.Length + 1, machine.DOS.CurrentPSPSegment);

			var oldFileNameSpan = machine.SystemMemory.AsSpan().Slice(oldFileNameAddress, oldFileNameBytes.Length + 1);
			var newFileNameSpan = machine.SystemMemory.AsSpan().Slice(newFileNameAddress, newFileNameBytes.Length + 1);

			oldFileNameBytes.CopyTo(oldFileNameSpan);
			oldFileNameSpan[oldFileNameBytes.Length] = 0;

			newFileNameBytes.CopyTo(newFileNameSpan);
			newFileNameSpan[newFileNameBytes.Length] = 0;

			var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

			var rin = new RegistersEx();

			rin.AX = (int)Interrupt0x21.Function.RenameFile << 8;
			rin.DS = (ushort)(oldFileNameAddress / MemoryManager.ParagraphSize);
			rin.DX = (ushort)(oldFileNameAddress % MemoryManager.ParagraphSize);
			rin.ES = (ushort)(newFileNameAddress / MemoryManager.ParagraphSize);
			rin.DI = (ushort)(newFileNameAddress % MemoryManager.ParagraphSize);

			// Act
			bool oldExistsBefore = File.Exists(OldFileName);
			bool newExistsBefore = File.Exists(NewFileName);

			var rout = sut.Execute(rin);

			bool oldExistsAfter = File.Exists(OldFileName);
			bool newExistsAfter = File.Exists(NewFileName);

			// Assert
			rout.FLAGS.Should().NotHaveFlag(Flags.Carry);

			oldExistsBefore.Should().BeTrue();
			newExistsBefore.Should().BeFalse();

			oldExistsAfter.Should().BeFalse();
			newExistsAfter.Should().BeTrue();
		}
	}

	[Test, NonParallelizable]
	public void GetDeviceData_should_identify_regular_files([Values] bool expectedPristine)
	{
		// Arrange
		using (var workspace = new TemporaryDirectory())
		{
			Environment.CurrentDirectory = workspace.Path;

			var machine = new Machine();

			machine.DOS.SetUpRunningProgramSegmentPrefix("");

			const string TestFileName = "TESTFILE.TXT";

			File.WriteAllText(TestFileName, "DOS 6.22");

			int fileHandle = machine.DOS.OpenFile("TESTFILE.TXT", QBX.OperatingSystem.FileStructures.FileMode.Open, OpenMode.Access_ReadWrite);

			if (!expectedPristine)
				machine.DOS.WriteByte(fileHandle, (byte)'Q', out _);

			int expectedDriveNumber = machine.DOS.GetDefaultDrive();

			try
			{
				var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

				var rin = new RegistersEx();

				rin.AX = (int)Interrupt0x21.Function.Function44 << 8;
				rin.AX |= (int)Interrupt0x21.Function44.GetDeviceData;
				rin.BX = (ushort)fileHandle;

				// Act
				var rout = sut.Execute(rin);

				// Assert
				rout.FLAGS.Should().NotHaveFlag(Flags.Carry);

				int dl = rout.DX & 0xFF;

				bool isDevice = (dl & 0x80) != 0;
				bool isPristine = (dl & 0x40) != 0;
				int driveNumber = dl & 0x3F;

				isDevice.Should().BeFalse();
				isPristine.Should().Be(expectedPristine);
				driveNumber.Should().Be(expectedDriveNumber);
			}
			finally
			{
				machine.DOS.CloseFile(fileHandle);
			}
		}
	}

	[Test]
	public void GetDeviceData_should_identify_console_device_from_standard_handles([Values(DOS.StandardInput, DOS.StandardOutput)] int fileHandle)
	{
		// Arrange
		var machine = new Machine();

		machine.DOS.SetUpRunningProgramSegmentPrefix("");

		var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

		var rin = new RegistersEx();

		rin.AX = (int)Interrupt0x21.Function.Function44 << 8;
		rin.AX |= (int)Interrupt0x21.Function44.GetDeviceData;
		rin.BX = (ushort)fileHandle;

		// Act
		var rout = sut.Execute(rin);

		// Assert
		rout.FLAGS.Should().NotHaveFlag(Flags.Carry);

		int dl = rout.DX & 0xFF;

		bool isDevice = (dl & 0x80) != 0;

		bool isConsoleInput = (dl & 1) != 0;
		bool isConsoleOutput = (dl & 2) != 0;
		bool isNull = (dl & 4) != 0;
		bool isClock = (dl & 8) != 0;
		bool isSpecialDevice = (dl & 0x10) != 0;

		var ioMode = (dl & 0x20) != 0 ? IOMode.Binary : IOMode.ASCII;
		bool isAtEOF = (dl & 0x40) == 0; // NB: checking for _not_ set

		isDevice.Should().BeTrue();

		if (fileHandle == DOS.StandardInput)
			isConsoleInput.Should().BeTrue();
		if (fileHandle == DOS.StandardOutput)
			isConsoleOutput.Should().BeTrue();
		isNull.Should().BeFalse();
		isClock.Should().BeFalse();
		isSpecialDevice.Should().BeFalse();

		ioMode.Should().Be(machine.DOS.Devices.Console.IOMode);
		isAtEOF.Should().Be(machine.DOS.Devices.Console.AtSoftEOF);
	}

	[Test]
	public void GetDeviceData_should_identify_console_device([Values("CON", "CON.OUT")] string deviceFileName)
	{
		// Arrange
		var machine = new Machine();

		machine.DOS.SetUpRunningProgramSegmentPrefix("");

		int fileHandle = machine.DOS.OpenFile(deviceFileName, QBX.OperatingSystem.FileStructures.FileMode.Open, OpenMode.Access_ReadWrite);

		var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

		var rin = new RegistersEx();

		rin.AX = (int)Interrupt0x21.Function.Function44 << 8;
		rin.AX |= (int)Interrupt0x21.Function44.GetDeviceData;
		rin.BX = (ushort)fileHandle;

		// Act
		var rout = sut.Execute(rin);

		// Assert
		rout.FLAGS.Should().NotHaveFlag(Flags.Carry);

		int dl = rout.DX & 0xFF;

		bool isDevice = (dl & 0x80) != 0;

		bool isConsoleInput = (dl & 1) != 0;
		bool isConsoleOutput = (dl & 2) != 0;
		bool isNull = (dl & 4) != 0;
		bool isClock = (dl & 8) != 0;
		bool isSpecialDevice = (dl & 0x10) != 0;

		var ioMode = (dl & 0x20) != 0 ? IOMode.Binary : IOMode.ASCII;
		bool isAtEOF = (dl & 0x40) == 0; // NB: checking for _not_ set

		isDevice.Should().BeTrue();

		isConsoleInput.Should().BeTrue();
		isConsoleOutput.Should().BeTrue();
		isNull.Should().BeFalse();
		isClock.Should().BeFalse();
		isSpecialDevice.Should().BeFalse();

		ioMode.Should().Be(machine.DOS.Devices.Console.IOMode);
		isAtEOF.Should().Be(machine.DOS.Devices.Console.AtSoftEOF);
	}

	[Test]
	public void GetDeviceData_should_identify_null_device([Values("NUL", "NUL.OUT")] string deviceFileName)
	{
		// Arrange
		var machine = new Machine();

		machine.DOS.SetUpRunningProgramSegmentPrefix("");

		int fileHandle = machine.DOS.OpenFile(deviceFileName, QBX.OperatingSystem.FileStructures.FileMode.Open, OpenMode.Access_WriteOnly);

		var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

		var rin = new RegistersEx();

		rin.AX = (int)Interrupt0x21.Function.Function44 << 8;
		rin.AX |= (int)Interrupt0x21.Function44.GetDeviceData;
		rin.BX = (ushort)fileHandle;

		// Act
		var rout = sut.Execute(rin);

		// Assert
		rout.FLAGS.Should().NotHaveFlag(Flags.Carry);

		int dl = rout.DX & 0xFF;

		bool isDevice = (dl & 0x80) != 0;

		bool isConsoleInput = (dl & 1) != 0;
		bool isConsoleOutput = (dl & 2) != 0;
		bool isNull = (dl & 4) != 0;
		bool isClock = (dl & 8) != 0;
		bool isSpecialDevice = (dl & 0x10) != 0;

		var ioMode = (dl & 0x20) != 0 ? IOMode.Binary : IOMode.ASCII;
		bool isAtEOF = (dl & 0x40) == 0; // NB: checking for _not_ set

		isDevice.Should().BeTrue();

		isConsoleInput.Should().BeFalse();
		isConsoleOutput.Should().BeFalse();
		isNull.Should().BeTrue();
		isClock.Should().BeFalse();
		isSpecialDevice.Should().BeFalse();

		ioMode.Should().Be(machine.DOS.Devices.Null.IOMode);
		isAtEOF.Should().Be(machine.DOS.Devices.Null.AtSoftEOF);
	}

	[Test]
	public void GetDeviceData_should_identify_clock_device([Values("CLOCK$", "CLOCK$.OUT")] string deviceFileName)
	{
		// Arrange
		var machine = new Machine();

		machine.DOS.SetUpRunningProgramSegmentPrefix("");

		int fileHandle = machine.DOS.OpenFile(deviceFileName, QBX.OperatingSystem.FileStructures.FileMode.Open, OpenMode.Access_WriteOnly);

		var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

		var rin = new RegistersEx();

		rin.AX = (int)Interrupt0x21.Function.Function44 << 8;
		rin.AX |= (int)Interrupt0x21.Function44.GetDeviceData;
		rin.BX = (ushort)fileHandle;

		// Act
		var rout = sut.Execute(rin);

		// Assert
		rout.FLAGS.Should().NotHaveFlag(Flags.Carry);

		int dl = rout.DX & 0xFF;

		bool isDevice = (dl & 0x80) != 0;

		bool isConsoleInput = (dl & 1) != 0;
		bool isConsoleOutput = (dl & 2) != 0;
		bool isNull = (dl & 4) != 0;
		bool isClock = (dl & 8) != 0;
		bool isSpecialDevice = (dl & 0x10) != 0;

		var ioMode = (dl & 0x20) != 0 ? IOMode.Binary : IOMode.ASCII;
		bool isAtEOF = (dl & 0x40) == 0; // NB: checking for _not_ set

		isDevice.Should().BeTrue();

		isConsoleInput.Should().BeFalse();
		isConsoleOutput.Should().BeFalse();
		isNull.Should().BeFalse();
		isClock.Should().BeTrue();
		isSpecialDevice.Should().BeFalse();

		ioMode.Should().Be(machine.DOS.Devices.Null.IOMode);
		isAtEOF.Should().Be(machine.DOS.Devices.Null.AtSoftEOF);
	}

	[Test]
	public void GetDeviceData_should_identify_invalid_handle()
	{
		// Arrange
		var machine = new Machine();

		machine.DOS.SetUpRunningProgramSegmentPrefix("");

		int fileHandle = machine.DOS.Files.FindIndex(entry => entry == null);

		if (fileHandle < 0)
			fileHandle = machine.DOS.Files.Count;

		var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

		var rin = new RegistersEx();

		rin.AX = (int)Interrupt0x21.Function.Function44 << 8;
		rin.AX |= (int)Interrupt0x21.Function44.GetDeviceData;
		rin.BX = (ushort)fileHandle;

		// Act
		var rout = sut.Execute(rin);

		// Assert
		rout.FLAGS.Should().HaveFlag(Flags.Carry);
		rout.AX.Should().Be((ushort)DOSError.InvalidHandle);
	}

	[Test]
	public void SetDeviceData_should_alter_IO_mode()
	{
		// Arrange
		var machine = new Machine();

		machine.DOS.SetUpRunningProgramSegmentPrefix("");

		var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

		var rinSetup = new RegistersEx();

		rinSetup.AX = (int)Interrupt0x21.Function.Function44 << 8;
		rinSetup.AX |= (int)Interrupt0x21.Function44.GetDeviceData;
		rinSetup.BX = DOS.StandardInput;

		var routSetup = sut.Execute(rinSetup);

		ushort newDeviceStatus = routSetup.DX;

		newDeviceStatus ^= 32; // flip IO status bit

		var expectedIOMode = ((newDeviceStatus & 32) != 0) ? IOMode.Binary : IOMode.ASCII;

		var rin = new RegistersEx();

		rin.AX = (int)Interrupt0x21.Function.Function44 << 8;
		rin.AX |= (int)Interrupt0x21.Function44.SetDeviceData;
		rin.BX = DOS.StandardInput;
		rin.DX = newDeviceStatus;

		// Act
		var ioModeBefore = machine.DOS.Devices.Console.IOMode;

		var rout = sut.Execute(rin);

		var ioModeAfter = machine.DOS.Devices.Console.IOMode;

		// Assert
		rout.FLAGS.Should().NotHaveFlag(Flags.Carry);

		ioModeBefore.Should().NotBe(expectedIOMode);
		ioModeAfter.Should().Be(expectedIOMode);
	}

	[Test]
	public void SetDeviceData_should_alter_soft_EOF_flag()
	{
		// Arrange
		var machine = new Machine();

		machine.DOS.SetUpRunningProgramSegmentPrefix("");

		var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

		var rinSetup = new RegistersEx();

		rinSetup.AX = (int)Interrupt0x21.Function.Function44 << 8;
		rinSetup.AX |= (int)Interrupt0x21.Function44.GetDeviceData;
		rinSetup.BX = DOS.StandardInput;

		var routSetup = sut.Execute(rinSetup);

		ushort newDeviceStatus = routSetup.DX;

		newDeviceStatus ^= 64; // flip EOF status bit

		bool expectedAtEOF = ((newDeviceStatus & 64) == 0);

		var rin = new RegistersEx();

		rin.AX = (int)Interrupt0x21.Function.Function44 << 8;
		rin.AX |= (int)Interrupt0x21.Function44.SetDeviceData;
		rin.BX = DOS.StandardInput;
		rin.DX = newDeviceStatus;

		// Act
		var atEOFBefore = machine.DOS.Devices.Console.AtSoftEOF;

		var rout = sut.Execute(rin);

		var atEOFAfter = machine.DOS.Devices.Console.AtSoftEOF;

		// Assert
		rout.FLAGS.Should().NotHaveFlag(Flags.Carry);

		atEOFBefore.Should().NotBe(expectedAtEOF);
		atEOFAfter.Should().Be(expectedAtEOF);
	}

	[Test, NonParallelizable]
	public void SetDeviceData_should_detect_non_device_handles()
	{
		// Arrange
		using (var workspace = new TemporaryDirectory())
		{
			Environment.CurrentDirectory = workspace.Path;

			var machine = new Machine();

			machine.DOS.SetUpRunningProgramSegmentPrefix("");

			const string TestFileName = "TESTFILE.TXT";

			File.WriteAllText(TestFileName, "DOS 6.22");

			int fileHandle = machine.DOS.OpenFile("TESTFILE.TXT", QBX.OperatingSystem.FileStructures.FileMode.Open, OpenMode.Access_ReadWrite);

			try
			{
				var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

				var rin = new RegistersEx();

				rin.AX = (int)Interrupt0x21.Function.Function44 << 8;
				rin.AX |= (int)Interrupt0x21.Function44.SetDeviceData;
				rin.BX = (ushort)fileHandle;
				rin.DX = 0x80;

				// Act
				var rout = sut.Execute(rin);

				// Assert
				rout.FLAGS.Should().HaveFlag(Flags.Carry);
				rout.AX.Should().Be((ushort)DOSError.InvalidFunction);
			}
			finally
			{
				machine.DOS.CloseFile(fileHandle);
			}
		}
	}

	[Test, NonParallelizable]
	public void CheckDeviceInputStatus_should_detect_file_EOF([Values] bool testAtEOF)
	{
		// Arrange
		using (var workspace = new TemporaryDirectory())
		{
			Environment.CurrentDirectory = workspace.Path;

			var machine = new Machine();

			machine.DOS.SetUpRunningProgramSegmentPrefix("");

			const string TestFileName = "TESTFILE.TXT";

			File.WriteAllText(TestFileName, "DOS 6.22");

			int fileHandle = machine.DOS.OpenFile("TESTFILE.TXT", QBX.OperatingSystem.FileStructures.FileMode.Open, OpenMode.Access_ReadWrite);

			byte expectedInputStatusValue = 0xFF;

			if (testAtEOF)
			{
				machine.DOS.SeekFile(fileHandle, 0, MoveMethod.FromEnd);
				expectedInputStatusValue = 0;
			}

			try
			{
				var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

				var rin = new RegistersEx();

				rin.AX = (int)Interrupt0x21.Function.Function44 << 8;
				rin.AX |= (int)Interrupt0x21.Function44.CheckDeviceInputStatus;
				rin.BX = (ushort)fileHandle;

				// Act
				var rout = sut.Execute(rin);

				// Assert
				rout.FLAGS.Should().NotHaveFlag(Flags.Carry);

				byte al = unchecked((byte)rout.AX);

				al.Should().Be(expectedInputStatusValue);
			}
			finally
			{
				machine.DOS.CloseFile(fileHandle);
			}
		}
	}

	[Test]
	public void CheckDeviceInputStatus_should_detect_console_input([Values] bool testWithInput)
	{
		var machine = new Machine();

		machine.DOS.SetUpRunningProgramSegmentPrefix("");

		byte expectedInputStatusValue = 0;

		if (testWithInput)
		{
			SimulateTyping("a", ControlCharacterHandling.SemanticKey, machine);
			expectedInputStatusValue = 0xFF;
		}

		var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

		var rin = new RegistersEx();

		rin.AX = (int)Interrupt0x21.Function.Function44 << 8;
		rin.AX |= (int)Interrupt0x21.Function44.CheckDeviceInputStatus;
		rin.BX = (ushort)DOS.StandardInput;

		// Act
		var rout = sut.Execute(rin);

		// Assert
		rout.FLAGS.Should().NotHaveFlag(Flags.Carry);

		byte al = unchecked((byte)rout.AX);

		al.Should().Be(expectedInputStatusValue);
	}

	[NonParallelizable]
	[TestCase("TESTFILE.TXT", 0xFF)]
	[TestCase("CON", 0xFF)]
	[TestCase("NUL", 0xFF)]
	[TestCase("CLOCK$", 0x00)]
	public void CheckDeviceOutputStatus_should_return_ready_to_write_status(string deviceOrFileName, byte expectedOutputStatusValue)
	{
		// Arrange
		using (var workspace = new TemporaryDirectory())
		{
			Environment.CurrentDirectory = workspace.Path;

			var machine = new Machine();

			machine.DOS.SetUpRunningProgramSegmentPrefix("");

			const string TestFileName = "TESTFILE.TXT";

			File.WriteAllText(TestFileName, "DOS 6.22");

			int fileHandle = machine.DOS.OpenFile(deviceOrFileName, QBX.OperatingSystem.FileStructures.FileMode.Open, OpenMode.Access_ReadWrite);

			try
			{
				var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

				var rin = new RegistersEx();

				rin.AX = (int)Interrupt0x21.Function.Function44 << 8;
				rin.AX |= (int)Interrupt0x21.Function44.CheckDeviceOutputStatus;
				rin.BX = (ushort)fileHandle;

				// Act
				var rout = sut.Execute(rin);

				// Assert
				rout.FLAGS.Should().NotHaveFlag(Flags.Carry);

				byte al = unchecked((byte)rout.AX);

				al.Should().Be(expectedOutputStatusValue);
			}
			finally
			{
				machine.DOS.CloseFile(fileHandle);
			}
		}
	}

	[Test]
	public void DoesDeviceUseRemovableMedia_should_return_correct_value([Values] bool testWithRemovableMedia)
	{
		var mockDriveInfo = Substitute.For<IDriveInfo>();

		mockDriveInfo.Name.Returns("A:");
		mockDriveInfo.IsReady.Returns(true);
		mockDriveInfo.RootDirectoryPath.Returns(@"A:\");
		mockDriveInfo.DriveType.Returns(testWithRemovableMedia ? DriveType.Removable : DriveType.Fixed);
		mockDriveInfo.DriveFormat.Returns("FAT");
		mockDriveInfo.AvailableFreeSpace.Returns(1_457_664);
		mockDriveInfo.TotalFreeSpace.Returns(1_457_664);
		mockDriveInfo.TotalSize.Returns(1_457_664);
		mockDriveInfo.VolumeLabel.Returns("FOO");

		var mockDriveInfoProvider = Substitute.For<IDriveInfoProvider>();

		mockDriveInfoProvider.GetDrives().Returns(new IDriveInfo[] { mockDriveInfo });
		mockDriveInfoProvider.GetDrive(Arg.Any<string>()).Returns(
			callInfo =>
			{
				string path = callInfo.Arg<string>();

				if (path.Equals("A", StringComparison.OrdinalIgnoreCase)
				 || path.StartsWith("A:", StringComparison.OrdinalIgnoreCase))
					return mockDriveInfo;
				else
				{
					var dummyDriveInfo = Substitute.For<IDriveInfo>();

					dummyDriveInfo.Name.Returns(path);
					dummyDriveInfo.RootDirectoryPath.Returns(path);
					dummyDriveInfo.VolumeLabel.Returns(path);

					return dummyDriveInfo;
				}
			});

		var machine = new Machine(mockDriveInfoProvider);

		machine.DOS.SetUpRunningProgramSegmentPrefix("");

		var dpbAddress = machine.DOS.GetDefaultDriveParameterBlock();

		ref var dpb = ref DriveParameterBlock.CreateReference(machine.SystemMemory, dpbAddress.ToLinearAddress());

		ushort expectedFixedMediaFlagValue =
			testWithRemovableMedia
			? (ushort)0x0000
			: (ushort)0x0001;

		var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

		var rin = new RegistersEx();

		rin.AX = (int)Interrupt0x21.Function.Function44 << 8;
		rin.AX |= (int)Interrupt0x21.Function44.DoesDeviceUseRemovableMedia;
		rin.BX = dpb.DriveIdentifier;
		rin.BX++;

		// Act
		var rout = sut.Execute(rin);

		// Assert
		rout.FLAGS.Should().NotHaveFlag(Flags.Carry);

		rout.AX.Should().Be(expectedFixedMediaFlagValue);
	}

	[Test]
	public void IsDriveRemote_should_return_correct_value([Values] bool testWithRemoteDrive)
	{
		var mockDriveInfo = Substitute.For<IDriveInfo>();

		mockDriveInfo.Name.Returns("X:");
		mockDriveInfo.IsReady.Returns(true);
		mockDriveInfo.RootDirectoryPath.Returns(@"X:\");
		mockDriveInfo.DriveType.Returns(testWithRemoteDrive ? DriveType.Network : DriveType.Fixed);
		mockDriveInfo.DriveFormat.Returns("FAT");
		mockDriveInfo.AvailableFreeSpace.Returns(1024 * 1024 * 1024);
		mockDriveInfo.TotalFreeSpace.Returns(1024 * 1024 * 1024);
		mockDriveInfo.TotalSize.Returns(1024 * 1024 * 1024);
		mockDriveInfo.VolumeLabel.Returns("FOO");

		var mockDriveInfoProvider = Substitute.For<IDriveInfoProvider>();

		mockDriveInfoProvider.GetDrives().Returns(new IDriveInfo[] { mockDriveInfo });
		mockDriveInfoProvider.GetDrive(Arg.Any<string>()).Returns(mockDriveInfo);

		var machine = new Machine(mockDriveInfoProvider);

		machine.DOS.SetUpRunningProgramSegmentPrefix("");

		ushort expectedRemoteDriveBitValue =
			testWithRemoteDrive
			? (ushort)(1 << 12)
			: (ushort)0;

		var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

		var rin = new RegistersEx();

		rin.AX = (int)Interrupt0x21.Function.Function44 << 8;
		rin.AX |= (int)Interrupt0x21.Function44.IsDriveRemote;
		rin.BX = 1; // "A:\"

		// Act
		var rout = sut.Execute(rin);

		// Assert
		rout.FLAGS.Should().NotHaveFlag(Flags.Carry);

		ushort remoteDriveBit = rout.DX;

		remoteDriveBit &= expectedRemoteDriveBitValue;

		remoteDriveBit.Should().Be(expectedRemoteDriveBitValue);
	}

	[Test, NonParallelizable]
	public void IsFileOrDeviceRemote_should_return_correct_value([Values] bool testWithRemoteDrive)
	{
		// Arrange
		var mockDriveInfo = Substitute.For<IDriveInfo>();

		mockDriveInfo.Name.Returns("X:");
		mockDriveInfo.IsReady.Returns(true);
		mockDriveInfo.RootDirectoryPath.Returns(@"X:\");
		mockDriveInfo.DriveType.Returns(testWithRemoteDrive ? DriveType.Network : DriveType.Fixed);
		mockDriveInfo.DriveFormat.Returns("FAT");
		mockDriveInfo.AvailableFreeSpace.Returns(1024 * 1024 * 1024);
		mockDriveInfo.TotalFreeSpace.Returns(1024 * 1024 * 1024);
		mockDriveInfo.TotalSize.Returns(1024 * 1024 * 1024);
		mockDriveInfo.VolumeLabel.Returns("FOO");

		var mockDriveInfoProvider = Substitute.For<IDriveInfoProvider>();

		mockDriveInfoProvider.GetDrives().Returns(new IDriveInfo[] { mockDriveInfo });
		mockDriveInfoProvider.GetDrive(Arg.Any<string>()).Returns(mockDriveInfo);

		using (var workspace = new TemporaryDirectory())
		{
			Environment.CurrentDirectory = workspace.Path;

			var machine = new Machine(mockDriveInfoProvider);

			machine.DOS.SetUpRunningProgramSegmentPrefix("");

			const string TestFileName = "TESTFILE.TXT";

			File.WriteAllText(TestFileName, "DOS 6.22");

			int fileHandle = machine.DOS.OpenFile("TESTFILE.TXT", QBX.OperatingSystem.FileStructures.FileMode.Open, OpenMode.Access_ReadWrite);

			ushort expectedRemoteDriveBitValue =
				testWithRemoteDrive
				? (ushort)(1 << 15)
				: (ushort)0;

			try
			{
				var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

				var rin = new RegistersEx();

				rin.AX = (int)Interrupt0x21.Function.Function44 << 8;
				rin.AX |= (int)Interrupt0x21.Function44.IsFileOrDeviceRemote;
				rin.BX = (ushort)fileHandle;

				// Act
				var rout = sut.Execute(rin);

				// Assert
				rout.FLAGS.Should().NotHaveFlag(Flags.Carry);

				ushort remoteDriveBit = rout.DX;

				remoteDriveBit &= expectedRemoteDriveBitValue;

				remoteDriveBit.Should().Be(expectedRemoteDriveBitValue);
			}
			finally
			{
				machine.DOS.CloseFile(fileHandle);
			}
		}
	}

	[Test, NonParallelizable]
	public void DuplicateFileHandle_should_duplicate_file_handle()
	{
		// Arrange
		using (var workspace = new TemporaryDirectory())
		{
			Environment.CurrentDirectory = workspace.Path;

			var machine = new Machine();

			machine.DOS.SetUpRunningProgramSegmentPrefix("");

			const string TestFileName = "TESTFILE.TXT";

			File.WriteAllText(TestFileName, "DOS 6.22");

			int originalFileHandle = machine.DOS.OpenFile(TestFileName, QBX.OperatingSystem.FileStructures.FileMode.Open, OpenMode.Access_ReadWrite);

			var originalFileDescriptor = machine.DOS.Files[originalFileHandle];

			try
			{
				var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

				var rin = new RegistersEx();

				rin.AX = (int)Interrupt0x21.Function.DuplicateFileHandle << 8;
				rin.BX = (ushort)originalFileHandle;

				// Act
				var rout = sut.Execute(rin);

				// Assert
				rout.FLAGS.Should().NotHaveFlag(Flags.Carry);

				int duplicatedFileHandle = rout.AX;

				try
				{
					duplicatedFileHandle.Should().NotBe(originalFileHandle);
					duplicatedFileHandle.Should().BeInRange(2, machine.DOS.Files.Count - 1);

					var duplicatedFileDescriptor = machine.DOS.Files[duplicatedFileHandle];

					duplicatedFileDescriptor.Should().BeSameAs(originalFileDescriptor);

					duplicatedFileDescriptor.ReferenceCount.Should().BeGreaterThan(1);
				}
				finally
				{
					machine.DOS.CloseFile(duplicatedFileHandle);
				}
			}
			finally
			{
				machine.DOS.CloseFile(originalFileHandle);
			}
		}
	}

	[Test]
	public void DuplicateFileHandle_should_duplicate_standard_handle([Values(DOS.StandardInput, DOS.StandardOutput)] int originalFileHandle)
	{
		// Arrange
		var machine = new Machine();

		machine.DOS.SetUpRunningProgramSegmentPrefix("");

		var originalFileDescriptor = machine.DOS.Files[originalFileHandle];

		var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

		var rin = new RegistersEx();

		rin.AX = (int)Interrupt0x21.Function.DuplicateFileHandle << 8;
		rin.BX = (ushort)originalFileHandle;

		// Act
		var rout = sut.Execute(rin);

		// Assert
		rout.FLAGS.Should().NotHaveFlag(Flags.Carry);

		int duplicatedFileHandle = rout.AX;

		try
		{
			duplicatedFileHandle.Should().NotBe(originalFileHandle);
			duplicatedFileHandle.Should().BeInRange(2, machine.DOS.Files.Count - 1);

			var duplicatedFileDescriptor = machine.DOS.Files[duplicatedFileHandle];

			duplicatedFileDescriptor.Should().BeSameAs(originalFileDescriptor);

			duplicatedFileDescriptor.ReferenceCount.Should().BeGreaterThan(1);
		}
		finally
		{
			machine.DOS.CloseFile(duplicatedFileHandle);
		}
	}

	[Test, NonParallelizable]
	public void DuplicateFileHandle_should_create_a_handle_that_still_works_when_the_original_is_closed()
	{
		// Arrange
		using (var workspace = new TemporaryDirectory())
		{
			Environment.CurrentDirectory = workspace.Path;

			var machine = new Machine();

			machine.DOS.SetUpRunningProgramSegmentPrefix("");

			const string TestFileName = "TESTFILE.TXT";
			const string TestData = "DOS 6.22";

			File.WriteAllText(TestFileName, TestData);

			byte[] testDataBytes = s_cp437.GetBytes(TestData);

			int originalFileHandle = machine.DOS.OpenFile(TestFileName, QBX.OperatingSystem.FileStructures.FileMode.Open, OpenMode.Access_ReadWrite);

			var originalFileDescriptor = machine.DOS.Files[originalFileHandle];

			const int ReadBufferSize = 100;

			int readBufferAddress = machine.DOS.MemoryManager.AllocateMemory(ReadBufferSize, machine.DOS.CurrentPSPSegment);

			var readBufferSpan = machine.SystemMemory.AsSpan().Slice(readBufferAddress, ReadBufferSize);

			try
			{
				var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

				var rin = new RegistersEx();

				rin.AX = (int)Interrupt0x21.Function.DuplicateFileHandle << 8;
				rin.BX = (ushort)originalFileHandle;

				// Act & Assert
				var rout = sut.Execute(rin);

				rout.FLAGS.Should().NotHaveFlag(Flags.Carry);

				int duplicatedFileHandle = rout.AX;

				try
				{
					duplicatedFileHandle.Should().NotBe(originalFileHandle);
					duplicatedFileHandle.Should().BeInRange(2, machine.DOS.Files.Count - 1);

					machine.DOS.CloseFile(originalFileHandle);

					// Exercise the new handle, make sure it still works.
					int readSize = machine.DOS.Read(duplicatedFileHandle, machine.SystemMemory, readBufferAddress, ReadBufferSize);

					readSize.Should().Be(testDataBytes.Length);

					readBufferSpan.Slice(0, readSize).ShouldMatch(testDataBytes);
				}
				finally
				{
					machine.DOS.CloseFile(duplicatedFileHandle);
				}
			}
			finally
			{
				machine.DOS.CloseFile(originalFileHandle);
			}
		}
	}

	[Test, NonParallelizable]
	public void DuplicateFileHandle_should_create_a_handle_that_can_be_closed_without_interfering_with_the_original()
	{
		// Arrange
		using (var workspace = new TemporaryDirectory())
		{
			Environment.CurrentDirectory = workspace.Path;

			var machine = new Machine();

			machine.DOS.SetUpRunningProgramSegmentPrefix("");

			const string TestFileName = "TESTFILE.TXT";
			const string TestData = "DOS 6.22";

			File.WriteAllText(TestFileName, TestData);

			byte[] testDataBytes = s_cp437.GetBytes(TestData);

			int originalFileHandle = machine.DOS.OpenFile(TestFileName, QBX.OperatingSystem.FileStructures.FileMode.Open, OpenMode.Access_ReadWrite);

			var originalFileDescriptor = machine.DOS.Files[originalFileHandle];

			const int ReadBufferSize = 100;

			int readBufferAddress = machine.DOS.MemoryManager.AllocateMemory(ReadBufferSize, machine.DOS.CurrentPSPSegment);

			var readBufferSpan = machine.SystemMemory.AsSpan().Slice(readBufferAddress, ReadBufferSize);

			try
			{
				var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

				var rin = new RegistersEx();

				rin.AX = (int)Interrupt0x21.Function.DuplicateFileHandle << 8;
				rin.BX = (ushort)originalFileHandle;

				// Act & Assert
				var rout = sut.Execute(rin);

				rout.FLAGS.Should().NotHaveFlag(Flags.Carry);

				int duplicatedFileHandle = rout.AX;

				try
				{
					duplicatedFileHandle.Should().NotBe(originalFileHandle);
					duplicatedFileHandle.Should().BeInRange(2, machine.DOS.Files.Count - 1);

					machine.DOS.CloseFile(duplicatedFileHandle);

					// Exercise the original handle, make sure it still works.
					int readSize = machine.DOS.Read(originalFileHandle, machine.SystemMemory, readBufferAddress, ReadBufferSize);

					readSize.Should().Be(testDataBytes.Length);

					readBufferSpan.Slice(0, readSize).ShouldMatch(testDataBytes);
				}
				finally
				{
					machine.DOS.CloseFile(duplicatedFileHandle);
				}
			}
			finally
			{
				machine.DOS.CloseFile(originalFileHandle);
			}
		}
	}

	[Test, NonParallelizable]
	public void DuplicateFileHandle_should_create_a_handle_that_closes_properly()
	{
		// Arrange
		using (var workspace = new TemporaryDirectory())
		{
			Environment.CurrentDirectory = workspace.Path;

			var machine = new Machine();

			machine.DOS.SetUpRunningProgramSegmentPrefix("");

			const string TestFileName = "TESTFILE.TXT";
			const string TestData = "DOS 6.22";

			File.WriteAllText(TestFileName, TestData);

			byte[] testDataBytes = s_cp437.GetBytes(TestData);

			int originalFileHandle = machine.DOS.OpenFile(TestFileName, QBX.OperatingSystem.FileStructures.FileMode.Open, OpenMode.Access_ReadWrite);

			var originalFileDescriptor = machine.DOS.Files[originalFileHandle];

			const int ReadBufferSize = 100;

			int readBufferAddress = machine.DOS.MemoryManager.AllocateMemory(ReadBufferSize, machine.DOS.CurrentPSPSegment);

			var readBufferSpan = machine.SystemMemory.AsSpan().Slice(readBufferAddress, ReadBufferSize);

			try
			{
				var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

				var rin = new RegistersEx();

				rin.AX = (int)Interrupt0x21.Function.DuplicateFileHandle << 8;
				rin.BX = (ushort)originalFileHandle;

				// Act & Assert
				var rout = sut.Execute(rin);

				rout.FLAGS.Should().NotHaveFlag(Flags.Carry);

				int duplicatedFileHandle = rout.AX;

				try
				{
					duplicatedFileHandle.Should().NotBe(originalFileHandle);
					duplicatedFileHandle.Should().BeInRange(2, machine.DOS.Files.Count - 1);

					machine.DOS.CloseFile(duplicatedFileHandle);

					// Verify that the handle we just closed no longer works.
					var action =
						() => machine.DOS.Read(duplicatedFileHandle, machine.SystemMemory, readBufferAddress, ReadBufferSize);

					action.Should().Throw<Exception>();

					machine.DOS.LastError.Should().Be(DOSError.InvalidHandle);
				}
				finally
				{
					machine.DOS.CloseFile(duplicatedFileHandle);
				}
			}
			finally
			{
				machine.DOS.CloseFile(originalFileHandle);
			}
		}
	}

	[Test, NonParallelizable]
	public void ForceDuplicateFileHandle_should_duplicate_handle_to_unused_handle()
	{
		// Arrange
		using (var workspace = new TemporaryDirectory())
		{
			Environment.CurrentDirectory = workspace.Path;

			var machine = new Machine();

			machine.DOS.SetUpRunningProgramSegmentPrefix("");

			const string TestFileName = "TESTFILE.TXT";

			File.WriteAllText(TestFileName, "DOS 6.22");

			int originalFileHandle = machine.DOS.OpenFile(TestFileName, QBX.OperatingSystem.FileStructures.FileMode.Open, OpenMode.Access_ReadWrite);

			var originalFileDescriptor = machine.DOS.Files[originalFileHandle];

			int duplicatedFileHandle = machine.DOS.Files.FindIndex(entry => entry is null);

			if (duplicatedFileHandle < 0)
				duplicatedFileHandle = machine.DOS.Files.Count;

			try
			{
				var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

				var rin = new RegistersEx();

				rin.AX = (int)Interrupt0x21.Function.ForceDuplicateFileHandle << 8;
				rin.BX = (ushort)originalFileHandle;
				rin.CX = (ushort)duplicatedFileHandle;

				// Act
				var rout = sut.Execute(rin);

				// Assert
				rout.FLAGS.Should().NotHaveFlag(Flags.Carry);

				try
				{
					duplicatedFileHandle.Should().BeInRange(2, machine.DOS.Files.Count - 1);

					var duplicatedFileDescriptor = machine.DOS.Files[duplicatedFileHandle];

					duplicatedFileDescriptor.Should().BeSameAs(originalFileDescriptor);

					duplicatedFileDescriptor.ReferenceCount.Should().BeGreaterThan(1);
				}
				finally
				{
					machine.DOS.CloseFile(duplicatedFileHandle);
				}
			}
			finally
			{
				machine.DOS.CloseFile(originalFileHandle);
			}
		}
	}

	[Test, NonParallelizable]
	public void ForceDuplicateFileHandle_should_duplicate_handle_to_unused_handle_forcing_file_handle_table_to_expand_by_more_than_one()
	{
		// Arrange
		using (var workspace = new TemporaryDirectory())
		{
			Environment.CurrentDirectory = workspace.Path;

			var machine = new Machine();

			machine.DOS.SetUpRunningProgramSegmentPrefix("");

			const string TestFileName = "TESTFILE.TXT";

			File.WriteAllText(TestFileName, "DOS 6.22");

			int originalFileHandle = machine.DOS.OpenFile(TestFileName, QBX.OperatingSystem.FileStructures.FileMode.Open, OpenMode.Access_ReadWrite);

			var originalFileDescriptor = machine.DOS.Files[originalFileHandle];

			int duplicatedFileHandle = machine.DOS.Files.Count + 10;

			try
			{
				var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

				var rin = new RegistersEx();

				rin.AX = (int)Interrupt0x21.Function.ForceDuplicateFileHandle << 8;
				rin.BX = (ushort)originalFileHandle;
				rin.CX = (ushort)duplicatedFileHandle;

				// Act
				int tableSizeBefore = machine.DOS.Files.Count;

				var rout = sut.Execute(rin);

				int tableSizeAfter = machine.DOS.Files.Count;

				// Assert
				rout.FLAGS.Should().NotHaveFlag(Flags.Carry);

				try
				{
					duplicatedFileHandle.Should().BeInRange(2, machine.DOS.Files.Count - 1);

					for (int i = tableSizeBefore; i < tableSizeAfter; i++)
					{
						if (i != duplicatedFileHandle)
							machine.DOS.Files[i].Should().BeNull();
					}

					var duplicatedFileDescriptor = machine.DOS.Files[duplicatedFileHandle];

					duplicatedFileDescriptor.Should().BeSameAs(originalFileDescriptor);

					duplicatedFileDescriptor.ReferenceCount.Should().BeGreaterThan(1);
				}
				finally
				{
					machine.DOS.CloseFile(duplicatedFileHandle);
				}
			}
			finally
			{
				machine.DOS.CloseFile(originalFileHandle);
			}
		}
	}

	[Test, NonParallelizable]
	public void ForceDuplicateFileHandle_should_duplicate_handle_to_open_handle_and_close_the_original_handle()
	{
		// Arrange
		using (var workspace = new TemporaryDirectory())
		{
			Environment.CurrentDirectory = workspace.Path;

			var machine = new Machine();

			machine.DOS.SetUpRunningProgramSegmentPrefix("");

			const string TestFileName = "TESTFILE.TXT";

			File.WriteAllText(TestFileName, "DOS 6.22");

			int originalFileHandle = DOS.StandardInput;

			var originalFileDescriptor = machine.DOS.Files[originalFileHandle];

			int duplicatedFileHandle = machine.DOS.OpenFile(TestFileName, QBX.OperatingSystem.FileStructures.FileMode.Open, OpenMode.Access_ReadWrite);

			var duplicatedFileDescriptorBefore = machine.DOS.Files[duplicatedFileHandle];

			duplicatedFileDescriptorBefore.Should().NotBeNull();

			var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

			var rin = new RegistersEx();

			rin.AX = (int)Interrupt0x21.Function.ForceDuplicateFileHandle << 8;
			rin.BX = (ushort)originalFileHandle;
			rin.CX = (ushort)duplicatedFileHandle;

			// Act
			int tableSizeBefore = machine.DOS.Files.Count;

			var rout = sut.Execute(rin);

			int tableSizeAfter = machine.DOS.Files.Count;

			// Assert
			rout.FLAGS.Should().NotHaveFlag(Flags.Carry);

			try
			{
				tableSizeAfter.Should().Be(tableSizeBefore);

				var duplicatedFileDescriptor = machine.DOS.Files[duplicatedFileHandle];

				duplicatedFileDescriptor.Should().BeSameAs(originalFileDescriptor);
				duplicatedFileDescriptor.Should().NotBeSameAs(duplicatedFileDescriptorBefore);

				duplicatedFileDescriptor.ReferenceCount.Should().BeGreaterThan(1);
				duplicatedFileDescriptorBefore.ReferenceCount.Should().Be(0);
				duplicatedFileDescriptorBefore.IsClosed.Should().BeTrue();
			}
			finally
			{
				machine.DOS.CloseFile(duplicatedFileHandle);
			}
		}
	}

	[Test, NonParallelizable]
	public void GetCurrentDirectory_should_return_mapped_current_directory()
	{
		// Arrange
		using (var workspace = new TemporaryDirectory())
		{
			Environment.CurrentDirectory = workspace.Path;

			Assume.That(ShortFileNames.TryMap(Environment.CurrentDirectory, out var shortCurrentDirectory) == true);

			// GetCurrentDirectory does not include a drive letter.
			if (PathCharacter.HasDriveLetter(shortCurrentDirectory))
				shortCurrentDirectory = shortCurrentDirectory.Substring(3);

			// GetCurrentDirectory always uses backslashes.
			shortCurrentDirectory = shortCurrentDirectory.Replace('/', '\\');

			byte[] shortCurrentDirectoryBytes = s_cp437.GetBytes(shortCurrentDirectory);

			// GetCurrentDirectory is documented as never writing more than 64 bytes (including null terminator).
			Assume.That(shortCurrentDirectoryBytes.Length < 64);

			var machine = new Machine();

			machine.DOS.SetUpRunningProgramSegmentPrefix("");

			const int BufferSize = 64;

			int bufferAddress = machine.DOS.MemoryManager.AllocateMemory(BufferSize, machine.DOS.CurrentPSPSegment);

			var bufferSpan = machine.SystemMemory.AsSpan().Slice(bufferAddress, BufferSize);

			var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

			var rin = new RegistersEx();

			rin.AX = (int)Interrupt0x21.Function.GetCurrentDirectory << 8;
			rin.DS = (ushort)(bufferAddress / MemoryManager.ParagraphSize);
			rin.SI = (ushort)(bufferAddress % MemoryManager.ParagraphSize);
			rin.DX = 0; // use the default drive

			// Act
			var rout = sut.Execute(rin);

			// Assert
			rout.FLAGS.Should().NotHaveFlag(Flags.Carry);

			bufferSpan.Slice(0, shortCurrentDirectoryBytes.Length).ShouldMatch(shortCurrentDirectoryBytes);
			bufferSpan[shortCurrentDirectoryBytes.Length].Should().Be(0);
		}
	}

	[Test]
	public void AllocateMemory_should_allocate_memory()
	{
		// Arrange
		var machine = new Machine();

		const int AllocationSizeInParagraphs = 1234;
		const int AllocationSizeInBytes = AllocationSizeInParagraphs * MemoryManager.ParagraphSize;

		const int AllocationSegment = 5678;
		const int AllocationLinearAddress = AllocationSegment * MemoryManager.ParagraphSize;

		var mockMemoryManager = Substitute.For<IMemoryManager>();

		mockMemoryManager.AllocateMemory(Arg.Any<int>(), Arg.Any<ushort>()).Returns(
			callInfo =>
			{
				int length = callInfo.Arg<int>();
				ushort ownerPSPSegment = callInfo.Arg<ushort>();

				return mockMemoryManager.AllocateMemory(length, ownerPSPSegment, out _);
			});

		mockMemoryManager.AllocateMemory(Arg.Any<int>(), Arg.Any<ushort>(), out Arg.Any<int>()).Returns(
			callInfo =>
			{
				int length = callInfo.ArgAt<int>(0);

				if (length != AllocationSizeInBytes)
					throw new Exception("Unrecognized allocation size");

				callInfo[2] = 0;

				return AllocationLinearAddress;
			});

		machine.DOS.MemoryManager = mockMemoryManager;

		var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

		var rin = new RegistersEx();

		rin.AX = (int)Interrupt0x21.Function.AllocateMemory << 8;
		rin.BX = AllocationSizeInParagraphs;

		// Act
		var rout = sut.Execute(rin);

		// Assert
		rout.FLAGS.Should().NotHaveFlag(Flags.Carry);

		int allocatedSegment = rout.AX;

		mockMemoryManager.Received().AllocateMemory(
			Arg.Is(AllocationSizeInBytes),
			Arg.Is(machine.DOS.CurrentPSPSegment),
			out Arg.Any<int>());

		allocatedSegment.Should().Be(AllocationSegment);
	}

	[Test]
	public void AllocateMemory_should_return_largest_available_block_size_when_request_is_too_large_to_satisfy()
	{
		// Arrange
		var machine = new Machine();

		const int AllocationSizeInParagraphs = 1234;
		const int AllocationSizeInBytes = AllocationSizeInParagraphs * MemoryManager.ParagraphSize;

		const int AvailableSizeInParagraphs = 876;
		const int AvailableSizeInBytes = AvailableSizeInParagraphs * MemoryManager.ParagraphSize;

		var mockMemoryManager = Substitute.For<IMemoryManager>();

		mockMemoryManager.AllocateMemory(Arg.Any<int>(), Arg.Any<ushort>()).Returns(
			callInfo =>
			{
				int length = callInfo.Arg<int>();
				ushort ownerPSPSegment = callInfo.Arg<ushort>();

				return mockMemoryManager.AllocateMemory(length, ownerPSPSegment, out _);
			});

		mockMemoryManager.AllocateMemory(Arg.Any<int>(), Arg.Any<ushort>(), out Arg.Any<int>()).Returns(
			callInfo =>
			{
				int length = callInfo.ArgAt<int>(0);

				if (length != AllocationSizeInBytes)
					throw new Exception("Unrecognized allocation size");

				callInfo[2] = AvailableSizeInBytes;

				throw new DOSException(DOSError.NotEnoughMemory);
			});

		machine.DOS.MemoryManager = mockMemoryManager;

		var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

		var rin = new RegistersEx();

		rin.AX = (int)Interrupt0x21.Function.AllocateMemory << 8;
		rin.BX = AllocationSizeInParagraphs;

		// Act
		var rout = sut.Execute(rin);

		// Assert
		rout.FLAGS.Should().HaveFlag(Flags.Carry);

		rout.AX.Should().Be((ushort)DOSError.NotEnoughMemory);

		rout.BX.Should().Be(AvailableSizeInParagraphs);

		mockMemoryManager.Received().AllocateMemory(
			Arg.Is(AllocationSizeInBytes),
			Arg.Is(machine.DOS.CurrentPSPSegment),
			out Arg.Any<int>());
	}

	[Test]
	public void FreeAllocatedMemory_should_free_previously_allocated_block()
	{
		// Arrange
		var machine = new Machine();

		var mockMemoryManager = Substitute.For<IMemoryManager>();

		machine.DOS.MemoryManager = mockMemoryManager;

		const int PreviouslyAllocatedSegment = 876;
		const int PreviouslyAllocatedLinearAddress = PreviouslyAllocatedSegment * MemoryManager.ParagraphSize;

		var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

		var rin = new RegistersEx();

		rin.AX = (int)Interrupt0x21.Function.FreeAllocatedMemory << 8;
		rin.ES = PreviouslyAllocatedSegment;

		// Act
		var rout = sut.Execute(rin);

		// Assert
		rout.FLAGS.Should().NotHaveFlag(Flags.Carry);

		mockMemoryManager.Received().FreeMemory(PreviouslyAllocatedLinearAddress);
	}

	[Test]
	public void FreeAllocatedMemory_should_return_correct_error_if_address_is_not_valid()
	{
		// Arrange
		var machine = new Machine();

		const int InvalidBlockSegment = 876;
		const int InvalidBlockLinearAddress = InvalidBlockSegment * MemoryManager.ParagraphSize;

		var mockMemoryManager = Substitute.For<IMemoryManager>();

		mockMemoryManager
			.When(mock => mock.FreeMemory(Arg.Any<int>()))
			.Do(
				callInfo =>
				{
					int address = callInfo.Arg<int>();

					if (address == InvalidBlockLinearAddress)
						throw new DOSException(DOSError.InvalidBlock);
				});

		machine.DOS.MemoryManager = mockMemoryManager;

		var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

		var rin = new RegistersEx();

		rin.AX = (int)Interrupt0x21.Function.FreeAllocatedMemory << 8;
		rin.ES = InvalidBlockSegment;

		// Act
		var rout = sut.Execute(rin);

		// Assert
		rout.FLAGS.Should().HaveFlag(Flags.Carry);

		rout.AX.Should().Be((ushort)DOSError.InvalidBlock);

		mockMemoryManager.Received().FreeMemory(InvalidBlockLinearAddress);
	}

	[Test]
	public void SetMemoryBlockSize_should_resize_block_to_new_size()
	{
		// Arrange
		var machine = new Machine();

		var mockMemoryManager = Substitute.For<IMemoryManager>();

		machine.DOS.MemoryManager = mockMemoryManager;

		const int PreviouslyAllocatedSegment = 876;
		const int PreviouslyAllocatedLinearAddress = PreviouslyAllocatedSegment * MemoryManager.ParagraphSize;

		const int NewBlockSizeInParagraphs = 2345;
		const int NewBlockSizeInBytes = NewBlockSizeInParagraphs * MemoryManager.ParagraphSize;

		var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

		var rin = new RegistersEx();

		rin.AX = (int)Interrupt0x21.Function.SetMemoryBlockSize << 8;
		rin.ES = PreviouslyAllocatedSegment;
		rin.BX = NewBlockSizeInParagraphs;

		// Act
		var rout = sut.Execute(rin);

		// Assert
		rout.FLAGS.Should().NotHaveFlag(Flags.Carry);

		mockMemoryManager.Received().ResizeAllocation(
			Arg.Is(PreviouslyAllocatedLinearAddress),
			Arg.Is(NewBlockSizeInBytes),
			out Arg.Any<int>());
	}

	[Test]
	public void SetMemoryBlockSize_should_return_largest_available_chunk_when_request_is_too_large_to_satisfy()
	{
		// Arrange
		var machine = new Machine();

		const int PreviouslyAllocatedSegment = 876;
		const int PreviouslyAllocatedLinearAddress = PreviouslyAllocatedSegment * MemoryManager.ParagraphSize;

		const int NewBlockSizeInParagraphs = 2345;
		const int NewBlockSizeInBytes = NewBlockSizeInParagraphs * MemoryManager.ParagraphSize;

		const int AvailableSizeInParagraphs = 1235;
		const int AvailableSizeInBytes = AvailableSizeInParagraphs * MemoryManager.ParagraphSize;

		var mockMemoryManager = Substitute.For<IMemoryManager>();

		mockMemoryManager
			.When(mock => mock.ResizeAllocation(Arg.Any<int>(), Arg.Any<int>(), out Arg.Any<int>()))
			.Do(
				callInfo =>
				{
					callInfo[2] = AvailableSizeInBytes;

					throw new DOSException(DOSError.NotEnoughMemory);
				});

		machine.DOS.MemoryManager = mockMemoryManager;

		var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

		var rin = new RegistersEx();

		rin.AX = (int)Interrupt0x21.Function.SetMemoryBlockSize << 8;
		rin.ES = PreviouslyAllocatedSegment;
		rin.BX = NewBlockSizeInParagraphs;

		// Act
		var rout = sut.Execute(rin);

		// Assert
		rout.FLAGS.Should().HaveFlag(Flags.Carry);

		rout.AX.Should().Be((ushort)DOSError.NotEnoughMemory);

		rout.BX.Should().Be(AvailableSizeInParagraphs);

		mockMemoryManager.Received().ResizeAllocation(
			Arg.Is(PreviouslyAllocatedLinearAddress),
			Arg.Is(NewBlockSizeInBytes),
			out Arg.Any<int>());
	}

	[Test]
	public void SetMemoryBlockSize_should_return_correct_error_if_address_is_not_valid()
	{
		// Arrange
		var machine = new Machine();

		const int InvalidBlockSegment = 876;
		const int InvalidBlockLinearAddress = InvalidBlockSegment * MemoryManager.ParagraphSize;

		const int NewBlockSizeInParagraphs = 2345;
		const int NewBlockSizeInBytes = NewBlockSizeInParagraphs * MemoryManager.ParagraphSize;

		var mockMemoryManager = Substitute.For<IMemoryManager>();

		mockMemoryManager
			.When(mock => mock.ResizeAllocation(Arg.Any<int>(), Arg.Any<int>(), out Arg.Any<int>()))
			.Do(
				callInfo =>
				{
					callInfo[2] = 0;

					int address = callInfo.ArgAt<int>(0);

					if (address == InvalidBlockLinearAddress)
						throw new DOSException(DOSError.InvalidBlock);
				});

		machine.DOS.MemoryManager = mockMemoryManager;

		var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

		var rin = new RegistersEx();

		rin.AX = (int)Interrupt0x21.Function.SetMemoryBlockSize << 8;
		rin.ES = InvalidBlockSegment;
		rin.BX = NewBlockSizeInParagraphs;

		// Act
		var rout = sut.Execute(rin);

		// Assert
		rout.FLAGS.Should().HaveFlag(Flags.Carry);

		rout.AX.Should().Be((ushort)DOSError.InvalidBlock);

		mockMemoryManager.Received().ResizeAllocation(
			Arg.Is(InvalidBlockLinearAddress),
			Arg.Is(NewBlockSizeInBytes),
			out Arg.Any<int>());
	}

	[Test]
	public void LoadAndExecuteProgram_should_run_program()
	{
		// Arrange
		var machine = new Machine();

		const int TestExitCode = 42;

		(string programFile, string commandTail) =
			RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
			? ("cmd.exe", $"/c exit {TestExitCode}")
			: ("sh", $"-c 'exit {TestExitCode}'");

		programFile = ShellExecute.FindProgramFileOnPath(programFile, out var interpreter);

		if (interpreter != null)
		{
			commandTail = $"{interpreter} {commandTail}";
			programFile = ShellExecute.FindProgramFileOnPath(programFile, out interpreter);

			if (interpreter != null)
				throw new Exception("Interpreter needed for interpreter?");
		}

		if (!ShortFileNames.TryMap(programFile, out var programFileShort))
			throw new Exception("Failed to map program file name");

		byte[] programFileBytes = s_cp437.GetBytes(programFileShort);

		int programFileAddress = machine.DOS.MemoryManager.AllocateMemory(programFileBytes.Length + 1, machine.DOS.CurrentPSPSegment);

		var programFileSpan = machine.SystemMemory.AsSpan().Slice(programFileAddress, programFileBytes.Length + 1);

		programFileBytes.CopyTo(programFileSpan);
		programFileSpan[programFileBytes.Length] = 0;

		var loadExec =
			new LoadExec()
			{
				CommandTail = commandTail,
			};

		loadExec.Environment["PATH"] = "C:\\DOS";

		var loadExecAddress = machine.DOS.MemoryManager.AllocateMemory(LoadExec.Size, machine.DOS.CurrentPSPSegment);

		loadExec.Serialize(
			machine.SystemMemory,
			loadExecAddress,
			machine.DOS.MemoryManager,
			machine.DOS.CurrentPSPSegment);

		var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

		var rin = new RegistersEx();

		rin.AX = (int)Interrupt0x21.Function.Function4B << 8;
		rin.AX |= (int)Interrupt0x21.Function4B.LoadAndExecuteProgram;
		rin.DS = (ushort)(programFileAddress / MemoryManager.ParagraphSize);
		rin.DX = (ushort)(programFileAddress % MemoryManager.ParagraphSize);
		rin.ES = (ushort)(loadExecAddress / MemoryManager.ParagraphSize);
		rin.BX = (ushort)(loadExecAddress % MemoryManager.ParagraphSize);

		// Act
		var rout = sut.Execute(rin);

		// Assert
		machine.DOS.LastChildProcessExitCode.Should().Be(TestExitCode);
	}

	[Test]
	public void EndProgram_should_mark_DOS_as_terminated_and_store_exit_code(
		[Values(0, 1, 37, 254, 255)] byte testExitCode)
	{
		// Arrange
		var machine = new Machine();

		var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

		var rin = new Registers();

		rin.AX = (int)Interrupt0x21.Function.EndProgram << 8;
		rin.AX |= testExitCode;

		Assume.That(machine.DOS.IsTerminated == false);

		// Act & Assert
		Action action = () => sut.Execute(rin);

		action.Should().Throw<TerminatedException>();
		machine.DOS.IsTerminated.Should().BeTrue();
		machine.DOS.LastError.Should().Be(DOSError.None);
		machine.ExitCode.Should().Be(testExitCode);
	}

	[Test]
	public void GetChildProgramReturnValue_should_return_last_child_process_exit_code()
	{
		// Arrange
		var machine = new Machine();

		const int TestExitCode = 25;

		(string programFile, string commandTail) =
			RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
			? ("cmd.exe", $"/c exit {TestExitCode}")
			: ("sh", $"-c 'exit {TestExitCode}'");

		var loadExec =
			new LoadExec()
			{
				CommandTail = commandTail,
			};

		programFile = ShellExecute.FindProgramFileOnPath(programFile, out var interpreter);

		if (interpreter != null)
		{
			commandTail = $"{interpreter} {commandTail}";
			programFile = ShellExecute.FindProgramFileOnPath(programFile, out interpreter);

			if (interpreter != null)
				throw new Exception("Interpreter needed for interpreter?");
		}

		machine.DOS.ExecuteChildProcess(programFile, loadExec);

		var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

		var rin = new Registers();

		rin.AX = (int)Interrupt0x21.Function.GetChildProgramReturnValue << 8;

		// Act
		var rout = sut.Execute(rin);

		// Assert
		int actualExitCode = rout.AX & 0xFF;

		actualExitCode.Should().Be(TestExitCode);
	}

	[Test, NonParallelizable]
	public void FindFirstFile_should_handle_no_matches()
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

			const string TestPattern = "TEST4*.*";

			byte[] patternBytes = s_cp437.GetBytes(TestPattern);

			int patternAddress = machine.DOS.MemoryManager.AllocateMemory(patternBytes.Length + 1, machine.DOS.CurrentPSPSegment);

			var patternSpan = machine.SystemMemory.AsSpan().Slice(patternAddress, patternBytes.Length + 1);

			patternBytes.CopyTo(patternSpan);
			patternSpan[patternBytes.Length] = 0;

			var dtaAddress = machine.DOS.MemoryManager.AllocateMemory(DOSFileInfo.Size, machine.DOS.CurrentPSPSegment);

			var dtaAddressSegmented = new SegmentedAddress(dtaAddress);

			machine.DOS.DiskTransferAddressSegment = dtaAddressSegmented.Segment;
			machine.DOS.DiskTransferAddressOffset = dtaAddressSegmented.Offset;

			var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

			var rin = new RegistersEx();

			rin.AX = (int)Interrupt0x21.Function.FindFirstFile << 8;
			rin.DS = (ushort)(patternAddress / MemoryManager.ParagraphSize);
			rin.DX = (ushort)(patternAddress % MemoryManager.ParagraphSize);

			// Act
			var rout = sut.Execute(rin);

			// Assert
			rout.FLAGS.Should().HaveFlag(Flags.Carry);

			rout.AX.Should().Be((ushort)DOSError.FileNotFound);
		}
	}

	[Test, NonParallelizable]
	public void FindFirstFile_should_find_first_file()
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

			byte[] patternBytes = s_cp437.GetBytes(TestPattern);

			int patternAddress = machine.DOS.MemoryManager.AllocateMemory(patternBytes.Length + 1, machine.DOS.CurrentPSPSegment);

			var patternSpan = machine.SystemMemory.AsSpan().Slice(patternAddress, patternBytes.Length + 1);

			patternBytes.CopyTo(patternSpan);
			patternSpan[patternBytes.Length] = 0;

			var dtaAddress = machine.DOS.MemoryManager.AllocateMemory(DOSFileInfo.Size, machine.DOS.CurrentPSPSegment);

			var dtaAddressSegmented = new SegmentedAddress(dtaAddress);

			machine.DOS.DiskTransferAddressSegment = dtaAddressSegmented.Segment;
			machine.DOS.DiskTransferAddressOffset = dtaAddressSegmented.Offset;

			var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

			var rin = new RegistersEx();

			rin.AX = (int)Interrupt0x21.Function.FindFirstFile << 8;
			rin.DS = (ushort)(patternAddress / MemoryManager.ParagraphSize);
			rin.DX = (ushort)(patternAddress % MemoryManager.ParagraphSize);

			// Act
			var rout = sut.Execute(rin);

			// Assert
			rout.FLAGS.Should().NotHaveFlag(Flags.Carry);

			var fileInfo = new DOSFileInfo();

			fileInfo.Deserialize(machine.SystemMemory, dtaAddress);

			var validMatches = Directory.GetFiles(workspace.Path, TestPattern).Select(path => Path.GetFileName(path));

			fileInfo.FileName.ToStringZ().Should().BeOneOf(validMatches);
		}
	}

	[Test, NonParallelizable]
	public void FindNextFile_should_find_all_matching_files()
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

			byte[] patternBytes = s_cp437.GetBytes(TestPattern);

			int patternAddress = machine.DOS.MemoryManager.AllocateMemory(patternBytes.Length + 1, machine.DOS.CurrentPSPSegment);

			var patternSpan = machine.SystemMemory.AsSpan().Slice(patternAddress, patternBytes.Length + 1);

			patternBytes.CopyTo(patternSpan);
			patternSpan[patternBytes.Length] = 0;

			var dtaAddress = machine.DOS.MemoryManager.AllocateMemory(DOSFileInfo.Size, machine.DOS.CurrentPSPSegment);

			var dtaAddressSegmented = new SegmentedAddress(dtaAddress);

			machine.DOS.DiskTransferAddressSegment = dtaAddressSegmented.Segment;
			machine.DOS.DiskTransferAddressOffset = dtaAddressSegmented.Offset;

			var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

			var rin = new RegistersEx();

			rin.AX = (int)Interrupt0x21.Function.FindFirstFile << 8;
			rin.DS = (ushort)(patternAddress / MemoryManager.ParagraphSize);
			rin.DX = (ushort)(patternAddress % MemoryManager.ParagraphSize);

			var rout = sut.Execute(rin);

			var allMatches = new List<string>();

			var fileInfo = new DOSFileInfo();

			fileInfo.Deserialize(machine.SystemMemory, dtaAddress);

			allMatches.Add(fileInfo.FileName.ToStringZ());

			rin.AX = (int)Interrupt0x21.Function.FindNextFile << 8;

			// Act
			rout = sut.Execute(rin);

			while ((rout.FLAGS & Flags.Carry) == 0)
			{
				fileInfo.Deserialize(machine.SystemMemory, dtaAddress);

				allMatches.Add(fileInfo.FileName.ToStringZ());

				rout = sut.Execute(rin);
			}

			// Assert
			var validMatches = Directory.GetFiles(workspace.Path, TestPattern).Select(path => Path.GetFileName(path));

			allMatches.Should().BeEquivalentTo(validMatches);
		}
	}

	[Test]
	public void SetPSPAddress_should_set_PSP_address()
	{
		// Arrange
		var machine = new Machine();

		var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

		const ushort TestSegmentValue = 0xBEEF;

		var rin = new RegistersEx();

		rin.AX = (int)Interrupt0x21.Function.SetPSPAddress << 8;
		rin.BX = TestSegmentValue;

		// Act
		var rout = sut.Execute(rin);

		// Assert
		rout.FLAGS.Should().NotHaveFlag(Flags.Carry);

		machine.DOS.CurrentPSPSegment.Should().Be(TestSegmentValue);
	}

	[Test]
	public void GetPSPAddress_should_get_PSP_address()
	{
		// Arrange
		var machine = new Machine();

		var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

		const ushort TestSegmentValue = 0xBABE;

		machine.DOS.CurrentPSPSegment = TestSegmentValue;

		var rin = new RegistersEx();

		rin.AX = (int)Interrupt0x21.Function.GetPSPAddress << 8;

		// Act
		var rout = sut.Execute(rin);

		// Assert
		rout.FLAGS.Should().NotHaveFlag(Flags.Carry);

		rout.BX.Should().Be(TestSegmentValue);
	}

	[Test]
	public void GetVerifyState_should_return_verify_state([Values] bool testVerifyState)
	{
		// Arrange
		var machine = new Machine();

		var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

		machine.DOS.VerifyWrites = testVerifyState;

		int expectedVerifyStateValue = testVerifyState ? 1 : 0;

		var rin = new RegistersEx();

		rin.AX = (int)Interrupt0x21.Function.GetVerifyState << 8;

		// Act
		var rout = sut.Execute(rin);

		// Assert
		rout.FLAGS.Should().NotHaveFlag(Flags.Carry);

		int al = rout.AX & 0xFF;

		al.Should().Be(expectedVerifyStateValue);
	}

	[Test, NonParallelizable]
	public void RenameFile_should_rename_files()
	{
		// Arrange
		using (var workspace = new TemporaryDirectory())
		{
			Environment.CurrentDirectory = workspace.Path;

			var machine = new Machine();

			machine.DOS.SetUpRunningProgramSegmentPrefix("");

			const string OldFileName = "TEST1.TXT";
			const string NewFileName = "TEST2.TXT";

			File.WriteAllText(OldFileName, "");

			byte[] oldFileNameBytes = s_cp437.GetBytes(OldFileName);
			byte[] newFileNameBytes = s_cp437.GetBytes(NewFileName);

			int oldFileNameAddress = machine.DOS.MemoryManager.AllocateMemory(oldFileNameBytes.Length + 1, machine.DOS.CurrentPSPSegment);
			int newFileNameAddress = machine.DOS.MemoryManager.AllocateMemory(newFileNameBytes.Length + 1, machine.DOS.CurrentPSPSegment);

			var oldFileNameSpan = machine.SystemMemory.AsSpan().Slice(oldFileNameAddress, oldFileNameBytes.Length + 1);
			var newFileNameSpan = machine.SystemMemory.AsSpan().Slice(newFileNameAddress, newFileNameBytes.Length + 1);

			oldFileNameBytes.CopyTo(oldFileNameSpan);
			oldFileNameSpan[oldFileNameBytes.Length] = 0;

			newFileNameBytes.CopyTo(newFileNameSpan);
			newFileNameSpan[newFileNameBytes.Length] = 0;

			var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

			var rin = new RegistersEx();

			rin.AX = (int)Interrupt0x21.Function.RenameFile << 8;
			rin.DS = (ushort)(oldFileNameAddress / MemoryManager.ParagraphSize);
			rin.DX = (ushort)(oldFileNameAddress % MemoryManager.ParagraphSize);
			rin.ES = (ushort)(newFileNameAddress / MemoryManager.ParagraphSize);
			rin.DI = (ushort)(newFileNameAddress % MemoryManager.ParagraphSize);

			// Act
			bool oldExistsBefore = File.Exists(OldFileName);
			bool newExistsBefore = File.Exists(NewFileName);

			var rout = sut.Execute(rin);

			bool oldExistsAfter = File.Exists(OldFileName);
			bool newExistsAfter = File.Exists(NewFileName);

			// Assert
			rout.FLAGS.Should().NotHaveFlag(Flags.Carry);

			oldExistsBefore.Should().BeTrue();
			newExistsBefore.Should().BeFalse();

			oldExistsAfter.Should().BeFalse();
			newExistsAfter.Should().BeTrue();
		}
	}

	[Test, NonParallelizable]
	public void RenameFile_should_rename_directories()
	{
		// Arrange
		using (var workspace = new TemporaryDirectory())
		{
			Environment.CurrentDirectory = workspace.Path;

			var machine = new Machine();

			machine.DOS.SetUpRunningProgramSegmentPrefix("");

			const string OldDirectoryName = "TEST1.TXT";
			const string NewDirectoryName = "TEST2.TXT";

			Directory.CreateDirectory(OldDirectoryName);

			byte[] oldDirectoryNameBytes = s_cp437.GetBytes(OldDirectoryName);
			byte[] newDirectoryNameBytes = s_cp437.GetBytes(NewDirectoryName);

			int oldDirectoryNameAddress = machine.DOS.MemoryManager.AllocateMemory(oldDirectoryNameBytes.Length + 1, machine.DOS.CurrentPSPSegment);
			int newDirectoryNameAddress = machine.DOS.MemoryManager.AllocateMemory(newDirectoryNameBytes.Length + 1, machine.DOS.CurrentPSPSegment);

			var oldDirectoryNameSpan = machine.SystemMemory.AsSpan().Slice(oldDirectoryNameAddress, oldDirectoryNameBytes.Length + 1);
			var newDirectoryNameSpan = machine.SystemMemory.AsSpan().Slice(newDirectoryNameAddress, newDirectoryNameBytes.Length + 1);

			oldDirectoryNameBytes.CopyTo(oldDirectoryNameSpan);
			oldDirectoryNameSpan[oldDirectoryNameBytes.Length] = 0;

			newDirectoryNameBytes.CopyTo(newDirectoryNameSpan);
			newDirectoryNameSpan[newDirectoryNameBytes.Length] = 0;

			var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

			var rin = new RegistersEx();

			rin.AX = (int)Interrupt0x21.Function.RenameFile << 8;
			rin.DS = (ushort)(oldDirectoryNameAddress / MemoryManager.ParagraphSize);
			rin.DX = (ushort)(oldDirectoryNameAddress % MemoryManager.ParagraphSize);
			rin.ES = (ushort)(newDirectoryNameAddress / MemoryManager.ParagraphSize);
			rin.DI = (ushort)(newDirectoryNameAddress % MemoryManager.ParagraphSize);

			// Act
			bool oldExistsBefore = Directory.Exists(OldDirectoryName);
			bool newExistsBefore = Directory.Exists(NewDirectoryName);

			var rout = sut.Execute(rin);

			bool oldExistsAfter = Directory.Exists(OldDirectoryName);
			bool newExistsAfter = Directory.Exists(NewDirectoryName);

			// Assert
			rout.FLAGS.Should().NotHaveFlag(Flags.Carry);

			oldExistsBefore.Should().BeTrue();
			newExistsBefore.Should().BeFalse();

			oldExistsAfter.Should().BeFalse();
			newExistsAfter.Should().BeTrue();
		}
	}

	[Test, NonParallelizable]
	public void RenameFile_should_move_files()
	{
		// Arrange
		using (var workspace = new TemporaryDirectory())
		{
			Environment.CurrentDirectory = workspace.Path;

			var machine = new Machine();

			machine.DOS.SetUpRunningProgramSegmentPrefix("");

			const string OldParentDirectory = "A";
			const string NewParentDirectory = "B";

			const string FileName = "TEST1.TXT";

			const string OldFileName = OldParentDirectory + "/" + FileName;
			const string NewFileName = NewParentDirectory + "/" + FileName;

			Directory.CreateDirectory(OldParentDirectory);
			Directory.CreateDirectory(NewParentDirectory);

			File.WriteAllText(OldFileName, "");

			byte[] oldFileNameBytes = s_cp437.GetBytes(OldFileName);
			byte[] newFileNameBytes = s_cp437.GetBytes(NewFileName);

			int oldFileNameAddress = machine.DOS.MemoryManager.AllocateMemory(oldFileNameBytes.Length + 1, machine.DOS.CurrentPSPSegment);
			int newFileNameAddress = machine.DOS.MemoryManager.AllocateMemory(newFileNameBytes.Length + 1, machine.DOS.CurrentPSPSegment);

			var oldFileNameSpan = machine.SystemMemory.AsSpan().Slice(oldFileNameAddress, oldFileNameBytes.Length + 1);
			var newFileNameSpan = machine.SystemMemory.AsSpan().Slice(newFileNameAddress, newFileNameBytes.Length + 1);

			oldFileNameBytes.CopyTo(oldFileNameSpan);
			oldFileNameSpan[oldFileNameBytes.Length] = 0;

			newFileNameBytes.CopyTo(newFileNameSpan);
			newFileNameSpan[newFileNameBytes.Length] = 0;

			var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

			var rin = new RegistersEx();

			rin.AX = (int)Interrupt0x21.Function.RenameFile << 8;
			rin.DS = (ushort)(oldFileNameAddress / MemoryManager.ParagraphSize);
			rin.DX = (ushort)(oldFileNameAddress % MemoryManager.ParagraphSize);
			rin.ES = (ushort)(newFileNameAddress / MemoryManager.ParagraphSize);
			rin.DI = (ushort)(newFileNameAddress % MemoryManager.ParagraphSize);

			// Act
			bool oldExistsBefore = File.Exists(OldFileName);
			bool newExistsBefore = File.Exists(NewFileName);

			var rout = sut.Execute(rin);

			bool oldExistsAfter = File.Exists(OldFileName);
			bool newExistsAfter = File.Exists(NewFileName);

			// Assert
			rout.FLAGS.Should().NotHaveFlag(Flags.Carry);

			oldExistsBefore.Should().BeTrue();
			newExistsBefore.Should().BeFalse();

			oldExistsAfter.Should().BeFalse();
			newExistsAfter.Should().BeTrue();
		}
	}

	[Test, NonParallelizable]
	public void RenameFile_should_move_directories()
	{
		// Arrange
		using (var workspace = new TemporaryDirectory())
		{
			Environment.CurrentDirectory = workspace.Path;

			var machine = new Machine();

			machine.DOS.SetUpRunningProgramSegmentPrefix("");

			const string OldParentDirectory = "A";
			const string NewParentDirectory = "B";

			const string DirectoryName = "TEST1.TXT";

			const string OldDirectoryName = OldParentDirectory + "/" + DirectoryName;
			const string NewDirectoryName = NewParentDirectory + "/" + DirectoryName;

			Directory.CreateDirectory(OldParentDirectory);
			Directory.CreateDirectory(NewParentDirectory);

			Directory.CreateDirectory(OldDirectoryName);

			byte[] oldDirectoryNameBytes = s_cp437.GetBytes(OldDirectoryName);
			byte[] newDirectoryNameBytes = s_cp437.GetBytes(NewDirectoryName);

			int oldDirectoryNameAddress = machine.DOS.MemoryManager.AllocateMemory(oldDirectoryNameBytes.Length + 1, machine.DOS.CurrentPSPSegment);
			int newDirectoryNameAddress = machine.DOS.MemoryManager.AllocateMemory(newDirectoryNameBytes.Length + 1, machine.DOS.CurrentPSPSegment);

			var oldDirectoryNameSpan = machine.SystemMemory.AsSpan().Slice(oldDirectoryNameAddress, oldDirectoryNameBytes.Length + 1);
			var newDirectoryNameSpan = machine.SystemMemory.AsSpan().Slice(newDirectoryNameAddress, newDirectoryNameBytes.Length + 1);

			oldDirectoryNameBytes.CopyTo(oldDirectoryNameSpan);
			oldDirectoryNameSpan[oldDirectoryNameBytes.Length] = 0;

			newDirectoryNameBytes.CopyTo(newDirectoryNameSpan);
			newDirectoryNameSpan[newDirectoryNameBytes.Length] = 0;

			var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

			var rin = new RegistersEx();

			rin.AX = (int)Interrupt0x21.Function.RenameFile << 8;
			rin.DS = (ushort)(oldDirectoryNameAddress / MemoryManager.ParagraphSize);
			rin.DX = (ushort)(oldDirectoryNameAddress % MemoryManager.ParagraphSize);
			rin.ES = (ushort)(newDirectoryNameAddress / MemoryManager.ParagraphSize);
			rin.DI = (ushort)(newDirectoryNameAddress % MemoryManager.ParagraphSize);

			// Act
			bool oldExistsBefore = Directory.Exists(OldDirectoryName);
			bool newExistsBefore = Directory.Exists(NewDirectoryName);

			var rout = sut.Execute(rin);

			bool oldExistsAfter = Directory.Exists(OldDirectoryName);
			bool newExistsAfter = Directory.Exists(NewDirectoryName);

			// Assert
			rout.FLAGS.Should().NotHaveFlag(Flags.Carry);

			oldExistsBefore.Should().BeTrue();
			newExistsBefore.Should().BeFalse();

			oldExistsAfter.Should().BeFalse();
			newExistsAfter.Should().BeTrue();
		}
	}

	[Test, NonParallelizable]
	public void GetFileDateAndTime_should_return_file_last_modified_datetime()
	{
		FileDateTimeTest(
			Interrupt0x21.Function57.GetFileDateAndTime,
			arrange:
				(dateTime, fileName, rin) =>
				{
					File.SetLastWriteTime(fileName, dateTime);
				},
			assert:
				(fileName, rout) =>
				{
					var time = new FileTime() { Raw = rout.CX };
					var date = new FileDate() { Raw = rout.DX };

					return date.Get().ToDateTime(time.Get());
				});
	}

	[Test, NonParallelizable]
	public void SetFileDateAndTime_should_return_file_last_modified_datetime()
	{
		FileDateTimeTest(
			Interrupt0x21.Function57.SetFileDateAndTime,
			arrange:
				(dateTime, fileName, rin) =>
				{
					var time = new FileTime().Set(dateTime);
					var date = new FileDate().Set(dateTime);

					rin.CX = time.Raw;
					rin.DX = date.Raw;
				},
			assert:
				(fileName, rout) =>
				{
					return File.GetLastWriteTime(fileName);
				});
	}

	[Test, NonParallelizable]
	public void GetFileLastAccessDateAndTime_should_return_file_last_accessed_datetime()
	{
		FileDateTimeTest(
			Interrupt0x21.Function57.GetFileLastAccessDateAndTime,
			arrange:
				(dateTime, fileName, rin) =>
				{
					File.SetLastAccessTime(fileName, dateTime);
				},
			assert:
				(fileName, rout) =>
				{
					var time = new FileTime() { Raw = rout.CX };
					var date = new FileDate() { Raw = rout.DX };

					return date.Get().ToDateTime(time.Get());
				});
	}

	[Test, NonParallelizable]
	public void SetFileLastAccessDateAndTime_should_return_file_last_accessed_datetime()
	{
		FileDateTimeTest(
			Interrupt0x21.Function57.SetFileLastAccessDateAndTime,
			arrange:
				(dateTime, fileName, rin) =>
				{
					var time = new FileTime().Set(dateTime);
					var date = new FileDate().Set(dateTime);

					rin.CX = time.Raw;
					rin.DX = date.Raw;
				},
			assert:
				(fileName, rout) =>
				{
					return File.GetLastAccessTime(fileName);
				});
	}

	[Test, NonParallelizable]
	[Platform(Exclude = NUnitPlatformNames.Linux)]
	public void GetFileCreationDateAndTime_should_return_file_created_datetime()
	{
		FileDateTimeTest(
			Interrupt0x21.Function57.GetFileCreationDateAndTime,
			arrange:
				(dateTime, fileName, rin) =>
				{
					File.SetCreationTime(fileName, dateTime);
				},
			assert:
				(fileName, rout) =>
				{
					var time = new FileTime() { Raw = rout.CX };
					var date = new FileDate() { Raw = rout.DX };

					return date.Get().ToDateTime(time.Get());
				});
	}

	[Test, NonParallelizable]
	[Platform(Exclude = NUnitPlatformNames.Linux)]
	public void SetFileCreationDateAndTime_should_return_file_created_datetime()
	{
		FileDateTimeTest(
			Interrupt0x21.Function57.SetFileCreationDateAndTime,
			arrange:
				(dateTime, fileName, rin) =>
				{
					var time = new FileTime().Set(dateTime);
					var date = new FileDate().Set(dateTime);

					rin.CX = time.Raw;
					rin.DX = date.Raw;
				},
			assert:
				(fileName, rout) =>
				{
					return File.GetCreationTime(fileName);
				});
	}

	[Test]
	public void GetAllocationStrategy_should_return_allocation_strategy([Values] MemoryAllocationStrategy testAllocationStrategy)
	{
		// Arrange
		var machine = new Machine();

		var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

		machine.DOS.MemoryManager.AllocationStrategy = testAllocationStrategy;

		var rin = new RegistersEx();

		rin.AX = (int)Interrupt0x21.Function.Function58 << 8;
		rin.AX |= (int)Interrupt0x21.Function58.GetAllocationStrategy;

		// Act
		var rout = sut.Execute(rin);

		// Assert
		rout.FLAGS.Should().NotHaveFlag(Flags.Carry);

		var allocationStrategy = (MemoryAllocationStrategy)rout.AX;

		allocationStrategy.Should().Be(testAllocationStrategy);
	}

	[Test]
	public void SetAllocationStrategy_should_set_allocation_strategy([Values] MemoryAllocationStrategy testAllocationStrategy)
	{
		// Arrange
		var machine = new Machine();

		var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

		var rin = new RegistersEx();

		rin.AX = (int)Interrupt0x21.Function.Function58 << 8;
		rin.AX |= (int)Interrupt0x21.Function58.SetAllocationStrategy;
		rin.BX = (ushort)testAllocationStrategy;

		// Act
		var rout = sut.Execute(rin);

		// Assert
		rout.FLAGS.Should().NotHaveFlag(Flags.Carry);

		machine.DOS.MemoryManager.AllocationStrategy.Should().Be(testAllocationStrategy);
	}

	[Test]
	public void GetExtendedError_should_return_error_details()
	{
		// Arrange
		var machine = new Machine();

		var testError = RandomEnumValue<DOSError>();
		var testErrorClass = RandomEnumValue<DOSErrorClass>();
		var testErrorAction = RandomEnumValue<DOSErrorAction>();
		var testErrorLocation = RandomEnumValue<DOSErrorLocation>();

		machine.DOS.SetExtendedError(testError, testErrorClass, testErrorAction, testErrorLocation);

		var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

		var rin = new RegistersEx();

		rin.AX = (int)Interrupt0x21.Function.GetExtendedError << 8;

		// Act
		var rout = sut.Execute(rin);

		// Assert
		rout.FLAGS.Should().NotHaveFlag(Flags.Carry);

		var error = (DOSError)rout.AX;
		var errorClass = (DOSErrorClass)(rout.BX >> 8);
		var errorAction = (DOSErrorAction)(rout.BX & 0xFF);
		var errorLocation = (DOSErrorLocation)(rout.CX >> 8);

		error.Should().Be(testError);
		errorClass.Should().Be(testErrorClass);
		errorAction.Should().Be(testErrorAction);
		errorLocation.Should().Be(testErrorLocation);
	}

	[Test, NonParallelizable]
	public void CreateTemporaryFile_should_create_and_open_temporary_file([Values(".", "A")] string folderName)
	{
		// Arrange
		using (var workspace = new TemporaryDirectory())
		{
			Environment.CurrentDirectory = workspace.Path;

			var machine = new Machine();

			machine.DOS.SetUpRunningProgramSegmentPrefix("");

			Directory.CreateDirectory("A");

			const int FileNameBufferSize = 64;

			int fileNameAddress = machine.DOS.MemoryManager.AllocateMemory(FileNameBufferSize, machine.DOS.CurrentPSPSegment);

			var fileNameSpan = machine.SystemMemory.AsSpan().Slice(fileNameAddress, FileNameBufferSize + 1);

			fileNameSpan.Clear();
			s_cp437.GetBytes(folderName + "\\", fileNameSpan);

			var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

			var rin = new RegistersEx();

			rin.AX = (int)Interrupt0x21.Function.CreateTemporaryFile << 8;
			rin.DS = (ushort)(fileNameAddress / MemoryManager.ParagraphSize);
			rin.DX = (ushort)(fileNameAddress % MemoryManager.ParagraphSize);

			// Act
			var rout = sut.Execute(rin);

			// Assert
			rout.FLAGS.Should().NotHaveFlag(Flags.Carry);

			int fileHandle = rout.AX;

			try
			{
				int terminator = fileNameSpan.IndexOf((byte)0);

				if (terminator < 0)
					terminator = fileNameSpan.Length;

				string shortFileName = s_cp437.GetString(fileNameSpan.Slice(0, terminator));

				string longFileName = ShortFileNames.Unmap(shortFileName);

				File.Exists(longFileName).Should().BeTrue();

				fileHandle.Should().BeInRange(2, machine.DOS.Files.Count - 1);

				var fileDescriptor = machine.DOS.Files[fileHandle];

				string fileDescriptorPath = fileDescriptor.Should().BeOfType<RegularFileDescriptor>().Which.PhysicalPath;

				IsSameFile(longFileName, fileDescriptorPath).Should().BeTrue();
			}
			finally
			{
				machine.DOS.CloseFile(fileHandle);
			}
		}
	}

	[Test, NonParallelizable]
	public void CreateNewFile_should_create_files()
	{
		// Arrange
		using (var workspace = new TemporaryDirectory())
		{
			Environment.CurrentDirectory = workspace.Path;

			var machine = new Machine();

			machine.DOS.SetUpRunningProgramSegmentPrefix("");

			const string TestFileName = "TESTFILE.TXT";

			byte[] fileNameBytes = s_cp437.GetBytes(TestFileName);

			int fileNameBufferSize = fileNameBytes.Length + 1;

			var fileNameAddress = machine.DOS.MemoryManager.AllocateMemory(fileNameBufferSize, machine.DOS.CurrentPSPSegment);

			var fileNameSpan = machine.SystemMemory.AsSpan().Slice(fileNameAddress, fileNameBufferSize);

			fileNameBytes.CopyTo(fileNameSpan);
			fileNameSpan[fileNameBytes.Length] = 0;

			int fileHandle = -1;

			try
			{
				var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

				var rin = new RegistersEx();

				rin.AX = (int)Interrupt0x21.Function.CreateNewFile << 8;
				rin.DS = (ushort)(fileNameAddress / MemoryManager.ParagraphSize);
				rin.DX = (ushort)(fileNameAddress % MemoryManager.ParagraphSize);
				rin.CX = (ushort)QBX.OperatingSystem.FileStructures.FileAttributes.Normal;

				// Act
				bool existsBefore = File.Exists(TestFileName);

				var rout = sut.Execute(rin);

				bool existsAfter = File.Exists(TestFileName);

				// Assert
				rout.FLAGS.Should().NotHaveFlag(Flags.Carry);

				fileHandle = rout.AX;

				existsBefore.Should().BeFalse();
				existsAfter.Should().BeTrue();

				fileHandle.Should().BeInRange(2, machine.DOS.Files.Count - 1);

				var regularFile = (RegularFileDescriptor)machine.DOS.Files[fileHandle]!;

				regularFile.PhysicalPath.Should().Be(Path.GetFullPath(TestFileName));
			}
			finally
			{
				machine.DOS.CloseFile(fileHandle);
			}
		}
	}

	[Test, NonParallelizable]
	public void CreateNewFile_should_fail_if_file_already_exists()
	{
		// Arrange
		using (var workspace = new TemporaryDirectory())
		{
			Environment.CurrentDirectory = workspace.Path;

			var machine = new Machine();

			machine.DOS.SetUpRunningProgramSegmentPrefix("");

			const string TestFileName = "TESTFILE.TXT";

			File.WriteAllText(TestFileName, "QuickBASIC");

			byte[] fileNameBytes = s_cp437.GetBytes(TestFileName);

			int fileNameBufferSize = fileNameBytes.Length + 1;

			var fileNameAddress = machine.DOS.MemoryManager.AllocateMemory(fileNameBufferSize, machine.DOS.CurrentPSPSegment);

			var fileNameSpan = machine.SystemMemory.AsSpan().Slice(fileNameAddress, fileNameBufferSize);

			fileNameBytes.CopyTo(fileNameSpan);
			fileNameSpan[fileNameBytes.Length] = 0;

			var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

			var rin = new RegistersEx();

			rin.AX = (int)Interrupt0x21.Function.CreateNewFile << 8;
			rin.DS = (ushort)(fileNameAddress / MemoryManager.ParagraphSize);
			rin.DX = (ushort)(fileNameAddress % MemoryManager.ParagraphSize);

			// Act
			var rout = sut.Execute(rin);

			// Assert
			rout.FLAGS.Should().HaveFlag(Flags.Carry);

			rout.AX.Should().Be((ushort)DOSError.FileExists);
		}
	}

	[Test]
	public void LockUnlockFile_should_lock_file_byte_regions()
	{
		// Arrange
		var machine = new Machine();

		var mockFile = Substitute.For<FileDescriptor>("TESTFILE.TXT");

		machine.DOS.Files.Add(mockFile);

		int fileHandle = machine.DOS.Files.Count - 1;

		const ushort LockFlag = 0; // 00h = lock
		const int LockRegionOffset = 0x2345ABCD;
		const int LockRegionLength = 0x1234FEDC;

		var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

		var rin = new RegistersEx();

		rin.AX = (int)Interrupt0x21.Function.LockUnlockFile << 8;
		rin.AX |= LockFlag;

		rin.BX = (ushort)fileHandle;

		rin.CX = unchecked((ushort)(LockRegionOffset >> 16));
		rin.DX = unchecked((ushort)LockRegionOffset);

		rin.SI = unchecked((ushort)(LockRegionLength	 >> 16));
		rin.DI = unchecked((ushort)LockRegionLength);

		// Act
		var rout = sut.Execute(rin);

		// Assert
		rout.FLAGS.Should().NotHaveFlag(Flags.Carry);

		mockFile.Received().Lock(LockRegionOffset, LockRegionLength);
	}

	[Test]
	public void LockUnlockFile_should_unlock_file_byte_regions()
	{
		// Arrange
		var machine = new Machine();

		var mockFile = Substitute.For<FileDescriptor>("TESTFILE.TXT");

		machine.DOS.Files.Add(mockFile);

		int fileHandle = machine.DOS.Files.Count - 1;

		const ushort LockFlag = 1; // 01h = unlock
		const int LockRegionOffset = 0x2345ABCD;
		const int LockRegionLength = 0x1234FEDC;

		var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

		var rin = new RegistersEx();

		rin.AX = (int)Interrupt0x21.Function.LockUnlockFile << 8;
		rin.AX |= LockFlag;

		rin.BX = (ushort)fileHandle;

		rin.CX = unchecked((ushort)(LockRegionOffset >> 16));
		rin.DX = unchecked((ushort)LockRegionOffset);

		rin.SI = unchecked((ushort)(LockRegionLength	 >> 16));
		rin.DI = unchecked((ushort)LockRegionLength);

		// Act
		var rout = sut.Execute(rin);

		// Assert
		rout.FLAGS.Should().NotHaveFlag(Flags.Carry);

		mockFile.Received().Unlock(LockRegionOffset, LockRegionLength);
	}

	const int SetExtendedErrorRepetitions = 3;

	[Test, Sequential]
	public void SetExtendedError_should_set_error_values(
		[Random(SetExtendedErrorRepetitions)] ushort testError,
		[Random(SetExtendedErrorRepetitions)] byte testErrorClass,
		[Random(SetExtendedErrorRepetitions)] byte testErrorAction,
		[Random(SetExtendedErrorRepetitions)] ushort testErrorLocation)
	{
		// Arrange
		var machine = new Machine();

		machine.DOS.SetUpRunningProgramSegmentPrefix("");

		// ERROR   STRUC
		//     errAX       dw  ?   ;ax register
		//     errBX       dw  ?   ;bx register
		//     errCX       dw  ?   ;cx register
		//     errDX       dw  ?   ;dx register
		//     errSI       dw  ?   ;si register
		//     errDI       dw  ?   ;di register
		//     errDS       dw  ?   ;ds register
		//     errES       dw  ?   ;es register
		//     errReserved dw  ?   ;reserved 16 bits
		//     errUID      dw  ?   ;user (computer) ID (0 = local computer)
		//     errPID      dw  ?   ;program ID (0 = local program)
		// ERROR   ENDS
		//
		// This structure is very self-describing and says exactly how
		// each field will be interpreted.
		//
		// Fortunately, it is documented:
		// - errAX  Specifies the error value.
		// - errBX  Specifies the error class in the high-order byte and the suggested action
		//          in the low-order byte.
		// - errCX  Specifies the error-location value.

		const int ErrorStructureFields = 11; // they're all words

		const int ErrorStructureSize = ErrorStructureFields * 2;

		int errorStructureAddress = machine.DOS.MemoryManager.AllocateMemory(ErrorStructureSize, machine.DOS.CurrentPSPSegment);

		var errorStructureStream = new SystemMemoryStream(machine.MemoryBus, errorStructureAddress, ErrorStructureSize);

		var writer = new BinaryWriter(errorStructureStream);

		writer.Write(testError); // ax
		writer.Write((ushort)((testErrorClass << 8) | testErrorAction)); // bx
		writer.Write(testErrorLocation); // cx
		writer.Write((ushort)0); // dx
		writer.Write((ushort)0); // si
		writer.Write((ushort)0); // di
		writer.Write((ushort)0); // ds
		writer.Write((ushort)0); // es
		writer.Write((ushort)0); // reserved
		writer.Write((ushort)0); // uid
		writer.Write((ushort)0); // pid

		var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

		var rin = new RegistersEx();

		rin.AX = (int)Interrupt0x21.Function.SetExtendedError << 8;
		rin.AX |= 0x0A; // ??
		rin.DS = unchecked((ushort)(errorStructureAddress / MemoryManager.ParagraphSize));
		rin.SI = unchecked((ushort)(errorStructureAddress % MemoryManager.ParagraphSize));

		// Act
		var rout = sut.Execute(rin);

		// Assert
		machine.DOS.LastError.Should().Be((DOSError)testError);
		machine.DOS.LastErrorClass.Should().Be((DOSErrorClass)testErrorClass);
		machine.DOS.LastErrorAction.Should().Be((DOSErrorAction)testErrorAction);
		machine.DOS.LastErrorLocation.Should().Be((DOSErrorLocation)testErrorLocation);
	}

	[Test]
	public void GetMachineName_should_copy_machine_name_to_buffer()
	{
		// Arrange
		var machine = new Machine();

		machine.DOS.SetUpRunningProgramSegmentPrefix("");

		const int BufferSize = 16;

		int bufferAddress = machine.DOS.MemoryManager.AllocateMemory(BufferSize, machine.DOS.CurrentPSPSegment);

		var bufferSpan = machine.SystemMemory.AsSpan().Slice(bufferAddress, BufferSize);

		var expectedMachineName = Environment.MachineName.AsSpan();

		int dot = expectedMachineName.IndexOf('.');

		if (dot > 0)
			expectedMachineName = expectedMachineName.Slice(0, dot);

		if (expectedMachineName.Length > 15)
			expectedMachineName.Slice(0, 15);

		byte[] expectedMachineNameBytes = new byte[expectedMachineName.Length + 1];

		s_cp437.GetBytes(expectedMachineName, expectedMachineNameBytes);

		var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

		var rin = new RegistersEx();

		rin.AX = (int)Interrupt0x21.Function.Function5E << 8;
		rin.AX |= (int)Interrupt0x21.Function5E.GetMachineName;
		rin.DS = unchecked((ushort)(bufferAddress / MemoryManager.ParagraphSize));
		rin.DX = unchecked((ushort)(bufferAddress % MemoryManager.ParagraphSize));

		// Act
		var rout = sut.Execute(rin);

		// Assert
		rout.FLAGS.Should().NotHaveFlag(Flags.Carry);

		int ch = rout.CX >> 8;

		ch.Should().NotBe(0);

		bufferSpan.ShouldStartWith(expectedMachineNameBytes);
	}

	[Test]
	public void SetPrinterSetup_should_not_crash()
	{
		// Arrange
		var machine = new Machine();

		machine.DOS.SetUpRunningProgramSegmentPrefix("");

		const int SetupStringSize = 100;
		const string SetupString = "blah";

		var setupStringAddress = machine.DOS.MemoryManager.AllocateMemory(SetupStringSize, machine.DOS.CurrentPSPSegment);

		var setupStringSpan = machine.SystemMemory.AsSpan().Slice(setupStringAddress, SetupStringSize);

		s_cp437.GetBytes(SetupString + "\0").CopyTo(setupStringSpan);

		var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

		var rin = new RegistersEx();

		rin.AX = (int)Interrupt0x21.Function.Function5E << 8;
		rin.AX |= (int)Interrupt0x21.Function5E.SetPrinterSetup;
		rin.DS = unchecked((ushort)(setupStringAddress / MemoryManager.ParagraphSize));
		rin.DX = unchecked((ushort)(setupStringAddress % MemoryManager.ParagraphSize));
		rin.CX = (ushort)SetupString.Length;

		// Act
		var rout = sut.Execute(rin);

		// Assert
		rout.FLAGS.Should().HaveFlag(Flags.Carry);
		rout.AX.Should().Be((ushort)DOSError.InvalidFunction);
	}

	[Test]
	public void GetPrinterSetup_should_not_crash()
	{
		// Arrange
		var machine = new Machine();

		machine.DOS.SetUpRunningProgramSegmentPrefix("");

		const int SetupStringSize = 100;

		var setupStringAddress = machine.DOS.MemoryManager.AllocateMemory(SetupStringSize, machine.DOS.CurrentPSPSegment);

		var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

		var rin = new RegistersEx();

		rin.AX = (int)Interrupt0x21.Function.Function5E << 8;
		rin.AX |= (int)Interrupt0x21.Function5E.GetPrinterSetup;
		rin.ES = unchecked((ushort)(setupStringAddress / MemoryManager.ParagraphSize));
		rin.DI = unchecked((ushort)(setupStringAddress % MemoryManager.ParagraphSize));

		// Act
		var rout = sut.Execute(rin);

		// Assert
		rout.FLAGS.Should().HaveFlag(Flags.Carry);
		rout.AX.Should().Be((ushort)DOSError.InvalidFunction);
	}

	[Test, NonParallelizable]
	public void TrueName_should_capitalize_path()
	{
		// Arrange
		using (var workspace = new TemporaryDirectory())
		{
			Environment.CurrentDirectory = workspace.Path;

			var machine = new Machine();

			machine.DOS.SetUpRunningProgramSegmentPrefix("");

			const string TestFileName = "a/testfile.txt";

			Directory.CreateDirectory("A");
			File.WriteAllText(TestFileName, "QuickBASIC");

			if (!ShortFileNames.TryMap(TestFileName, out var expectedTrueName))
				throw new Exception("Failed to map test filename");

			expectedTrueName = ShortFileNames.GetFullPath(expectedTrueName);

			expectedTrueName = expectedTrueName.ToUpper();

			var expectedTrueNameBytes = s_cp437.GetBytes(expectedTrueName + '\0');

			byte[] fileNameBytes = s_cp437.GetBytes(TestFileName);

			int inputBufferSize = fileNameBytes.Length + 1;

			var inputAddress = machine.DOS.MemoryManager.AllocateMemory(inputBufferSize, machine.DOS.CurrentPSPSegment);

			var inputSpan = machine.SystemMemory.AsSpan().Slice(inputAddress, inputBufferSize);

			fileNameBytes.CopyTo(inputSpan);
			inputSpan[fileNameBytes.Length] = 0;

			const int OutputBufferSize = 128;

			var outputAddress = machine.DOS.MemoryManager.AllocateMemory(OutputBufferSize, machine.DOS.CurrentPSPSegment);

			var outputSpan = machine.SystemMemory.AsSpan().Slice(outputAddress, OutputBufferSize);

			var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

			var rin = new RegistersEx();

			rin.AX = (int)Interrupt0x21.Function.TrueName << 8;
			rin.DS = (ushort)(inputAddress / MemoryManager.ParagraphSize);
			rin.SI = (ushort)(inputAddress % MemoryManager.ParagraphSize);
			rin.ES = (ushort)(outputAddress / MemoryManager.ParagraphSize);
			rin.DI = (ushort)(outputAddress % MemoryManager.ParagraphSize);

			// Act
			var rout = sut.Execute(rin);

			// Assert
			rout.FLAGS.Should().NotHaveFlag(Flags.Carry);

			outputSpan.ShouldStartWith(expectedTrueNameBytes);
		}
	}

	[Test, NonParallelizable]
	public void TrueName_should_normalize_separator_characters()
	{
		// Arrange
		using (var workspace = new TemporaryDirectory())
		{
			Environment.CurrentDirectory = workspace.Path;

			var machine = new Machine();

			machine.DOS.SetUpRunningProgramSegmentPrefix("");

			const string TestFileName = "a/testfile.txt";

			Directory.CreateDirectory("A");
			File.WriteAllText(TestFileName, "QuickBASIC");

			if (!ShortFileNames.TryMap(TestFileName, out var expectedTrueName))
				throw new Exception("Failed to map test filename");

			expectedTrueName = ShortFileNames.GetFullPath(expectedTrueName);

			string input = expectedTrueName.Replace('\\', '/');

			expectedTrueName = expectedTrueName.ToUpper();

			var expectedTrueNameBytes = s_cp437.GetBytes(expectedTrueName + '\0');

			byte[] inputBytes = s_cp437.GetBytes(input);

			int inputBufferSize = inputBytes.Length + 1;

			var inputAddress = machine.DOS.MemoryManager.AllocateMemory(inputBufferSize, machine.DOS.CurrentPSPSegment);

			var inputSpan = machine.SystemMemory.AsSpan().Slice(inputAddress, inputBufferSize);

			inputBytes.CopyTo(inputSpan);
			inputSpan[inputBytes.Length] = 0;

			const int OutputBufferSize = 128;

			var outputAddress = machine.DOS.MemoryManager.AllocateMemory(OutputBufferSize, machine.DOS.CurrentPSPSegment);

			var outputSpan = machine.SystemMemory.AsSpan().Slice(outputAddress, OutputBufferSize);

			var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

			var rin = new RegistersEx();

			rin.AX = (int)Interrupt0x21.Function.TrueName << 8;
			rin.DS = (ushort)(inputAddress / MemoryManager.ParagraphSize);
			rin.SI = (ushort)(inputAddress % MemoryManager.ParagraphSize);
			rin.ES = (ushort)(outputAddress / MemoryManager.ParagraphSize);
			rin.DI = (ushort)(outputAddress % MemoryManager.ParagraphSize);

			// Act
			var rout = sut.Execute(rin);

			// Assert
			rout.FLAGS.Should().NotHaveFlag(Flags.Carry);

			outputSpan.ShouldStartWith(expectedTrueNameBytes);
		}
	}

	[Test, NonParallelizable]
	public void TrueName_should_follow_navigation_tokens_to_current_directory()
	{
		// Arrange
		using (var workspace = new TemporaryDirectory())
		{
			Environment.CurrentDirectory = workspace.Path;

			var machine = new Machine();

			machine.DOS.SetUpRunningProgramSegmentPrefix("");

			const string TestFileName = "testfile.txt";

			File.WriteAllText(TestFileName, "QuickBASIC");

			if (!ShortFileNames.TryMap(TestFileName, out var expectedTrueName))
				throw new Exception("Failed to map test filename");

			expectedTrueName = ShortFileNames.GetFullPath(expectedTrueName);
			expectedTrueName = expectedTrueName.ToUpper();

			string input = expectedTrueName;

			int lastSlash = input.LastIndexOf('\\');

			input = input.Substring(0, lastSlash) + "\\." + input.Substring(lastSlash);

			var expectedTrueNameBytes = s_cp437.GetBytes(expectedTrueName + '\0');

			byte[] inputBytes = s_cp437.GetBytes(input);

			int inputBufferSize = inputBytes.Length + 1;

			var inputAddress = machine.DOS.MemoryManager.AllocateMemory(inputBufferSize, machine.DOS.CurrentPSPSegment);

			var inputSpan = machine.SystemMemory.AsSpan().Slice(inputAddress, inputBufferSize);

			inputBytes.CopyTo(inputSpan);
			inputSpan[inputBytes.Length] = 0;

			const int OutputBufferSize = 128;

			var outputAddress = machine.DOS.MemoryManager.AllocateMemory(OutputBufferSize, machine.DOS.CurrentPSPSegment);

			var outputSpan = machine.SystemMemory.AsSpan().Slice(outputAddress, OutputBufferSize);

			var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

			var rin = new RegistersEx();

			rin.AX = (int)Interrupt0x21.Function.TrueName << 8;
			rin.DS = (ushort)(inputAddress / MemoryManager.ParagraphSize);
			rin.SI = (ushort)(inputAddress % MemoryManager.ParagraphSize);
			rin.ES = (ushort)(outputAddress / MemoryManager.ParagraphSize);
			rin.DI = (ushort)(outputAddress % MemoryManager.ParagraphSize);

			// Act
			var rout = sut.Execute(rin);

			// Assert
			rout.FLAGS.Should().NotHaveFlag(Flags.Carry);

			outputSpan.ShouldStartWith(expectedTrueNameBytes);
		}
	}

	[Test, NonParallelizable]
	public void TrueName_should_follow_navigation_tokens_to_parent_directory()
	{
		// Arrange
		using (var workspace = new TemporaryDirectory())
		{
			Environment.CurrentDirectory = workspace.Path;

			var machine = new Machine();

			machine.DOS.SetUpRunningProgramSegmentPrefix("");

			const string TestFileName = "testfile.txt";

			File.WriteAllText(TestFileName, "QuickBASIC");

			if (!ShortFileNames.TryMap(TestFileName, out var expectedTrueName))
				throw new Exception("Failed to map test filename");

			expectedTrueName = ShortFileNames.GetFullPath(expectedTrueName);
			expectedTrueName = expectedTrueName.ToUpper();

			Directory.CreateDirectory("A");

			Environment.CurrentDirectory = "A";

			string input = expectedTrueName;

			int lastSlash = input.LastIndexOf('\\');

			input = input.Substring(0, lastSlash) + "\\A\\.." + input.Substring(lastSlash);

			var expectedTrueNameBytes = s_cp437.GetBytes(expectedTrueName + '\0');

			byte[] inputBytes = s_cp437.GetBytes(input);

			int inputBufferSize = inputBytes.Length + 1;

			var inputAddress = machine.DOS.MemoryManager.AllocateMemory(inputBufferSize, machine.DOS.CurrentPSPSegment);

			var inputSpan = machine.SystemMemory.AsSpan().Slice(inputAddress, inputBufferSize);

			inputBytes.CopyTo(inputSpan);
			inputSpan[inputBytes.Length] = 0;

			const int OutputBufferSize = 128;

			var outputAddress = machine.DOS.MemoryManager.AllocateMemory(OutputBufferSize, machine.DOS.CurrentPSPSegment);

			var outputSpan = machine.SystemMemory.AsSpan().Slice(outputAddress, OutputBufferSize);

			var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

			var rin = new RegistersEx();

			rin.AX = (int)Interrupt0x21.Function.TrueName << 8;
			rin.DS = (ushort)(inputAddress / MemoryManager.ParagraphSize);
			rin.SI = (ushort)(inputAddress % MemoryManager.ParagraphSize);
			rin.ES = (ushort)(outputAddress / MemoryManager.ParagraphSize);
			rin.DI = (ushort)(outputAddress % MemoryManager.ParagraphSize);

			// Act
			var rout = sut.Execute(rin);

			// Assert
			rout.FLAGS.Should().NotHaveFlag(Flags.Carry);

			outputSpan.ShouldStartWith(expectedTrueNameBytes);
		}
	}

	[Test]
	public void GetCurrentPSPAddress_should_return_PSP_segment()
	{
		// Arrange
		var machine = new Machine();

		machine.DOS.SetUpRunningProgramSegmentPrefix("");

		var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

		var rin = new RegistersEx();

		rin.AX = (int)Interrupt0x21.Function.GetCurrentPSPAddress << 8;

		// Act
		var rout = sut.Execute(rin);

		// Assert
		rout.FLAGS.Should().NotHaveFlag(Flags.Carry);

		rout.BX.Should().Be(machine.DOS.CurrentPSPSegment);
	}

	[Test, Sequential]
	public void GetExtendedCountryInformation_should_return_culture_information(
		[Values(0xFFFF, 437, 850)] int testCodePageID,
		[Values((CountryCode)0xFFFF, CountryCode.UnitedStates, CountryCode.CanadianFrench)] CountryCode testCountryCode)
	{
		// Arrange
		var machine = new Machine();

		machine.DOS.SetUpRunningProgramSegmentPrefix("");

		var bufferAddress = machine.DOS.MemoryManager.AllocateMemory(ExtendedCountryInfo.Size, machine.DOS.CurrentPSPSegment);

		var expectedCountryInfo = new ExtendedCountryInfo();

		if (testCodePageID == 0xFFFF) // current
			expectedCountryInfo.Import(machine.DOS.CurrentCulture);
		else
		{
			var culture = CultureUtility.GetCultureInfoForCodePageAndCountry(testCodePageID, testCountryCode);

			if (culture == null)
				throw new Exception("Couldn't get test culture");

			expectedCountryInfo.Import(culture);
		}

		var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

		var rin = new RegistersEx();

		rin.AX = (int)Interrupt0x21.Function.Function65 << 8;
		rin.AX |= (int)Interrupt0x21.Function65.GetExtendedCountryInformation;
		rin.BX = (ushort)testCodePageID;
		rin.CX = ExtendedCountryInfo.Size;
		rin.DX = (ushort)testCountryCode;
		rin.ES = unchecked((ushort)(bufferAddress / MemoryManager.ParagraphSize));
		rin.DI = unchecked((ushort)(bufferAddress % MemoryManager.ParagraphSize));

		// Act
		var rout = sut.Execute(rin);

		// Assert
		rout.FLAGS.Should().NotHaveFlag(Flags.Carry);

		var actualCountryInfo = new ExtendedCountryInfo();

		actualCountryInfo.Deserialize(machine.MemoryBus, bufferAddress);

		actualCountryInfo.Should().BeEquivalentTo(expectedCountryInfo);
	}

	[Test, Sequential]
	public void GetUppercaseTable_should_present_and_return_uppercase_table(
		[Values(0xFFFF, 437, 850)] int testCodePageID,
		[Values((CountryCode)0xFFFF, CountryCode.UnitedStates, CountryCode.CanadianFrench)] CountryCode testCountryCode)
	{
		InternationalizationTableTest(
			table: Interrupt0x21.Function65.GetUppercaseTable,
			testCodePageID,
			testCountryCode,
			getExpectedTable: culture => CharacterTables.GetUppercaseTable(culture));
	}

	[Test, Sequential]
	public void GetFilenameUppercaseTable_should_present_and_return_filename_uppercase_table(
		[Values(0xFFFF, 437, 850)] int testCodePageID,
		[Values((CountryCode)0xFFFF, CountryCode.UnitedStates, CountryCode.CanadianFrench)] CountryCode testCountryCode)
	{
		InternationalizationTableTest(
			table: Interrupt0x21.Function65.GetFilenameUppercaseTable,
			testCodePageID,
			testCountryCode,
			getExpectedTable: culture => CharacterTables.GetFilenameUppercaseTable(culture));
	}

	[Test, Sequential]
	public void GetFilenameCharacterTable_should_present_and_return_filename_character_table(
		[Values(0xFFFF, 437, 850)] int testCodePageID,
		[Values((CountryCode)0xFFFF, CountryCode.UnitedStates, CountryCode.CanadianFrench)] CountryCode testCountryCode)
	{
		InternationalizationTableTest(
			table: Interrupt0x21.Function65.GetFilenameCharacterTable,
			testCodePageID,
			testCountryCode,
			getExpectedTable: _ => FileCharTable.Default.ToByteArray());
	}

	[Test, Sequential]
	public void GetCollateSequenceTable_should_present_and_return_collate_sequence_table(
		[Values(0xFFFF, 437, 850)] int testCodePageID,
		[Values((CountryCode)0xFFFF, CountryCode.UnitedStates, CountryCode.CanadianFrench)] CountryCode testCountryCode)
	{
		InternationalizationTableTest(
			table: Interrupt0x21.Function65.GetCollateSequenceTable,
			testCodePageID,
			testCountryCode,
			getExpectedTable: culture => CharacterTables.GetCollateSequenceTable(culture));
	}

	[Test, Sequential]
	public void GetDoubleByteCharacterSet_should_present_and_return_doublebyte_character_set(
		[Values(0xFFFF, 437, 850)] int testCodePageID,
		[Values((CountryCode)0xFFFF, CountryCode.UnitedStates, CountryCode.CanadianFrench)] CountryCode testCountryCode)
	{
		InternationalizationTableTest(
			table: Interrupt0x21.Function65.GetDoubleByteCharacterSet,
			testCodePageID,
			testCountryCode,
			getExpectedTable: culture => CharacterTables.GetDoubleByteCharacterSet(culture));
	}

	[Test]
	public void ConvertCharacter_should_uppercase_character([Range(1, 254)] byte testCharacter)
	{
		// Arrange
		var machine = new Machine();

		machine.DOS.SetUpRunningProgramSegmentPrefix("");

		var uppercaseTable = CharacterTables.GetUppercaseTable(machine.DOS.CurrentCulture);

		byte expectedUppercaseCharacter = testCharacter;

		if (expectedUppercaseCharacter < 128)
		{
			if ((expectedUppercaseCharacter >= 'a') && (expectedUppercaseCharacter <= 'z'))
				expectedUppercaseCharacter -= 32;
		}
		else
			expectedUppercaseCharacter = uppercaseTable[expectedUppercaseCharacter - 128];

		var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

		var rin = new RegistersEx();

		rin.AX = (int)Interrupt0x21.Function.Function65 << 8;
		rin.AX |= (int)Interrupt0x21.Function65.ConvertCharacter;
		rin.DX = testCharacter;

		// Act
		var rout = sut.Execute(rin);

		// Assert
		int actualUppercaseCharacter = rout.DX & 0xFF;

		actualUppercaseCharacter.Should().Be(expectedUppercaseCharacter);
	}

	[Test]
	public void ConvertString_should_uppercase_entire_string()
	{
		InternationalizationStringUppercaseTest(
			subfunction: Interrupt0x21.Function65.ConvertString);
	}

	[Test]
	public void ConvertASCIIZString_should_uppercase_entire_string()
	{
		InternationalizationStringUppercaseTest(
			subfunction: Interrupt0x21.Function65.ConvertASCIIZString);
	}

	[Test]
	public void GetGlobalCodePage_should_return_current_code_page()
	{
		// Arrange
		var machine = new Machine();

		var currentCodePageID = (ushort)machine.DOS.CurrentCulture.TextInfo.OEMCodePage;

		var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

		var rin = new RegistersEx();

		rin.AX = (int)Interrupt0x21.Function.Function66 << 8;
		rin.AX |= (int)Interrupt0x21.Function66.GetGlobalCodePage;

		// Act
		var rout = sut.Execute(rin);

		// Assert
		rout.FLAGS.Should().NotHaveFlag(Flags.Carry);

		rout.BX.Should().Be(currentCodePageID);
		rout.DX.Should().Be(currentCodePageID);
	}

	[Test]
	public void SetGlobalCodePage_should_set_current_code_page()
	{
		// Arrange
		var machine = new Machine();

		var currentCodePageID = (ushort)machine.DOS.CurrentCulture.TextInfo.OEMCodePage;

		var testCodePageID = currentCodePageID == 850 ? 437 : 850;

		var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

		var rin = new RegistersEx();

		rin.AX = (int)Interrupt0x21.Function.Function66 << 8;
		rin.AX |= (int)Interrupt0x21.Function66.SetGlobalCodePage;
		rin.BX = (ushort)testCodePageID;

		// Act
		var rout = sut.Execute(rin);

		// Assert
		rout.FLAGS.Should().NotHaveFlag(Flags.Carry);

		machine.DOS.CurrentCulture.TextInfo.OEMCodePage.Should().Be(testCodePageID);
	}

	[Test, Sequential]
	public void SetMaximumHandleCount_should_set_handle_limit(
		[Values(10, 100, 1000)] int testRequestLimit,
		[Values(20, 100, 1000)] int expectedLimit)
	{
		// Arrange
		var machine = new Machine();

		// Act
		var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

		var rin = new RegistersEx();

		rin.AX = (int)Interrupt0x21.Function.SetMaximumHandleCount << 8;
		rin.BX = (ushort)testRequestLimit;

		// Act
		var rout = sut.Execute(rin);

		// Assert
		machine.DOS.MaxFiles.Should().Be(expectedLimit);
	}

	/*
	public enum Function : byte
	{
		CommitFile = 0x68,
		CommitFile2 = 0x6A,
		ExtendedOpenCreate = 0x6C,
	}
	 */

	#region Common Test Methods
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

		object eventQueuedSync = new object();
		bool eventQueued = false;

		SDL.Keymod modState = 0;

		machine.Keyboard.GetModStateTestHook += () => modState;

		void QueueEvent()
		{
			lock (eventQueuedSync)
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

		bool eventQueuedOnReturn;

		lock (eventQueuedSync)
			eventQueuedOnReturn = eventQueued;

		// Assert
		rout.Should().NotBeNull();

		byte characterRead = (byte)(rout.AX & 0xFF);

		eventQueuedOnReturn.Should().BeTrue();
		captureBuffer.ToString().Should().Be(expectedOutput);
		breakEventOccurred.Should().Be(shouldBreak);
		characterRead.Should().Be(expectedCharacterRead);
		machine.DOS.LastError.Should().Be(DOSError.None);
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

	static readonly DateTime Epoch = new DateTime(1980, 1, 1);
	static readonly TimeSpan DateRange = new DateTime(1980 + 127, 12, 31, 23, 59, 59) - Epoch;

	void FileDateTimeTest(
		Interrupt0x21.Function57 subfunction,
		Action<DateTime, string, Registers> arrange,
		Func<string, Registers, DateTime> assert)
	{
		// Arrange
		using (var workspace = new TemporaryDirectory())
		{
			Environment.CurrentDirectory = workspace.Path;

			var machine = new Machine();

			machine.DOS.SetUpRunningProgramSegmentPrefix("");

			var testDateTime = Epoch.AddSeconds(TestContext.CurrentContext.Random.NextDouble() * DateRange.TotalSeconds);

			const string TestFileName = "TEST1.TXT";

			File.WriteAllText(TestFileName, "");

			int fileHandle = machine.DOS.OpenFile(TestFileName, QBX.OperatingSystem.FileStructures.FileMode.Open, OpenMode.Access_ReadWrite | OpenMode.Share_DenyNone);

			try
			{
				var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

				var rin = new RegistersEx();

				rin.AX = (int)Interrupt0x21.Function.Function57 << 8;
				rin.AX |= (ushort)subfunction;
				rin.BX = (ushort)fileHandle;

				arrange(testDateTime, TestFileName, rin);

				// Act
				var rout = sut.Execute(rin);

				// Assert
				rout.FLAGS.Should().NotHaveFlag(Flags.Carry);

				var resultDateTimeUTC = assert(TestFileName, rout);

				resultDateTimeUTC.Should().BeCloseTo(testDateTime, precision: TimeSpan.FromSeconds(2));
			}
			finally
			{
				machine.DOS.CloseFile(fileHandle);
			}
		}
	}

	void InternationalizationTableTest(
		Interrupt0x21.Function65 table,
		int testCodePageID,
		CountryCode testCountryCode,
		Func<CultureInfo, byte[]> getExpectedTable)
	{
		// Arrange
		var machine = new Machine();

		machine.DOS.SetUpRunningProgramSegmentPrefix("");

		const int BufferSize = 5; // table identifier plus long pointer

		int bufferAddress = machine.DOS.MemoryManager.AllocateMemory(BufferSize, machine.DOS.CurrentPSPSegment);

		var bufferSpan = machine.SystemMemory.AsSpan().Slice(bufferAddress, BufferSize);

		CultureInfo expectedCulture;

		if (testCodePageID == 0xFFFF) // current
			expectedCulture = machine.DOS.CurrentCulture;
		else
		{
			expectedCulture = CultureUtility.GetCultureInfoForCodePageAndCountry(testCodePageID, testCountryCode)
				?? throw new Exception($"Failed to retrieve CultureInfo for code page {testCodePageID} and country code {testCountryCode}");
		}

		var expectedTable = getExpectedTable(expectedCulture);

		var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

		var rin = new RegistersEx();

		rin.AX = (int)Interrupt0x21.Function.Function65 << 8;
		rin.AX |= (byte)table;
		rin.BX = (ushort)testCodePageID;
		rin.CX = BufferSize;
		rin.DX = (ushort)testCountryCode;
		rin.ES = (ushort)(bufferAddress / MemoryManager.ParagraphSize);
		rin.DI = (ushort)(bufferAddress % MemoryManager.ParagraphSize);

		// Act
		var rout = sut.Execute(rin);

		// Assert
		rout.FLAGS.Should().NotHaveFlag(Flags.Carry);

		bufferSpan[0].Should().Be((byte)table);

		var farPointer = MemoryMarshal.Cast<byte, ushort>(bufferSpan.Slice(1));

		var tableAddressSegmented = new SegmentedAddress(farPointer[1], farPointer[0]);

		int tableAddress = tableAddressSegmented.ToLinearAddress();

		var tableHeader = machine.SystemMemory.AsSpan().Slice(tableAddress, 2);

		int tableSize = tableHeader[0] + (tableHeader[1] << 8);

		var tableSpan = machine.SystemMemory.AsSpan().Slice(tableAddress + 2, tableSize);

		tableSpan.ShouldMatch(expectedTable);
	}

	void InternationalizationStringUppercaseTest(Interrupt0x21.Function65 subfunction)
	{
		// Arrange
		var machine = new Machine();

		machine.DOS.SetUpRunningProgramSegmentPrefix("");

		var testString = new StringValue();

		for (byte b = 1; b <= 254; b++)
			testString.Append(b);
		testString.Append(0); // ignored for ConvertString

		int bufferAddress = machine.DOS.MemoryManager.AllocateMemory(testString.Length, machine.DOS.CurrentPSPSegment);

		var bufferSpan = machine.SystemMemory.AsSpan().Slice(bufferAddress, testString.Length);

		testString.AsSpan().CopyTo(bufferSpan);

		var uppercaseTable = CharacterTables.GetUppercaseTable(machine.DOS.CurrentCulture);

		var expectedResult = new StringValue(testString);

		for (int i = 0; i < expectedResult.Length; i++)
		{
			byte b = expectedResult[i];

			if ((b >= 'a') && (b <= 'z'))
				b -= 32;
			else if (b >= 128)
				b = uppercaseTable[b - 128];

			expectedResult[i] = b;
		}

		var sut = machine.InterruptHandlers[0x21] ?? throw new Exception("Internal error");

		var rin = new RegistersEx();

		rin.AX = (int)Interrupt0x21.Function.Function65 << 8;
		rin.AX |= (byte)subfunction;
		rin.CX = (ushort)(testString.Length - 1); // ignored for ConvertASCIIZString
		rin.DS = unchecked((ushort)(bufferAddress / MemoryManager.ParagraphSize));
		rin.DX = unchecked((ushort)(bufferAddress % MemoryManager.ParagraphSize));

		// Act
		var rout = sut.Execute(rin);

		// Assert
		rout.FLAGS.Should().NotHaveFlag(Flags.Carry);

		bufferSpan.ShouldMatch(expectedResult.AsSpan());
	}
	#endregion

	#region Keyboard Input Simulator
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
	#endregion

	#region Test Data Helpers
	static Dictionary<Type, object[]> s_cachedEnumDomains = new Dictionary<Type, object[]>();

	static TEnum RandomEnumValue<TEnum>()
	{
		var enumType = typeof(TEnum);

		if (!s_cachedEnumDomains.TryGetValue(enumType, out var enumDomain))
		{
			TEnum[] values = (TEnum[])Enum.GetValues(typeof(TEnum));

			enumDomain = values.Cast<object>().ToArray();

			s_cachedEnumDomains[enumType] = enumDomain;
		}

		int index = TestContext.CurrentContext.Random.Next(0, enumDomain.Length);

		return (TEnum)enumDomain[index];
	}
	#endregion

	#region File Assertions
	static bool IsSameFile(string path1, string path2)
	{
		if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			return IsSameFile(path1, path2, new FileIndexProvider());
		else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
			return IsSameFile(path1, path2, new LinuxINodeProvider());
		else if (RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD))
			return IsSameFile(path1, path2, new FreeBSDINodeProvider());
		else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
			return IsSameFile(path1, path2, new OSXINodeProvider());
		else
			return Path.GetFullPath(path1) == Path.GetFullPath(path2);
	}

	static bool IsSameFile<TINode>(string path1, string path2, INodeProvider<TINode> inodeProvider)
		where TINode : INode<TINode>
	{
		return
			inodeProvider.TryGetINode(path1, out var inode1) &&
			inodeProvider.TryGetINode(path2, out var inode2) &&
			inode1.IsSameVolumeAndFileAs(inode2);
	}
	#endregion
}
