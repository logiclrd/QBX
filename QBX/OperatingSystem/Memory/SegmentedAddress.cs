using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace QBX.OperatingSystem.Memory;

[StructLayout(LayoutKind.Explicit)]
public struct SegmentedAddress
{
	[FieldOffset(2)] public ushort Segment;
	[FieldOffset(0)] public ushort Offset;

	public SegmentedAddress() { }

	public SegmentedAddress(ushort segment, ushort offset)
	{
		Segment = segment;
		Offset = offset;
	}

	public SegmentedAddress(int linearAddress)
	{
		unchecked
		{
			Segment = (ushort)(linearAddress >> 4);
			Offset = (ushort)(linearAddress & 15);
		}
	}

	public SegmentedAddress(uint linearAddress)
	{
		unchecked
		{
			Segment = (ushort)(linearAddress >> 4);
			Offset = (ushort)(linearAddress & 15);
		}
	}

	public int ToLinearAddress() => (Segment << 4) + Offset;

	public static bool operator ==(SegmentedAddress left, SegmentedAddress right)
		=> left.ToLinearAddress() == right.ToLinearAddress();
	public static bool operator !=(SegmentedAddress left, SegmentedAddress right)
		=> left.ToLinearAddress() != right.ToLinearAddress();

	public static bool operator ==(SegmentedAddress left, int right)
		=> left.ToLinearAddress() == right;
	public static bool operator !=(SegmentedAddress left, int right)
		=> left.ToLinearAddress() != right;

	public override bool Equals([NotNullWhen(true)] object? obj)
	{
		if (obj is SegmentedAddress other)
			return this == other;
		else if (obj is int linearAddress)
			return this == linearAddress;
		else
			return false;
	}

	public override int GetHashCode() => ToLinearAddress().GetHashCode();

	public static implicit operator SegmentedAddress(int linearAddress)
		=> new SegmentedAddress(linearAddress);
}
