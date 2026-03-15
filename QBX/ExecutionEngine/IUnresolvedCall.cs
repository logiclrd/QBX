using QBX.ExecutionEngine.Compiled;
using QBX.LexicalAnalysis;

namespace QBX.ExecutionEngine;

public interface IUnresolvedCall
{
	Token? SourceToken { get; }

	void Resolve(Routine routine);
}
