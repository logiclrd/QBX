using System.Runtime.InteropServices;

using QBX.Hardware;

namespace QBX.OperatingSystem.Memory;

[StructLayout(LayoutKind.Explicit, Size = Size)]
public struct ProgramSegmentPrefix
{
	public const int Size = 256;

	public const ushort Int20hInstructionValue = 0x20CD;

	[FieldOffset(0)] public ushort Int20hInstruction; // old way to exit
	[FieldOffset(2)] public ushort NextSegment;
	//   4   1   (reserved)
	//   5   5   bytes for a far call to the DOS function dispatcher
	//  10   4   terminate address
	//  14   4   Ctrl-Break handler address
	//  18   4   Critical Error handler address
	//  22  22   DOS reserved area

	[FieldOffset(22)] public MemoryControlBlockProgramName Reserved_ProgramName;

	[FieldOffset(44)] public ushort EnvironmentSegment;
	//  46  46   DOS reserved area

	//  92  16   First 16 bytes of an FCB preconfigured on the assumption that the first command-line argument is a filename
	[FieldOffset(92)] public TruncatedFileControlBlock FCB1;
	// 108  16   First 16 bytes of an FCB preconfigured on the assumption that the second command-line argument is a filename
	[FieldOffset(108)] public TruncatedFileControlBlock FCB2;
	// 124   4   Padding (documented as being part of FCB2)
	[FieldOffset(128)] public byte CommandLineLength;
	[FieldOffset(129)] public ProgramSegmentPrefixCommandLineBytes CommandLine;

	// NB: If FCB1 is opened in-place, then FCB2 is overwritten.

	public static ref ProgramSegmentPrefix CreateReference(SystemMemory systemMemory, int offset)
	{
		var mcbBytes = systemMemory.AsSpan().Slice(offset, ProgramSegmentPrefix.Size);

		return ref MemoryMarshal.Cast<byte, ProgramSegmentPrefix>(mcbBytes)[0];
	}
}
