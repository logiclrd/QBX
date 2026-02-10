using System.Runtime.InteropServices;

using QBX.Hardware;

namespace QBX.OperatingSystem.Memory;

[StructLayout(LayoutKind.Explicit, Size = MemoryManager.ParagraphSize)]
public struct MemoryControlBlock
{
	[FieldOffset(0)] public MemoryControlBlockType Type;
	[FieldOffset(1)] public ushort OwnerPSPSegment;
	[FieldOffset(3)] public ushort SizeInParagraphs;
	[FieldOffset(8)] public MemoryControlBlockProgramName ProgramName;

	public const ushort FreeBlockOwner = 0;

	public bool HasValidType => (Type == MemoryControlBlockType.HasNextNode) || (Type == MemoryControlBlockType.LastNode);

	public bool IsFree => (OwnerPSPSegment == FreeBlockOwner);

	public int SizeInBytes => SizeInParagraphs * MemoryManager.ParagraphSize;

	public static ref MemoryControlBlock CreateReference(SystemMemory systemMemory, int offset)
	{
		var mcbBytes = systemMemory.AsSpan().Slice(offset, MemoryManager.ParagraphSize);

		return ref MemoryMarshal.Cast<byte, MemoryControlBlock>(mcbBytes)[0];
	}
}
