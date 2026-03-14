using QBX.ExecutionEngine.Execution.Events;

namespace QBX.Tests.ExecutionEngine;

public class DummyEventHub() : EventHub(new DummyDispatcher())
{
	class DummyDispatcher : IDispatcher
	{
		public void Dispatch(Action action) => action();
	}
}
