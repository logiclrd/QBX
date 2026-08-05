using System;

namespace QBX.ExecutionEngine.Compiled;

public class ParameterDefinition(ParameterRepresentation representation, DataType type) : IEquatable<ParameterDefinition>
{
	public ParameterRepresentation Representation = representation;
	public DataType Type = type;

	public ParameterDefinition(CodeModel.ParameterDefinition sourceDefinition, DataType resolvedType)
		: this(
				sourceDefinition.Representation.ToExecutionEngineType(),
				resolvedType)
	{
	}

	public override bool Equals(object? obj)
		=> Equals(obj as ParameterDefinition);

	public override int GetHashCode()
		=> Representation.GetHashCode() ^ Type.GetHashCode();

	public bool Equals(ParameterDefinition? other)
	{
		return
			(other != null) &&
			(Representation == other.Representation) &&
			Type.Equals(other.Type);
	}
}
