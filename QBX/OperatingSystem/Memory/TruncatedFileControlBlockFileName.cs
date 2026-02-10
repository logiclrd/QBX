using System;
using System.Runtime.CompilerServices;

namespace QBX.OperatingSystem.Memory;

[InlineArray(length: 11)]
public struct TruncatedFileControlBlockFileName
{
	private byte _element0;

	public int Length => 11;

	public void Clear()
	{
		((Span<byte>)this).Clear();
	}

	public void Set(ReadOnlySpan<byte> bytes)
	{
		if (bytes.Length > Length)
			bytes = bytes.Slice(0, Length);

		Clear();
		bytes.CopyTo(this);
	}
}
