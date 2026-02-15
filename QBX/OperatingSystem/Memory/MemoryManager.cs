using System;

using QBX.ExecutionEngine.Execution;
using QBX.Hardware;
using QBX.OperatingSystem.Processes;

namespace QBX.OperatingSystem.Memory;

public class MemoryManager
{
	public const int ParagraphSize = 16;

	SystemMemory _systemMemory;
	int _offset;

	public MemoryAllocationStrategy AllocationStrategy = MemoryAllocationStrategy.FirstFitLow;

	ushort _rootPSPSegment;

	public ushort RootPSPSegment => _rootPSPSegment;

	public MemoryManager(SystemMemory systemMemory, int offset, int length)
	{
		_systemMemory = systemMemory;
		_offset = offset;

		ref MemoryControlBlock mcb = ref MemoryControlBlock.CreateReference(_systemMemory, _offset);

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
			throw new OutOfMemoryException();

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

				if ((mcb.SizeInBytes >= length) && (mcb.SizeInBytes < bestFitSoFar))
				{
					if (((strategy == MemoryAllocationStrategy.LastFit) && (mcb.Type == MemoryControlBlockType.HasNextNode))
					 || ((strategy == MemoryAllocationStrategy.BestFit) && (mcb.SizeInParagraphs > lengthInParagraphs)))
					{
						int nextBlockAddress = scanOffset + ParagraphSize + mcb.SizeInBytes;

						int laterMatch = ScanForFreeBlockAndAllocate(nextBlockAddress, length, ownerPSPSegment, strategy, bestFitSoFar: mcb.SizeInBytes, ref largestBlockSize);

						if (laterMatch >= 0)
							return laterMatch;
					}

					int remainingBytes = mcb.SizeInBytes - length;

					// Split the control block if there are enough bytes for a subsequent allocation.
					if (remainingBytes >= 32)
					{
						if (strategy == MemoryAllocationStrategy.FirstFit)
						{
							var nodeType = mcb.Type;

							mcb.SizeInParagraphs = (ushort)((length + ParagraphSize - 1) / ParagraphSize);
							mcb.Type = MemoryControlBlockType.HasNextNode;

							ref MemoryControlBlock mcbNext = ref mcb.Next();

							mcbNext.Type = nodeType;
							mcbNext.OwnerPSPSegment = MemoryControlBlock.FreeBlockOwner;
							mcbNext.SizeInParagraphs = (ushort)((remainingBytes - 1) / ParagraphSize);
							mcbNext.ProgramName.Clear();
						}
						else
						{
							// Last fit; keep unallocated space at the start of this chunk.
							var nodeType = mcb.Type;

							mcb.OwnerPSPSegment = MemoryControlBlock.FreeBlockOwner;
							mcb.SizeInParagraphs = (ushort)((remainingBytes - 1) / ParagraphSize);
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

	private void ConsolidateFreeBlocks()
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
					continue;
				}
			}

			scanOffset = scanOffset + ParagraphSize + mcb.SizeInBytes;
		}
	}

	public void ResizeAllocation(int address, int newSize, out int largestBlockSize)
	{
		ref MemoryControlBlock mcb = ref MemoryControlBlock.CreateReference(_systemMemory, address - 16);

		if (!mcb.HasValidType)
			throw new DOSException(DOSError.ArenaTrashed);

		if ((mcb.Type == MemoryControlBlockType.LastNode) || (newSize <= mcb.SizeInBytes))
		{
			largestBlockSize = mcb.SizeInBytes;

			if (newSize > mcb.SizeInBytes)
				throw new DOSException(DOSError.NotEnoughMemory);

			int freedBytes = mcb.SizeInBytes - newSize;

			if (freedBytes >= 32)
			{
				mcb.SizeInParagraphs = (ushort)((newSize + ParagraphSize - 1) / ParagraphSize);
				mcb.Type = MemoryControlBlockType.HasNextNode;

				ref MemoryControlBlock mcbNext = ref mcb.Next();

				mcbNext.Type = MemoryControlBlockType.LastNode;
				mcbNext.OwnerPSPSegment = MemoryControlBlock.FreeBlockOwner;
				mcbNext.SizeInParagraphs = (ushort)((freedBytes - 1) / ParagraphSize);
				mcbNext.ProgramName.Clear();
			}
		}
		else
		{
			ref MemoryControlBlock mcbNext = ref mcb.Next();

			if (mcbNext.OwnerPSPSegment == MemoryControlBlock.FreeBlockOwner)
				largestBlockSize = mcb.SizeInParagraphs + 1 + mcbNext.SizeInParagraphs;
			else
				largestBlockSize = mcb.SizeInParagraphs;

			if (newSize > largestBlockSize)
				throw new DOSException(DOSError.NotEnoughMemory);

			// First, subsume the next block
			mcb.SizeInParagraphs = (ushort)(largestBlockSize / ParagraphSize);

			// Then, subdivide back if possible.
			int remainingBytes = mcb.SizeInBytes - newSize;

			if (remainingBytes >= 32)
			{
				mcb.SizeInParagraphs = (ushort)((newSize + ParagraphSize - 1) / ParagraphSize);

				ref MemoryControlBlock mcbNewNext = ref mcb.Next();

				mcbNewNext.Type = MemoryControlBlockType.HasNextNode;
				mcbNewNext.OwnerPSPSegment = MemoryControlBlock.FreeBlockOwner;
				mcbNewNext.SizeInParagraphs = (ushort)((remainingBytes - 1) / ParagraphSize);
				mcbNewNext.ProgramName.Clear();
			}
		}
	}

	public void FreeMemory(int address)
	{
		ref MemoryControlBlock mcb = ref MemoryControlBlock.CreateReference(_systemMemory, address - 16);

		if (!mcb.HasValidType)
			throw new DOSException(DOSError.ArenaTrashed);

		mcb.OwnerPSPSegment = MemoryControlBlock.FreeBlockOwner;
		mcb.ProgramName.Clear();
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
			try
			{
				psp.FCB1.ParseFileName(commandLine.AsSpan().Slice(firstArgumentStart, firstArgumentEnd - firstArgumentStart + 1));
			}
			catch { }

			int secondArgumentStart = firstArgumentStart + 1;

			while ((secondArgumentStart < commandLine.Length) && (commandLine[secondArgumentStart] == (byte)' '))
				secondArgumentStart++;

			int secondArgumentEnd = secondArgumentStart;

			while ((secondArgumentEnd + 1 < commandLine.Length) && (commandLine[secondArgumentEnd + 1] != (byte)' '))
				secondArgumentEnd++;

			if (secondArgumentStart < commandLine.Length)
			{
				try
				{
					psp.FCB2.ParseFileName(commandLine.AsSpan().Slice(secondArgumentStart, secondArgumentEnd - secondArgumentStart + 1));
				}
				catch { }
			}
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
