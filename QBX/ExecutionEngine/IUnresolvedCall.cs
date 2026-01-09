using QBX.ExecutionEngine.Compiled;

namespace QBX.ExecutionEngine;

public interface IUnresolvedCall
{
	void Resolve(Routine routine);
}
