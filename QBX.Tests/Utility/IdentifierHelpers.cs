using System.Diagnostics.CodeAnalysis;

using QBX.Parser;

namespace QBX.Tests.Utility;

public static class IdentifierHelpers
{
	[return: NotNullIfNotNull(nameof(name))]
	public static Identifier? ID(string? name) => (name == null) ? null : Identifier.Standalone(name);
}
