using System;
using System.Diagnostics.CodeAnalysis;

namespace QBX.Parser;

public class Identifier(string value, IdentifierRepository repository) : IEquatable<Identifier>
{
	string BareValue => value;

	public IdentifierRepository Repository => repository;
	public virtual string Value => value;

	public static readonly Identifier Empty = Identifier.Standalone("");

	public void SetValue(string newValue)
	{
		if (!string.Equals(value, newValue, StringComparison.OrdinalIgnoreCase))
			throw new Exception("Cannot change an Identifier from " + value + " to " + newValue + " because these are not equivalent identifiers");

		value = newValue;
	}

	public static Identifier Standalone(string value)
		=> new Identifier(value, new IdentifierRepository());

	public override bool Equals(object? obj)
	{
		return Equals(obj as Identifier);
	}

	public bool Equals(Identifier? other)
	{
		return (other != null) && string.Equals(value, other.BareValue, StringComparison.OrdinalIgnoreCase);
	}

	public override int GetHashCode()
	{
		return StringComparer.OrdinalIgnoreCase.GetHashCode(value);
	}

	public override string ToString() => value;

	[return: NotNullIfNotNull(nameof(identifier))]
	public static implicit operator string?(Identifier? identifier) => identifier?.Value;

	public static bool operator ==(Identifier? left, Identifier? right)
	{
		if ((left is null) && (right is null))
			return true;

		if ((left is null) || (right is null))
			return false;

		return left.Equals(right);
	}

	public static bool operator !=(Identifier? left, Identifier? right)
		=> !(left == right);
}
