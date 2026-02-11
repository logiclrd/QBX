using QBX.Hardware;
using QBX.OperatingSystem.Memory;
using System.Runtime.InteropServices;

namespace QBX.OperatingSystem.FileStructures;

[StructLayout(LayoutKind.Explicit, Size = Size)]
public struct DriveParameterBlock
{
	public const int Size = 33;

	[FieldOffset(0)] public byte DriveIdentifier;
	[FieldOffset(1)] public byte Unit;
	[FieldOffset(2)] public ushort SectorSize;
	[FieldOffset(4)] public byte ClusterMask; // ClusterMask == (1 << ClusterShift) - 1
	[FieldOffset(5)] public byte ClusterShift;
	[FieldOffset(6)] public ushort FirstFAT;
	[FieldOffset(8)] public byte FATCount;
	[FieldOffset(9)] public ushort RootEntries;
	[FieldOffset(11)] public ushort FirstSector;
	[FieldOffset(13)] public ushort MaxCluster;
	[FieldOffset(15)] public ushort SectorsPerFAT;
	[FieldOffset(17)] public ushort DirectorySector;
	[FieldOffset(19)] public uint DeviceDriverAddress;
	[FieldOffset(23)] public byte MediaDescriptor;
	[FieldOffset(24)] public byte FirstAccess;
	[FieldOffset(25)] public SegmentedAddress NextDPBAddress;
	[FieldOffset(29)] public ushort NextFreeCluster;
	[FieldOffset(31)] public ushort FreeClusterCount;

	public static ref DriveParameterBlock CreateReference(SystemMemory systemMemory, int offset)
	{
		var dpbBytes = systemMemory.AsSpan().Slice(offset, Size);

		return ref MemoryMarshal.Cast<byte, DriveParameterBlock>(dpbBytes)[0];
	}
}
