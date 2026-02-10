using System;
using System.Runtime.CompilerServices;

using QBX.Firmware.Fonts;

namespace QBX.OperatingSystem.Memory;

[InlineArray(length: 8)]
public struct MemoryControlBlockProgramName
{
	private byte _element0;

	static CP437Encoding s_cp437 = new CP437Encoding(ControlCharacterInterpretation.Semantic);

	public void Clear()
	{
		var span = (Span<byte>)this;

		span.Clear();
	}

	public override string ToString()
	{
		var span = (Span<byte>)this;

		int endOfString = span.IndexOf((byte)0);

		if (endOfString < 0)
			endOfString = span.Length;

		return s_cp437.GetString(span.Slice(0, endOfString));
	}

	public void Set(ReadOnlySpan<byte> bytes)
	{
		if (bytes.Length > 7)
			bytes = bytes.Slice(0, 7);

		Clear();
		bytes.CopyTo(this);
	}
}
