using System;

namespace QBX.ExecutionEngine.Marshalling;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Parameter)]
public class FixedLengthAttribute(int length) : Attribute
{
	public int Length => length;
}
