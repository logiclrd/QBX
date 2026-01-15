using QBX.ExecutionEngine.Compiled;

namespace QBX.ExecutionEngine;

public interface IUnresolvedLineReference
{
	void Resolve(Routine routine);
}
