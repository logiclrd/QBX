using QBX.ExecutionEngine.Execution;
using QBX.OperatingSystem.Processes;

namespace QBX.OperatingSystem.Memory;

public interface IMemoryManager
{
	ushort RootPSPSegment { get; }

	MemoryAllocationStrategy AllocationStrategy { get; set; }

	int AllocateMemory(int length, ushort ownerPSPSegment);
	int AllocateMemory(int length, ushort ownerPSPSegment, out int largestBlockSize);
	ushort CreatePSP(EnvironmentBlock environment, StringValue commandLine);
	ushort CreatePSP(ushort environmentSegment, StringValue commandLine);
	void FreeMemory(int address);
	void FreePSP(ushort pspSegment);
	void ResizeAllocation(int address, int newSize, out int largestBlockSize);
}
