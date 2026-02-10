using System;

using QBX.ExecutionEngine.Execution;
using QBX.Hardware;

namespace QBX.OperatingSystem.Memory;

public class MemoryManager
{
	public const int ParagraphSize = 16;

	SystemMemory _systemMemory;
	int _offset;

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

		var rootEnvironment = new StringValue();

		foreach (System.Collections.DictionaryEntry variable in Environment.GetEnvironmentVariables(EnvironmentVariableTarget.Process))
		{
			if (variable.Key?.ToString() is string key)
			{
				rootEnvironment.Append(key).Append('=');

				if (variable.Value?.ToString() is string value)
					rootEnvironment.Append(value);

				rootEnvironment.Append(0);
			}
		}

		rootEnvironment.Append(0);

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
	{
		if (ownerPSPSegment == MemoryControlBlock.FreeBlockOwner)
			throw new ArgumentException(nameof(ownerPSPSegment));

		ConsolidateFreeBlocks();

		int scanOffset = _offset;

		ref ProgramSegmentPrefix psp = ref ProgramSegmentPrefix.CreateReference(_systemMemory, ownerPSPSegment * ParagraphSize);

		while (true)
		{
			ref MemoryControlBlock mcb = ref MemoryControlBlock.CreateReference(_systemMemory, scanOffset);

			if (!mcb.HasValidType)
				throw new DOSException(DOSError.ArenaTrashed);

			if ((mcb.OwnerPSPSegment == MemoryControlBlock.FreeBlockOwner)
			 || (mcb.SizeInBytes >= length))
			{
				mcb.OwnerPSPSegment = ownerPSPSegment;
				mcb.ProgramName.Set(psp.Reserved_ProgramName);

				int remainingBytes = mcb.SizeInBytes - length;

				// Split the control block if there are enough bytes for a subsequent allocation.
				if (remainingBytes >= 32)
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

				return scanOffset + ParagraphSize;
			}

			if (mcb.Type == MemoryControlBlockType.LastNode)
				throw new OutOfMemoryException();

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
			psp.FCB1.ParseFileName(commandLine.AsSpan().Slice(firstArgumentStart, firstArgumentEnd - firstArgumentStart + 1));

			int secondArgumentStart = firstArgumentStart + 1;

			while ((secondArgumentStart < commandLine.Length) && (commandLine[secondArgumentStart] == (byte)' '))
				secondArgumentStart++;

			int secondArgumentEnd = secondArgumentStart;

			while ((secondArgumentEnd + 1 < commandLine.Length) && (commandLine[secondArgumentEnd + 1] != (byte)' '))
				secondArgumentEnd++;

			if (secondArgumentStart < commandLine.Length)
				psp.FCB2.ParseFileName(commandLine.AsSpan().Slice(secondArgumentStart, secondArgumentEnd - secondArgumentStart + 1));
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
