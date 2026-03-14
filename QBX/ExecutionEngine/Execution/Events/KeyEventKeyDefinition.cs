using System;

using QBX.Hardware;

namespace QBX.ExecutionEngine.Execution.Events;

public struct KeyEventKeyDefinition : IEquatable<KeyEventKeyDefinition>
{
	public KeyEventKeyModifiers Modifiers;
	public ScanCode ScanCode;

	public KeyEventKeyDefinition()
	{
	}

	public KeyEventKeyDefinition(KeyEventKeyModifiers modifiers, ScanCode scanCode)
	{
		Modifiers = modifiers;
		ScanCode = scanCode;
	}

	public bool Equals(KeyEventKeyDefinition other)
	{
		return
			(Modifiers == other.Modifiers) &&
			(ScanCode == other.ScanCode);
	}

	public override bool Equals(object? obj)
		=> (obj is KeyEventKeyDefinition other) && Equals(other);
	public override int GetHashCode()
		=> (Modifiers.GetHashCode() << 256) ^ ScanCode.GetHashCode();

	public static bool operator ==(KeyEventKeyDefinition left, KeyEventKeyDefinition right)
		=> left.Equals(right);
	public static bool operator !=(KeyEventKeyDefinition left, KeyEventKeyDefinition right)
		=> !left.Equals(right);
}

