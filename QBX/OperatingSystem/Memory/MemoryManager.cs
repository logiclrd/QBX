using System;

using QBX.ExecutionEngine.Execution;
using QBX.Hardware;
using QBX.OperatingSystem.Processes;

namespace QBX.OperatingSystem.Memory;

public class MemoryManager : IMemoryManager
{
	public const int ParagraphSize = 16;

	SystemMemory _systemMemory;
	int _offset;

	public MemoryAllocationStrategy AllocationStrategy { get; set; } = MemoryAllocationStrategy.FirstFitLow;

	ushort _rootPSPSegment;

	public ushort RootPSPSegment => _rootPSPSegment;

	internal ref MemoryControlBlock GetFirstMemoryControlBlock()
		=> ref MemoryControlBlock.CreateReference(_systemMemory, _offset);

	public MemoryManager(SystemMemory systemMemory, int offset, int length)
	{
		_systemMemory = systemMemory;
		_offset = offset;

		ref MemoryControlBlock mcb = ref GetFirstMemoryControlBlock();

		mcb.Type = MemoryControlBlockType.LastNode;
		mcb.SizeInParagraphs = (ushort)Math.Min(ushort.MaxValue, (length - ParagraphSize) / ParagraphSize);
		mcb.OwnerPSPSegment = 0;
		mcb.ProgramName.Clear();

		var rootEnvironmentBlock = EnvironmentBlock.FromAmbientEnvironment();

		var rootEnvironment = new StringValue();

		rootEnvironmentBlock.EncodeTo(rootEnvironment);

		_rootPSPSegment = MemoryControlBlock.FreeBlockOwner;
		_rootPSPSegment ^= 0x0101; // just need a value that won't be mistaken for a free block in the chain

		int rootEnvironmentAddress = AllocateMemory(rootEnvironment.Length, _rootPSPSegment);

		rootEnvironment.AsSpan().CopyTo(_systemMemory.AsSpan().Slice(rootEnvironmentAddress));

		ushort rootEnvironmentSegment = (ushort)(rootEnvironmentAddress / ParagraphSize);

		CreateRootPSP(rootEnvironmentSegment);

		ref MemoryControlBlock environmentMCB = ref MemoryControlBlock.CreateReference(_systemMemory, rootEnvironmentAddress - ParagraphSize);

		environmentMCB.OwnerPSPSegment = _rootPSPSegment;
	}

	public int AllocateMemory(int length, ushort ownerPSPSegment)
		=> AllocateMemory(length, ownerPSPSegment, out _);

	public int AllocateMemory(int length, ushort ownerPSPSegment, out int largestBlockSize)
	{
		if (ownerPSPSegment == MemoryControlBlock.FreeBlockOwner)
			throw new ArgumentException(nameof(ownerPSPSegment));

		ConsolidateFreeBlocks();

		int scanOffset = _offset;

		var strategy = AllocationStrategy & MemoryAllocationStrategy.StrategyMask;

		largestBlockSize = 0;

		int allocatedAddress = ScanForFreeBlockAndAllocate(_offset, length, ownerPSPSegment, strategy, bestFitSoFar: int.MaxValue, ref largestBlockSize);

		if (allocatedAddress < 0)
			throw new DOSException(DOSError.NotEnoughMemory);

		return allocatedAddress;
	}

	int ScanForFreeBlockAndAllocate(int scanOffset, int length, ushort ownerPSPSegment, MemoryAllocationStrategy strategy, int bestFitSoFar, ref int largestBlockSize)
	{
		int lengthInParagraphs = (length + ParagraphSize - 1) / ParagraphSize;

		while (true)
		{
			ref MemoryControlBlock mcb = ref MemoryControlBlock.CreateReference(_systemMemory, scanOffset);

			if (!mcb.HasValidType)
				throw new DOSException(DOSError.ArenaTrashed);

			if (mcb.OwnerPSPSegment == MemoryControlBlock.FreeBlockOwner)
			{
				largestBlockSize = Math.Max(largestBlockSize, mcb.SizeInBytes);

				if ((mcb.SizeInBytes >= length)
				 && ((mcb.SizeInBytes < bestFitSoFar) || (strategy != MemoryAllocationStrategy.BestFit)))
				{
					if ((mcb.Type == MemoryControlBlockType.HasNextNode)
					 && ((strategy == MemoryAllocationStrategy.LastFit)
					  || ((strategy == MemoryAllocationStrategy.BestFit) && (mcb.SizeInParagraphs > lengthInParagraphs))))
					{
						int nextBlockAddress = scanOffset + ParagraphSize + mcb.SizeInBytes;

						int laterMatch = ScanForFreeBlockAndAllocate(nextBlockAddress, length, ownerPSPSegment, strategy, bestFitSoFar: mcb.SizeInBytes, ref largestBlockSize);

						if (laterMatch >= 0)
							return laterMatch;
					}

					int remainingBytes = mcb.SizeInBytes - length;
					int remainingParagraphs = remainingBytes / ParagraphSize;

					// Split the control block if there are enough bytes for a subsequent allocation.
					if (remainingBytes >= 32)
					{
						if ((strategy == MemoryAllocationStrategy.FirstFit)
						 || (strategy == MemoryAllocationStrategy.BestFit))
						{
							var nodeType = mcb.Type;

							mcb.SizeInParagraphs = (ushort)((length + ParagraphSize - 1) / ParagraphSize);
							mcb.Type = MemoryControlBlockType.HasNextNode;

							ref MemoryControlBlock mcbNext = ref mcb.Next();

							mcbNext.Type = nodeType;
							mcbNext.OwnerPSPSegment = MemoryControlBlock.FreeBlockOwner;
							mcbNext.SizeInParagraphs = (ushort)(remainingParagraphs - 1);
							mcbNext.ProgramName.Clear();
						}
						else
						{
							// Last fit; keep unallocated space at the start of this chunk.
							var nodeType = mcb.Type;

							mcb.SizeInParagraphs = (ushort)(remainingParagraphs - 1);
							mcb.ProgramName.Clear();
							mcb.Type = MemoryControlBlockType.HasNextNode;

							scanOffset = scanOffset + ParagraphSize + mcb.SizeInBytes;
							mcb = ref mcb.Next();

							mcb.Type = nodeType;
							mcb.SizeInParagraphs = (ushort)((length + ParagraphSize - 1) / ParagraphSize);
						}
					}

					ref ProgramSegmentPrefix psp = ref ProgramSegmentPrefix.CreateReference(_systemMemory, ownerPSPSegment * ParagraphSize);

					mcb.OwnerPSPSegment = ownerPSPSegment;
					mcb.ProgramName.Set(psp.Reserved_ProgramName);

					return scanOffset + ParagraphSize;
				}
			}

			if (mcb.Type == MemoryControlBlockType.LastNode)
				return -1;

			scanOffset = scanOffset + ParagraphSize + mcb.SizeInBytes;
		}
	}

	internal void ConsolidateFreeBlocks()
	{
		int scanOffset = _offset;

		while (true)
		{
			ref MemoryControlBlock mcb = ref MemoryControlBlock.CreateReference(_systemMemory, scanOffset);

			if (!mcb.HasValidType)
				throw new DOSException(DOSError.ArenaTrashed);

			if (mcb.Type == MemoryControlBlockType.LastNode)
				break;

			ref MemoryControlBlock mcbNext = ref mcb.Next();

			if (mcb.IsFree && mcbNext.IsFree)
			{
				int newSize = mcb.SizeInParagraphs + 1 + mcbNext.SizeInParagraphs;

				if (newSize <= ushort.MaxValue)
				{
					mcb.SizeInParagraphs = (ushort)newSize;
					mcb.Type = mcbNext.Type;
					continue;
				}
			}

			scanOffset = scanOffset + ParagraphSize + mcb.SizeInBytes;
		}
	}

	public void ResizeAllocation(int address, int newSize, out int largestBlockSize)
	{
		if (newSize < 0)
			throw new ArgumentOutOfRangeException(nameof(newSize));

		ConsolidateFreeBlocks();

		int scanOffset = _offset;

		int newSizeInParagraphs = (newSize + ParagraphSize - 1) / ParagraphSize;

		while (true)
		{
			ref MemoryControlBlock mcb = ref MemoryControlBlock.CreateReference(_systemMemory, scanOffset);

			if (!mcb.HasValidType)
				throw new DOSException(DOSError.ArenaTrashed);

			int mcbBlockAddress = scanOffset + ParagraphSize;

			if (mcbBlockAddress == address)
			{
				// Found it!
				if (mcb.IsFree)
					throw new DOSException(DOSError.InvalidBlock); // but wait

				if ((mcb.Type == MemoryControlBlockType.LastNode) || (newSize <= mcb.SizeInBytes))
				{
					largestBlockSize = mcb.SizeInBytes;

					if (newSizeInParagraphs > mcb.SizeInParagraphs)
						throw new DOSException(DOSError.NotEnoughMemory);

					int freedParagraphs = mcb.SizeInParagraphs - newSizeInParagraphs;

					if (freedParagraphs >= 2)
					{
						mcb.SizeInParagraphs = (ushort)newSizeInParagraphs;

						ref MemoryControlBlock mcbNext = ref mcb.Next();

						mcbNext.Type = mcb.Type;
						mcbNext.OwnerPSPSegment = MemoryControlBlock.FreeBlockOwner;
						mcbNext.SizeInParagraphs = (ushort)(freedParagraphs - 1);
						mcbNext.ProgramName.Clear();

						mcb.Type = MemoryControlBlockType.HasNextNode;
					}
				}
				else
				{
					ref MemoryControlBlock mcbNext = ref mcb.Next();

					int largestBlockSizeInParagraphs;

					if (mcbNext.OwnerPSPSegment == MemoryControlBlock.FreeBlockOwner)
						largestBlockSizeInParagraphs = mcb.SizeInParagraphs + 1 + mcbNext.SizeInParagraphs;
					else
						largestBlockSizeInParagraphs = mcb.SizeInParagraphs;

					largestBlockSize = largestBlockSizeInParagraphs * ParagraphSize;

					if (newSizeInParagraphs > largestBlockSizeInParagraphs)
						throw new DOSException(DOSError.NotEnoughMemory);

					// First, subsume the next block
					mcb.Type = mcbNext.Type;
					mcb.SizeInParagraphs = (ushort)largestBlockSizeInParagraphs;

					// Then, subdivide back if possible.
					int remainingParagraphs = mcb.SizeInParagraphs - newSizeInParagraphs;

					if (remainingParagraphs >= 2)
					{
						mcb.SizeInParagraphs = (ushort)newSizeInParagraphs;

						ref MemoryControlBlock mcbNewNext = ref mcb.Next();

						mcbNewNext.Type = mcb.Type;
						mcbNewNext.OwnerPSPSegment = MemoryControlBlock.FreeBlockOwner;
						mcbNewNext.SizeInParagraphs = (ushort)(remainingParagraphs - 1);
						mcbNewNext.ProgramName.Clear();

						mcb.Type = MemoryControlBlockType.HasNextNode;
					}
				}

				return;
			}

			if (mcbBlockAddress > address)
				break; // definitely won't find it

			if (mcb.Type == MemoryControlBlockType.LastNode)
				break;

			scanOffset = scanOffset + ParagraphSize + mcb.SizeInBytes;
		}

		throw new DOSException(DOSError.InvalidBlock);
	}

	public void FreeMemory(int address)
	{
		int scanOffset = _offset;

		while (true)
		{
			ref MemoryControlBlock mcb = ref MemoryControlBlock.CreateReference(_systemMemory, scanOffset);

			if (!mcb.HasValidType)
				throw new DOSException(DOSError.ArenaTrashed);

			int mcbBlockAddress = scanOffset + ParagraphSize;

			if (mcbBlockAddress == address)
			{
				// Found it!
				if (mcb.IsFree)
					throw new DOSException(DOSError.InvalidBlock); // but wait

				mcb.OwnerPSPSegment = MemoryControlBlock.FreeBlockOwner;
				mcb.ProgramName.Clear();

				return;
			}

			if (mcbBlockAddress > address)
				break; // definitely won't find it

			if (mcb.Type == MemoryControlBlockType.LastNode)
				break;

			scanOffset = scanOffset + ParagraphSize + mcb.SizeInBytes;
		}

		throw new DOSException(DOSError.InvalidBlock);
	}

	void CreateRootPSP(ushort environmentSegment)
	{
		_rootPSPSegment = MemoryControlBlock.FreeBlockOwner;
		_rootPSPSegment ^= 0x0101; // just need a value that won't be mistaken for a free block in the chain

		int address = CreatePSP(environmentSegment, new StringValue());

		_rootPSPSegment = (ushort)(address / ParagraphSize);

		ref MemoryControlBlock mcb = ref MemoryControlBlock.CreateReference(_systemMemory, address - ParagraphSize);

		mcb.OwnerPSPSegment = _rootPSPSegment;
	}

	public ushort CreatePSP(EnvironmentBlock environment, StringValue commandLine)
	{
		var environmentBytes = environment.Encode();

		if (environmentBytes.Length > 32768)
			throw new DOSException(DOSError.BadEnvironment);

		int allocation = AllocateMemory(environmentBytes.Length, RootPSPSegment);

		environmentBytes.AsSpan().CopyTo(_systemMemory.AsSpan().Slice(allocation));

		ushort allocationSegment = (ushort)(allocation / ParagraphSize);

		return CreatePSP(allocationSegment, commandLine);
	}

	public ushort CreatePSP(ushort environmentSegment, StringValue commandLine)
	{
		int address = AllocateMemory(length: 256, ownerPSPSegment: _rootPSPSegment);

		ref ProgramSegmentPrefix psp = ref ProgramSegmentPrefix.CreateReference(_systemMemory, address);

		psp.Int20hInstruction = ProgramSegmentPrefix.Int20hInstructionValue;
		psp.NextSegment = 0; // will have to know what, if anything, anybody uses this for in order to figure out how to emulate it
		psp.EnvironmentSegment = environmentSegment;

		int firstArgumentStart = 0;

		while ((firstArgumentStart < commandLine.Length) && (commandLine[firstArgumentStart] == (byte)' '))
			firstArgumentStart++;

		int firstArgumentEnd = firstArgumentStart;

		while ((firstArgumentEnd + 1 < commandLine.Length) && (commandLine[firstArgumentEnd + 1] != (byte)' '))
			firstArgumentEnd++;

		if (firstArgumentStart < commandLine.Length)
		{
			psp.FCB1.TryParseFileName(commandLine.AsSpan().Slice(firstArgumentStart, firstArgumentEnd - firstArgumentStart + 1));

			int secondArgumentStart = firstArgumentEnd + 1;

			while ((secondArgumentStart < commandLine.Length) && (commandLine[secondArgumentStart] == (byte)' '))
				secondArgumentStart++;

			int secondArgumentEnd = secondArgumentStart;

			while ((secondArgumentEnd + 1 < commandLine.Length) && (commandLine[secondArgumentEnd + 1] != (byte)' '))
				secondArgumentEnd++;

			if (secondArgumentStart < commandLine.Length)
				psp.FCB2.TryParseFileName(commandLine.AsSpan().Slice(secondArgumentStart, secondArgumentEnd - secondArgumentStart + 1));
		}

		var commandLineBytes = commandLine.AsSpan();

		if (commandLineBytes.Length > 126)
			commandLineBytes = commandLineBytes.Slice(0, 126);

		psp.CommandLineLength = (byte)commandLineBytes.Length;

		commandLineBytes.CopyTo(psp.CommandLine);
		psp.CommandLine[commandLineBytes.Length] = (byte)'\r';

		ushort pspSegment = (ushort)(address / ParagraphSize);

		return pspSegment;
	}

	public void FreePSP(ushort pspSegment)
	{
		FreeMemory(pspSegment * ParagraphSize);
	}
}
