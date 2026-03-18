using System.Collections.Generic;

using QBX.CodeModel;

namespace QBX.Parser;

public class IdentifierRepository
{
	Dictionary<Identifier, Identifier> _canonicalIdentifiers = new();

	public Identifier GetOrAddCanonicalIdentifier(string identifier)
	{
		if (TypeCharacter.TryParse(identifier[identifier.Length - 1], out var typeCharacter))
		{
			string unqualifiedIdentifier = identifier.Substring(0, identifier.Length - 1);

			var unqualifiedCanonicalIdentifier = GetOrAddCanonicalIdentifier(new Identifier(unqualifiedIdentifier, this));

			return new QualifiedIdentifier(unqualifiedCanonicalIdentifier, typeCharacter);
		}
		else
			return GetOrAddCanonicalIdentifier(new Identifier(identifier, this));
	}

	public Identifier GetOrAddCanonicalIdentifier(Identifier identifier)
	{
		if (_canonicalIdentifiers.TryGetValue(identifier, out var existing))
		{
			if (identifier is QualifiedIdentifier qualifiedIdentifier)
				return new QualifiedIdentifier(existing, qualifiedIdentifier.TypeCharacter);
			else
				return existing;
		}

		return _canonicalIdentifiers[identifier] = identifier;
	}

	public Identifier UpdateCanonicalIdentifier(string identifier)
	{
		if (TypeCharacter.TryParse(identifier[identifier.Length - 1], out var typeCharacter))
		{
			string unqualifiedIdentifier = identifier.Substring(0, identifier.Length - 1);

			var unqualifiedCanonicalIdentifier = UpdateCanonicalIdentifier(new Identifier(unqualifiedIdentifier, this));

			return new QualifiedIdentifier(unqualifiedCanonicalIdentifier, typeCharacter);
		}
		else
			return UpdateCanonicalIdentifier(new Identifier(identifier, this));
	}

	public Identifier UpdateCanonicalIdentifier(Identifier identifier)
	{
		if (_canonicalIdentifiers.TryGetValue(identifier, out var existing))
		{
			existing.SetValue(identifier.Value);

			if (identifier is QualifiedIdentifier qualifiedIdentifier)
				return new QualifiedIdentifier(existing, qualifiedIdentifier.TypeCharacter);
			else
				return existing;
		}

		return _canonicalIdentifiers[identifier] = identifier;
	}
}
