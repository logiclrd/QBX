using QBX.ExecutionEngine.Execution;
using QBX.Hardware;
using QBX.OperatingSystem;
using QBX.OperatingSystem.FileStructures;
using QBX.OperatingSystem.Memory;
using QBX.OperatingSystem.Processes;

namespace QBX.Tests.OperatingSystem.Memory;

public class MemoryManagerTests
{
	[Test]
	public void Constructor_should_set_up_arena_properly()
	{
		// Arrange
		var machine = new Machine();

		var systemMemory = machine.SystemMemory;

		ref var startOfSystemMemory = ref MemoryControlBlock.CreateReference(systemMemory, 0);
		var startOfSystemMemorySpan = new Span<MemoryControlBlock>(ref startOfSystemMemory);

		// Act
		var sut = new MemoryManager(systemMemory, 0, systemMemory.Length);

		// Assert
		ref var firstMemoryControlBlock = ref sut.GetFirstMemoryControlBlock();
		var firstMemoryControlBlockSpan = new Span<MemoryControlBlock>(ref firstMemoryControlBlock);

		firstMemoryControlBlockSpan.ShouldBe(startOfSystemMemorySpan);

		// Walk the chain and find the first address after the last block.
		int offset = 0;

		while (true)
		{
			ref var mcb = ref MemoryControlBlock.CreateReference(systemMemory, offset);

			mcb.HasValidType.Should().BeTrue();

			offset = offset + MemoryManager.ParagraphSize + mcb.SizeInBytes;

			if (mcb.Type == QBX.OperatingSystem.MemoryControlBlockType.LastNode)
				break;
		}

		offset.Should().Be(systemMemory.Length);
	}

	[Test]
	public void AllocateMemory_should_allocate_matching_block_according_to_strategy(
		[Values(MemoryAllocationStrategy.FirstFit, MemoryAllocationStrategy.LastFit, MemoryAllocationStrategy.BestFit)]
		MemoryAllocationStrategy allocationStrategy)
	{
		// Arrange
		var machine = new Machine();

		var systemMemory = machine.SystemMemory;

		const int ArenaStart = 65536;
		const int ArenaSize = 131072;

		const int ArenaEnd = ArenaStart + ArenaSize;

		var sut = new MemoryManager(systemMemory, ArenaStart, ArenaSize);

		// Deliberately create fragmentation with blocks of known sizes.
		// [0] Free block (too small)
		// [1] Allocated block
		// [2] Free block (too large)
		// [3] Allocated block
		// [4] Free block (right size)
		// [5] Allocated block
		// [6] Free block (too large)
		//
		// First fit strategy should pick block 2
		// Best fit strategy should block block 4
		// Last fit strategy should pick block 6 and allocate from the end of the block

		const int OwnerPSPSegment = 1;

		const int AllocationSize = 1024;
		const int DeviationSize = 256;

		int block0 = sut.AllocateMemory(AllocationSize - DeviationSize, OwnerPSPSegment);
		int block1 = sut.AllocateMemory(1, OwnerPSPSegment);
		int block2 = sut.AllocateMemory(AllocationSize + DeviationSize, OwnerPSPSegment);
		int block3 = sut.AllocateMemory(1, OwnerPSPSegment);
		int block4 = sut.AllocateMemory(AllocationSize, OwnerPSPSegment);
		int block5 = sut.AllocateMemory(1, OwnerPSPSegment);
		int block6 = sut.AllocateMemory(AllocationSize + DeviationSize, OwnerPSPSegment);

		sut.FreeMemory(block0);
		sut.FreeMemory(block2);
		sut.FreeMemory(block4);
		sut.FreeMemory(block6);

		sut.AllocationStrategy = allocationStrategy;

		// Act
		var result = sut.AllocateMemory(AllocationSize, OwnerPSPSegment);

		// Assert
		switch (allocationStrategy)
		{
			case MemoryAllocationStrategy.FirstFit: result.Should().Be(block2); break;
			case MemoryAllocationStrategy.BestFit: result.Should().Be(block4); break;
			case MemoryAllocationStrategy.LastFit: result.Should().BeInRange(block6, ArenaEnd - AllocationSize); break;
		}

		VerifyMemoryControlBlockChain(
			systemMemory,
			ArenaStart,
			ArenaSize,
			findBlockAddress: result,
			allocationSize: AllocationSize,
			foundBlock: out bool foundBlock,
			foundCandidateAfterBlock: out bool foundCandidateAfterBlock);

		foundBlock.Should().BeTrue();

		if (allocationStrategy == MemoryAllocationStrategy.LastFit)
			foundCandidateAfterBlock.Should().BeFalse();
	}

	[Test]
	public void ConsolidateFreeBlocks_should_consolidate_the_appropriate_adjacent_blocks_only()
	{
		// Arrange
		var machine = new Machine();

		var systemMemory = machine.SystemMemory;

		const int ArenaStart = 65536;
		const int ArenaSize = 131072;

		var sut = new MemoryManager(systemMemory, ArenaStart, ArenaSize);

		// Prepare a bunch of free blocks for consolidation
		// [0] Free
		// [1] Free
		// [2] Allocated
		// [3] Free
		// [4] Allocated
		// [5] Free
		// [6] Free
		// [7] Free (implicit remaining space)

		const int OwnerPSPSegment = 1;

		int block0 = sut.AllocateMemory(384, OwnerPSPSegment);
		int block1 = sut.AllocateMemory(5182, OwnerPSPSegment);
		int block2 = sut.AllocateMemory(126, OwnerPSPSegment);
		int block3 = sut.AllocateMemory(6518, OwnerPSPSegment);
		int block4 = sut.AllocateMemory(5192, OwnerPSPSegment);
		int block5 = sut.AllocateMemory(6891, OwnerPSPSegment);
		int block6 = sut.AllocateMemory(569, OwnerPSPSegment);

		sut.FreeMemory(block0);
		sut.FreeMemory(block1);
		sut.FreeMemory(block3);
		sut.FreeMemory(block5);
		sut.FreeMemory(block6);

		var freeBlocksBefore = new List<(int Start, int Size)>();
		var freeBlocksAfter = new List<(int Start, int Size)>();

		CollectFreeBlocks(systemMemory, ArenaStart, freeBlocksBefore);

		var expectedFreeBlocksAfter = new List<(int Start, int Size)>(freeBlocksBefore);

		// Quick and dirty consolidation
		int index = 0;

		while (index + 1 < expectedFreeBlocksAfter.Count)
		{
			var thisBlock = expectedFreeBlocksAfter[index];
			var nextBlock = expectedFreeBlocksAfter[index + 1];

			int thisBlockEnd = thisBlock.Start + thisBlock.Size;

			if (thisBlockEnd == nextBlock.Start)
			{
				expectedFreeBlocksAfter[index] = (thisBlock.Start, thisBlock.Size + nextBlock.Size);
				expectedFreeBlocksAfter.RemoveAt(index + 1);
			}
			else
				index++;
		}

		// Act
		sut.ConsolidateFreeBlocks();

		// Assert
		CollectFreeBlocks(systemMemory, ArenaStart, freeBlocksAfter);

		freeBlocksAfter.Should().BeEquivalentTo(expectedFreeBlocksAfter, options => options.WithStrictOrdering());
	}

	[Test]
	public void ResizeAllocation_should_make_non_last_block_smaller()
	{
		// Arrange
		var machine = new Machine();

		var systemMemory = machine.SystemMemory;

		const int ArenaStart = 65536;
		const int ArenaSize = 131072;

		var sut = new MemoryManager(systemMemory, ArenaStart, ArenaSize);

		const int InitialAllocationSize = 1024;
		const int NewAllocationSize = 750;

		const int OwnerPSPSegment = 1;

		var allocationAddress = sut.AllocateMemory(InitialAllocationSize, OwnerPSPSegment);

		// Place a block after the one we're resizing
		sut.AllocateMemory(InitialAllocationSize, OwnerPSPSegment);

		// Act
		sut.ResizeAllocation(allocationAddress, NewAllocationSize, out int largestBlockSize);

		// Assert
		VerifyMemoryControlBlockChain(
			systemMemory,
			ArenaStart,
			ArenaSize,
			findBlockAddress: allocationAddress,
			allocationSize: NewAllocationSize,
			foundBlock: out bool foundBlock,
			foundCandidateAfterBlock: out _);

		foundBlock.Should().BeTrue();
	}

	[Test]
	public void ResizeAllocation_should_make_last_block_smaller()
	{
		// Arrange
		var machine = new Machine();

		var systemMemory = machine.SystemMemory;

		const int ArenaStart = 65536;
		const int ArenaSize = 131072;

		var sut = new MemoryManager(systemMemory, ArenaStart, ArenaSize);

		const int InitialAllocationSize = 1024;
		const int NewAllocationSize = 750;

		const int OwnerPSPSegment = 1;

		var allocationAddress = sut.AllocateMemory(InitialAllocationSize, OwnerPSPSegment);

		// Act
		sut.ResizeAllocation(allocationAddress, NewAllocationSize, out int largestBlockSize);

		// Assert
		VerifyMemoryControlBlockChain(
			systemMemory,
			ArenaStart,
			ArenaSize,
			findBlockAddress: allocationAddress,
			allocationSize: NewAllocationSize,
			foundBlock: out bool foundBlock,
			foundCandidateAfterBlock: out _);

		foundBlock.Should().BeTrue();
	}

	[Test]
	public void ResizeAllocation_should_do_nothing_if_block_size_is_unchanged()
	{
		// Arrange
		var machine = new Machine();

		var systemMemory = machine.SystemMemory;

		const int ArenaStart = 65536;
		const int ArenaSize = 131072;

		var sut = new MemoryManager(systemMemory, ArenaStart, ArenaSize);

		const int AllocationSize = 1024;

		const int OwnerPSPSegment = 1;

		var allocationAddress = sut.AllocateMemory(AllocationSize, OwnerPSPSegment);

		var blocksBefore = new List<(int Start, int Size, int Owner)>();
		var blocksAfter = new List<(int Start, int Size, int Owner)>();

		// Act
		CollectMemoryControlBlockChain(systemMemory, ArenaStart, blocksBefore);

		sut.ResizeAllocation(allocationAddress, AllocationSize, out int largestBlockSize);

		CollectMemoryControlBlockChain(systemMemory, ArenaStart, blocksAfter);

		// Assert
		VerifyMemoryControlBlockChain(
			systemMemory,
			ArenaStart,
			ArenaSize,
			findBlockAddress: allocationAddress,
			allocationSize: AllocationSize,
			foundBlock: out bool foundBlock,
			foundCandidateAfterBlock: out _);

		foundBlock.Should().BeTrue();

		blocksBefore.Should().BeEquivalentTo(blocksAfter, options => options.WithStrictOrdering());
	}

	[Test]
	public void ResizeAllocation_should_make_block_larger()
	{
		// Arrange
		var machine = new Machine();

		var systemMemory = machine.SystemMemory;

		const int ArenaStart = 65536;
		const int ArenaSize = 131072;

		var sut = new MemoryManager(systemMemory, ArenaStart, ArenaSize);

		const int InitialAllocationSize = 1024;
		const int NewAllocationSize = 2048;

		const int OwnerPSPSegment = 1;

		var allocationAddress = sut.AllocateMemory(InitialAllocationSize, OwnerPSPSegment);

		// Act
		sut.ResizeAllocation(allocationAddress, NewAllocationSize, out int largestBlockSize);

		// Assert
		VerifyMemoryControlBlockChain(
			systemMemory,
			ArenaStart,
			ArenaSize,
			findBlockAddress: allocationAddress,
			allocationSize: NewAllocationSize,
			foundBlock: out bool foundBlock,
			foundCandidateAfterBlock: out _);

		foundBlock.Should().BeTrue();
	}

	[Test]
	public void ResizeAllocation_should_throw_correct_error_if_new_size_is_invalid()
	{
		// Arrange
		var machine = new Machine();

		var systemMemory = machine.SystemMemory;

		const int ArenaStart = 65536;
		const int ArenaSize = 131072;

		var sut = new MemoryManager(systemMemory, ArenaStart, ArenaSize);

		const int AllocationSize = 1024;

		const int OwnerPSPSegment = 1;

		var allocationAddress = sut.AllocateMemory(AllocationSize, OwnerPSPSegment);

		// Act & Assert
		Action action = () => sut.ResizeAllocation(allocationAddress, newSize: -1, out _);

		action.Should().Throw<ArgumentOutOfRangeException>();
	}

	[Test]
	public void ResizeAllocation_should_throw_correct_error_if_insufficient_space_for_expansion([Values] bool nextBlockIsFree)
	{
		// Arrange
		var machine = new Machine();

		var systemMemory = machine.SystemMemory;

		const int ArenaStart = 65536;
		const int ArenaSize = 131072;

		var sut = new MemoryManager(systemMemory, ArenaStart, ArenaSize);

		const int InitialAllocationSize = 1024;
		const int NewAllocationSize = 2048;
		const int BlockExpansionSize = 512;

		const int OwnerPSPSegment = 1;

		var allocationAddress = sut.AllocateMemory(InitialAllocationSize, OwnerPSPSegment);

		var blockExpansionAddress = sut.AllocateMemory(BlockExpansionSize, OwnerPSPSegment);

		if (nextBlockIsFree)
		{
			int freeButNotLargeEnoughAddress = blockExpansionAddress;

			blockExpansionAddress = sut.AllocateMemory(BlockExpansionSize, OwnerPSPSegment);

			sut.FreeMemory(freeButNotLargeEnoughAddress);
		}

		// Act & Assert
		Action action = () => sut.ResizeAllocation(allocationAddress, NewAllocationSize, out int largestBlockSize);

		action.Should().Throw<DOSException>().Which.Error.Should().Be(DOSError.NotEnoughMemory);
	}

	[Test]
	public void ResizeAllocation_should_throw_correct_error_if_supplied_address_is_not_valid()
	{
		// Arrange
		var machine = new Machine();

		var systemMemory = machine.SystemMemory;

		const int ArenaStart = 65536;
		const int ArenaSize = 131072;

		var sut = new MemoryManager(systemMemory, ArenaStart, ArenaSize);

		const int AllocationSize = 1024;

		const int OwnerPSPSegment = 1;

		var allocationAddress = sut.AllocateMemory(AllocationSize, OwnerPSPSegment);

		int invalidAddress = allocationAddress + AllocationSize / 2;

		// Act & Assert
		Action action = () => sut.ResizeAllocation(invalidAddress, newSize: AllocationSize, out _);

		action.Should().Throw<DOSException>().Which.Error.Should().Be(DOSError.InvalidBlock);
	}

	[Test]
	public void FreeMemory_should_free_first_block()
	{
		// Arrange
		var machine = new Machine();

		var systemMemory = machine.SystemMemory;

		const int ArenaStart = 65536;
		const int ArenaSize = 131072;

		var sut = new MemoryManager(systemMemory, ArenaStart, ArenaSize);

		// MemoryManager allocates some of its own memory. We're going to be naughty and free the
		// first allocated block, even though we didn't allocate it.
		int firstAllocationAddress = ArenaStart + 16;

		var freeBlocksBefore = new List<(int Start, int Size)>();
		var freeBlocksAfter = new List<(int Start, int Size)>();

		CollectFreeBlocks(systemMemory, ArenaStart, freeBlocksBefore);

		var expectedFreeBlocksAfter = new List<(int Start, int Size)>(freeBlocksBefore);

		ref var mcb = ref MemoryControlBlock.CreateReference(systemMemory, ArenaStart);

		expectedFreeBlocksAfter.Insert(0, (ArenaStart, mcb.SizeInBytes + MemoryManager.ParagraphSize));

		// Act
		sut.FreeMemory(firstAllocationAddress);

		// Assert
		CollectFreeBlocks(systemMemory, ArenaStart, freeBlocksAfter);

		VerifyMemoryControlBlockChain(
			systemMemory,
			ArenaStart,
			ArenaSize,
			-1, -1, out _, out _);

		freeBlocksAfter.Should().BeEquivalentTo(expectedFreeBlocksAfter, options => options.WithStrictOrdering());
	}

	[Test]
	public void FreeMemory_should_free_last_block()
	{
		// Arrange
		var machine = new Machine();

		var systemMemory = machine.SystemMemory;

		const int ArenaStart = 65536;
		const int ArenaSize = 131072;

		var sut = new MemoryManager(systemMemory, ArenaStart, ArenaSize);

		sut.AllocationStrategy = MemoryAllocationStrategy.LastFit;

		const int OwnerPSPSegment = 1;

		const int AllocationSize = 3000;

		int allocationAddress = sut.AllocateMemory(AllocationSize, OwnerPSPSegment);

		var freeBlocksBefore = new List<(int Start, int Size)>();
		var freeBlocksAfter = new List<(int Start, int Size)>();

		CollectFreeBlocks(systemMemory, ArenaStart, freeBlocksBefore);

		var expectedFreeBlocksAfter = new List<(int Start, int Size)>(freeBlocksBefore);

		int allocatedBlockAddress = allocationAddress - MemoryManager.ParagraphSize;

		ref var mcb = ref MemoryControlBlock.CreateReference(systemMemory, allocatedBlockAddress);

		expectedFreeBlocksAfter.Add((allocatedBlockAddress, mcb.SizeInBytes + MemoryManager.ParagraphSize));

		// Act
		sut.FreeMemory(allocationAddress);

		// Assert
		CollectFreeBlocks(systemMemory, ArenaStart, freeBlocksAfter);

		VerifyMemoryControlBlockChain(
			systemMemory,
			ArenaStart,
			ArenaSize,
			-1, -1, out _, out _);

		freeBlocksAfter.Should().BeEquivalentTo(expectedFreeBlocksAfter, options => options.WithStrictOrdering());
	}

	[Test]
	public void FreeMemory_should_free_middle_block()
	{
		// Arrange
		var machine = new Machine();

		var systemMemory = machine.SystemMemory;

		const int ArenaStart = 65536;
		const int ArenaSize = 131072;

		var sut = new MemoryManager(systemMemory, ArenaStart, ArenaSize);

		sut.AllocationStrategy = MemoryAllocationStrategy.LastFit;

		// We want to observe a free block being added to the middle. So,
		// we arrange allocated blocks as follows:
		//
		// [0] Free
		// [1] Allocated
		// [2] Allocated <-- will free this one
		// [3] Allocated
		// [4] Free (implicit remaining space)

		const int OwnerPSPSegment = 1;

		const int AllocationSize = 3000;

		int block0 = sut.AllocateMemory(AllocationSize, OwnerPSPSegment);
		int block1 = sut.AllocateMemory(AllocationSize, OwnerPSPSegment);
		int block2 = sut.AllocateMemory(AllocationSize, OwnerPSPSegment);
		int block3 = sut.AllocateMemory(AllocationSize, OwnerPSPSegment);

		sut.FreeMemory(block0);

		int allocationAddress = block2;

		var freeBlocksBefore = new List<(int Start, int Size)>();
		var freeBlocksAfter = new List<(int Start, int Size)>();

		CollectFreeBlocks(systemMemory, ArenaStart, freeBlocksBefore);

		var expectedFreeBlocksAfter = new List<(int Start, int Size)>(freeBlocksBefore);

		int allocatedBlockAddress = allocationAddress - MemoryManager.ParagraphSize;

		ref var mcb = ref MemoryControlBlock.CreateReference(systemMemory, allocatedBlockAddress);

		expectedFreeBlocksAfter.Insert(1, (allocatedBlockAddress, mcb.SizeInBytes + MemoryManager.ParagraphSize));

		// Act
		sut.FreeMemory(allocationAddress);

		// Assert
		CollectFreeBlocks(systemMemory, ArenaStart, freeBlocksAfter);

		VerifyMemoryControlBlockChain(
			systemMemory,
			ArenaStart,
			ArenaSize,
			-1, -1, out _, out _);

		freeBlocksAfter.Should().BeEquivalentTo(expectedFreeBlocksAfter, options => options.WithStrictOrdering());
	}

	[Test]
	public void FreeMemory_should_throw_correct_error_if_supplied_address_is_not_valid()
	{
		// Arrange
		var machine = new Machine();

		var systemMemory = machine.SystemMemory;

		const int ArenaStart = 65536;
		const int ArenaSize = 131072;

		var sut = new MemoryManager(systemMemory, ArenaStart, ArenaSize);

		const int AllocationSize = 1024;

		const int OwnerPSPSegment = 1;

		var allocationAddress = sut.AllocateMemory(AllocationSize, OwnerPSPSegment);

		int invalidAddress = allocationAddress + AllocationSize / 2;

		// Act & Assert
		Action action = () => sut.FreeMemory(invalidAddress);

		action.Should().Throw<DOSException>().Which.Error.Should().Be(DOSError.InvalidBlock);
	}

	[Test]
	public void FreeMemory_should_throw_correct_error_if_supplied_address_is_previously_freed()
	{
		// Arrange
		var machine = new Machine();

		var systemMemory = machine.SystemMemory;

		const int ArenaStart = 65536;
		const int ArenaSize = 131072;

		var sut = new MemoryManager(systemMemory, ArenaStart, ArenaSize);

		const int AllocationSize = 1024;

		const int OwnerPSPSegment = 1;

		var allocationAddress = sut.AllocateMemory(AllocationSize, OwnerPSPSegment);

		sut.FreeMemory(allocationAddress);

		// Act & Assert
		Action action = () => sut.FreeMemory(allocationAddress);

		action.Should().Throw<DOSException>().Which.Error.Should().Be(DOSError.InvalidBlock);
	}

	[Test]
	public void CreatePSP_should_initialize_program_segment_prefix()
	{
		// Arrange
		var machine = new Machine();

		var systemMemory = machine.SystemMemory;

		const int ArenaStart = 65536;
		const int ArenaSize = 131072;

		var sut = new MemoryManager(systemMemory, ArenaStart, ArenaSize);

		var environment = new EnvironmentBlock();

		for (int i = 0; i < 10; i++)
			environment[Guid.NewGuid().ToString()] = Guid.NewGuid().ToString();
		for (int i = 0; i < 10; i++)
			environment[TestContext.CurrentContext.Random.Next(0, int.MaxValue).ToString()] = Guid.NewGuid().ToString();

		var commandLine = new StringValue();

		const string TestArg1 = "TEST.TXT";
		const string TestArg2 = "FOO.BIN";

		commandLine.Append(TestArg1).Append(' ');
		commandLine.Append(TestArg2).Append(' ');
		for (int i = 0; i < 75; i++)
			commandLine.Append((byte)TestContext.CurrentContext.Random.Next(32, 126));

		// Act
		var pspSegment = sut.CreatePSP(environment, commandLine);

		// Assert
		var pspAddress = new SegmentedAddress(pspSegment, 0);

		ref var psp = ref ProgramSegmentPrefix.CreateReference(systemMemory, pspAddress.ToLinearAddress());

		psp.Int20hInstruction.Should().Be(ProgramSegmentPrefix.Int20hInstructionValue);

		FileControlBlock.GetFileName(psp.FCB1.FileNameBytes).Should().Be(TestArg1);
		FileControlBlock.GetFileName(psp.FCB2.FileNameBytes).Should().Be(TestArg2);

		ReadOnlySpan<byte> commandLineSpan = psp.CommandLine;

		commandLineSpan.Slice(0, psp.CommandLineLength).ShouldMatch(commandLine.AsSpan());
		commandLineSpan[psp.CommandLineLength].Should().Be((byte)'\r');

		var environmentAddress = new SegmentedAddress(psp.EnvironmentSegment);

		var actualEnvironment = new EnvironmentBlock();

		DOSError localLastError = DOSError.None;

		EnvironmentBlock.DecodeTo(
			systemMemory,
			environmentAddress.ToLinearAddress(),
			(memory, address) => DOS.ReadStringZ(memory, address, ref localLastError),
			() => localLastError,
			actualEnvironment);
	}

	static void VerifyMemoryControlBlockChain(SystemMemory systemMemory, int arenaStart, int arenaSize, int findBlockAddress, int allocationSize, out bool foundBlock, out bool foundCandidateAfterBlock)
	{
		foundBlock = false;
		foundCandidateAfterBlock = false;

		int offset = arenaStart;
		int arenaEnd = arenaStart + arenaSize;

		while (true)
		{
			if (offset >= arenaEnd)
				throw new Exception("Memory Control Block chain is not terminated properly (walked off the end of managed memory)");

			ref var mcb = ref MemoryControlBlock.CreateReference(systemMemory, offset);

			mcb.HasValidType.Should().BeTrue();

			int allocationAddress = offset + MemoryManager.ParagraphSize;

			if (allocationAddress == findBlockAddress)
			{
				foundBlock = true;
				mcb.SizeInBytes.Should().BeInRange(
					allocationSize,
					allocationSize + 2 * MemoryManager.ParagraphSize - 1); // block can't be split without a full 2 paragraphs following
			}
			else if (foundBlock && (mcb.SizeInBytes >= allocationSize))
				foundCandidateAfterBlock = true;

			offset = offset + MemoryManager.ParagraphSize + mcb.SizeInBytes;

			if (mcb.Type == QBX.OperatingSystem.MemoryControlBlockType.LastNode)
				break;
		}

		offset.Should().Be(arenaEnd);
	}

	void CollectMemoryControlBlockChain(SystemMemory systemMemory, int arenaStart, List<(int Start, int Size, int Owner)> blocks)
	{
		int offset = arenaStart;

		while (true)
		{
			ref var mcb = ref MemoryControlBlock.CreateReference(systemMemory, offset);

			mcb.HasValidType.Should().BeTrue();

			blocks.Add((offset, mcb.SizeInBytes + MemoryManager.ParagraphSize, mcb.OwnerPSPSegment));

			offset = offset + MemoryManager.ParagraphSize + mcb.SizeInBytes;

			if (mcb.Type == QBX.OperatingSystem.MemoryControlBlockType.LastNode)
				break;
		}
	}

	void CollectFreeBlocks(SystemMemory systemMemory, int arenaStart, List<(int Start, int Size)> freeBlocks)
	{
		int offset = arenaStart;

		while (true)
		{
			ref var mcb = ref MemoryControlBlock.CreateReference(systemMemory, offset);

			mcb.HasValidType.Should().BeTrue();

			if (mcb.OwnerPSPSegment == MemoryControlBlock.FreeBlockOwner)
				freeBlocks.Add((offset, mcb.SizeInBytes + MemoryManager.ParagraphSize));

			offset = offset + MemoryManager.ParagraphSize + mcb.SizeInBytes;

			if (mcb.Type == QBX.OperatingSystem.MemoryControlBlockType.LastNode)
				break;
		}
	}

}
