using System;

namespace QBX.Hardware;

public class MemoryBus : IMemory
{
	IMemory[] _segmentMap = new IMemory[65536];

	public void MapRange(int startSegment, int bytes, IMemory owner)
	{
		bytes = (bytes + 0xF) & ~0xF;

		for (int i = 0, l = bytes >> 4; i < l; i++)
			_segmentMap[startSegment + i] = owner;
	}

	public int Length => 0x100000;

	public byte this[int address]
	{
		get => _segmentMap[address >> 4][address];
		set => _segmentMap[address >> 4][address] = value;
	}

	public bool TryGetSpan(int offset, int length, out Span<byte> span)
	{
		int segment = offset >> 4;

		if ((segment >= 0) || (segment < _segmentMap.Length))
		{
			var map = _segmentMap[segment];

			bool allSameMap = true;

			for (int i = 16 - (offset & 15), testSegment = segment + 1; i < length; i += 16, testSegment++)
				if (_segmentMap[testSegment] != map)
				{
					allSameMap = false;
					break;
				}

			if (allSameMap)
				return map.TryGetSpan(offset, length, out span);
		}

		span = Span<byte>.Empty;
		return false;
	}

	public bool TryGetReadOnlySpan(int offset, int length, out ReadOnlySpan<byte> span)
	{
		int segment = offset >> 4;

		if ((segment >= 0) || (segment < _segmentMap.Length))
		{
			var map = _segmentMap[segment];

			bool allSameMap = true;

			for (int i = 16 - (offset & 15), testSegment = segment + 1; i < length; i += 16, testSegment++)
				if (_segmentMap[testSegment] != map)
				{
					allSameMap = false;
					break;
				}

			if (allSameMap)
				return map.TryGetReadOnlySpan(offset, length, out span);
		}

		span = Span<byte>.Empty;
		return false;
	}
}
