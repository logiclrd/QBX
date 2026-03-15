using System;

namespace QBX.ExecutionEngine.Execution;

public class ArraySubscript : IEquatable<ArraySubscript>
{
	public short LowerBound;
	public short UpperBound;

	public int ElementCount => UpperBound - LowerBound + 1;

	public override bool Equals(object? obj)
		=> Equals(obj as ArraySubscript);

	public bool Equals(ArraySubscript? other)
	{
		if (other == null)
			return false;

		return
			(LowerBound == other.LowerBound) &&
			(UpperBound == other.UpperBound);
	}

	public override int GetHashCode()
	{
		return unchecked(LowerBound ^ (UpperBound * 1019));
	}
}
