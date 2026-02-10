using System.Runtime.InteropServices;

namespace QBX.OperatingSystem.Memory;

public static class MemoryControlBlockExtensions
{
	public static ref MemoryControlBlock Next(this ref MemoryControlBlock mcb)
	{
		var span = MemoryMarshal.CreateSpan(ref mcb, 2 + mcb.SizeInParagraphs);

		return ref span[span.Length - 1];
	}
}
