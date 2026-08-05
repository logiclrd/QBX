using System.Collections.Generic;

namespace QBX.ExecutionEngine.Compiled;

public class RoutineFacade(Routine routine)
{
	public Routine Routine => routine;
	public List<ParameterDefinition> ParameterDefinitions = new List<ParameterDefinition>();
}
