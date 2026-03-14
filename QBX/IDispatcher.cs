using System;

namespace QBX;

public interface IDispatcher
{
	void Dispatch(Action action);
}
