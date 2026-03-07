using System;

namespace QBX.Hardware;

public interface IMemory
{
	int Length { get; }
	byte this[int index] { get; set; }

	bool TryGetSpan(int offset, int length, out Span<byte> span);
}
