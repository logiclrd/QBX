using System.Collections.Generic;

namespace QBX.ExecutionEngine.Compiled;

public interface IHasTypedParameters
{
	IList<Evaluable> Arguments { get; }
	void EnsureParameterTypes(IReadOnlyList<ParameterDefinition>? parameterDefinitions, bool matchFacades);
}
