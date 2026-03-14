using System;
using System.Diagnostics.CodeAnalysis;

namespace QBX.ExecutionEngine.Execution.Events;

public struct Event(EventType type, int source = -1) : IEquatable<Event>
{
	public EventType Type => type;
	public int Source => source;

	public bool Equals(Event other)
	{
		return
			(Type == other.Type) &&
			(Source == other.Source);
	}

	public override bool Equals([NotNullWhen(true)] object? obj)
		=> (obj is Event evt) && Equals(evt);
	public override int GetHashCode()
		=> (Type.GetHashCode() << 8) ^ Source.GetHashCode();

	public static bool operator ==(Event left, Event right)
		=> left.Equals(right);
	public static bool operator !=(Event left, Event right)
		=> !left.Equals(right);
}

